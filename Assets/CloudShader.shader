Shader "Cloud"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            // Step size can't go below this otherwise risk crashing
            #define MIN_STEP_SIZE .2
            
            #pragma region Structs

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD1;
            };
            
            #pragma endregion 

            #pragma region Uniforms

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            
            float4 _Color;
            float4 _MainTex_ST; // Unity specific

            float3 _BoundsMin, _BoundsMax;

            // 3D Tex Sampling
            float _StepSize;
            
            Texture3D<float4> _3DTex;
            SamplerState sampler_3DTex;
            
            float _DensityThreshold, _DensityMultiplier;
            
            float _CloudScale;
            float3 _CloudOffset;

            #pragma endregion
            
            #pragma region Helper Functions

            // Ray-Box intersection edited from: http://jcgt.org/published/0007/03/04/ 
            // Float[0] = Distance to start of box. Float[1] = Distance to back of box
            // (This article is simple but awesome, and might  be helpful for Tech Art)
            float2 rayBoxDist(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir) {
                float3 t0 = (boundsMin - rayOrigin) * invRaydir;
                float3 t1 = (boundsMax - rayOrigin) * invRaydir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                
                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(tmax.x, min(tmax.y, tmax.z));

                float distToBox = max(0, dstA);
                float distInsideBox = max(0, dstB - distToBox);
                
                return float2(distToBox, distInsideBox);
            }

            /// <summary>
            /// Returns density (red channel) of 3D Tex uniform, at Pos.
            /// </summary>
            float sampleDensity(float3 samplePos)
            {
                float3 uvw = samplePos * _CloudScale + _CloudOffset;
                float4 noise = _3DTex.SampleLevel(sampler_3DTex, uvw.xyz, 0.0f);
                float density = max(0, noise.r - _DensityThreshold) * _DensityMultiplier;
                return density;
            }

            /// <summary>
            /// 
            /// </summary>
            float densityRayMarch(float3 startPos, float3 dir, float distLimit, float stepSize) {
                float totalDensity = 0;
                float dstTravelled = 0;
                
                while (dstTravelled < distLimit)
                {
                    float3 samplePosition = startPos + dir * dstTravelled;
                    
                    totalDensity += sampleDensity(samplePosition) * _StepSize;
                    
                    dstTravelled += _StepSize;
                }

                return totalDensity;
            }
            
            #pragma  endregion
            
            #pragma region Shaders
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Get screen
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Remap UVs [0,1] -> [-1,1]
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                o.viewDir = mul(unity_CameraToWorld, float4(viewVector, 0));

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 clr = tex2D(_MainTex, i.uv);

                // Ray setup from camera
                float3 rayStart = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewDir);
                
                // Determine if Frag is in bounding box
                float2 rayBoxInfo = rayBoxDist(_BoundsMin, _BoundsMax, rayStart, 1/rayDir);
                float distToBox = rayBoxInfo.x;
                float distInsideBox = rayBoxInfo.y;
                
                // Don't over draw
                float depthLinear = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv)) * length(i.viewDir);
                
                bool rayInBounds = distInsideBox > 0 && distToBox < depthLinear;
                
                if(!rayInBounds)
                    return clr;

                // Do the thing!
                float distLimit = min(depthLinear - distToBox, distInsideBox);
                float3 rayBoxEntryPos = rayStart + rayDir * distToBox;
                _StepSize = max(MIN_STEP_SIZE, _StepSize);

                float totalDensity = densityRayMarch(rayBoxEntryPos, rayDir, distLimit, _StepSize);
                
                float transmittence = exp(-totalDensity);

                //return lerp(_Color, clr, transmittence);
                return clr * transmittence;
            }

            #pragma endregion
            
            ENDCG
        }
    }
}
