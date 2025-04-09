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

            // Will be useful later
            Texture3D<float4> _3DTex;
            SamplerState sampler_3DTex;
            float _Alpha;
            float _StepSize;

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

                // Don't over draw
                float depthLinear = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv)) * length(i.viewDir);
                //return depthLinear;
                
                // Determine if Frag is in bounding box
                float2 rayBoxInfo = rayBoxDist(_BoundsMin, _BoundsMax, rayStart, 1/rayDir);
                float distToBox = rayBoxInfo.x;
                float distInsideBox = rayBoxInfo.y;
                
                bool rayInBounds = distInsideBox > 0 && distToBox < depthLinear;
                
                if(rayInBounds)
                    return _Color;

                return clr;
            }

            #pragma endregion
            
            ENDCG
        }
    }
}
