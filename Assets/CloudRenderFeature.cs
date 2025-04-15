using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloudRenderFeature : ScriptableRendererFeature
{
    /// <summary>
    /// Editing shader settings
    /// </summary>
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public Color color = new Color(.5f, .5f, .5f, 1);
        
        public Vector3 boundsMin = new Vector3(-5, -5, -5), boundsMax = new Vector3(5, 5, 5);
        public float stepSize = .1f;
        
        public Texture3D tex3D;

        [Header("Noise Settings")] 
        
        [Range(0,1)] public float cloudScale = 1;
        public Vector3 cloudOffset = Vector3.zero;
        
        [Range(0,1)] public float densityThreshold = 0;
        [Min(0)] public float densityMultiplier = 1;

        [Range(0,16)] public int lightSteps = 16;

        public float lightAbsToSun, darkThresh;
    }

    /// <summary>
    /// Setup pass and set Uniforms
    /// </summary>
    class CloudPass : ScriptableRenderPass
    {
        public Settings settings;
        private RenderTargetIdentifier source;
        RenderTargetHandle tempTexture;
        private string profilerTag;

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public CloudPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
            ConfigureTarget(tempTexture.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            cmd.Clear();

            if (settings.material == null) return;

            settings.material.SetColor("_Color", settings.color);
            settings.material.SetVector("_BoundsMin", settings.boundsMin);
            settings.material.SetVector("_BoundsMax", settings.boundsMax);
            settings.material.SetTexture("_3DTex", settings.tex3D);
            settings.material.SetFloat("_StepSize", settings.stepSize);
            
            settings.material.SetFloat("_CloudScale", settings.cloudScale);
            settings.material.SetVector("_CloudOffset", settings.cloudOffset);
            
            settings.material.SetFloat("_DensityThreshold", settings.densityThreshold);
            settings.material.SetFloat("_DensityMultiplier", settings.densityMultiplier);
            
            settings.material.SetInt("_LightSteps", settings.lightSteps);
            settings.material.SetFloat("_LightAbsorbtionTowardsSun", settings.lightAbsToSun);
            settings.material.SetFloat("_DarknessThreshold", settings.darkThresh);
            
            cmd.Blit(source, tempTexture.Identifier());
            cmd.Blit(tempTexture.Identifier(), source, settings.material, 0);

            context.ExecuteCommandBuffer(cmd);
            
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
    
    public Settings settings = new Settings();

    private CloudPass pass;

    public override void Create()
    {
        pass = new CloudPass("Cloud Pass");
        name = "Cloud Pass";
        pass.settings = settings;
        pass.renderPassEvent = settings.renderPassEvent;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var cameraColorTargetIdent = renderer.cameraColorTarget;
        pass.Setup(cameraColorTargetIdent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }

}
