using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public enum Denoising
{
    Gaussian,
    Bilateral
}


public class GaussianFiltering : ScriptableRendererFeature
{
    [Serializable]
    public class ConfigSetting
    {
        public Denoising denoisingType = Denoising.Gaussian;
        public FilterMode FilterMode;
        public RenderPassEvent passEvent;
    }

    [Serializable]
    public class Settings
    {
        [Range(0,30)]
        public int iteration;
        [Range(1f, 10f)]
        public float radius;
        [Range(0, 35)]
        public int sigma;
        [Range(0f, 0.99f)]
        public float threshold;

        public ConfigSetting config;
    }

    public Settings settings;

    private static readonly int params_id = Shader.PropertyToID("_Params");
    private static readonly int DenoisingBuffer_id_1 = Shader.PropertyToID("DenoisingBuffer_1");
    private static readonly int DenoisingBuffer_id_2 = Shader.PropertyToID("DenoisingBuffer_2");
    class GaussianFilteringRenderPass : ScriptableRenderPass
    {
        Settings m_settings;
        private Material m_Material;
        public GaussianFilteringRenderPass(Settings settings)
        {
            m_settings = settings;
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor cameraData = renderingData.cameraData.cameraTargetDescriptor;

            cmd.GetTemporaryRT(DenoisingBuffer_id_1, cameraData.width, cameraData.height, 0, m_settings.config.FilterMode);
            cmd.GetTemporaryRT(DenoisingBuffer_id_2, cameraData.width, cameraData.height, 0, m_settings.config.FilterMode);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("ImageSharpening");
            switch(m_settings.config.denoisingType)
            {
                case Denoising.Gaussian:
                    m_Material = CoreUtils.CreateEngineMaterial(Shader.Find("Pineapple/GaussianFiltering"));
                    Gaussian_Render(cmd,context,ref renderingData);
                    break;
                case Denoising.Bilateral:
                    m_Material = CoreUtils.CreateEngineMaterial(Shader.Find("Pineapple/GaussianFiltering"));
                    Bilateral_Render(cmd, context,ref renderingData);
                    break;
            }
           

            

        }
        void Gaussian_Render(CommandBuffer cmd, ScriptableRenderContext context, ref RenderingData renderingData)
        {

            if (m_Material == null)
                return;

            var source = renderingData.cameraData.renderer.cameraColorTarget;
            m_Material.SetVector(params_id, new Vector4(m_settings.sigma, m_settings.radius, 0, 0));


            cmd.Blit(source, DenoisingBuffer_id_1);
            for (int i = 0; i <= m_settings.iteration; i++)
            {
                cmd.Blit(DenoisingBuffer_id_1, DenoisingBuffer_id_2, m_Material, 0);
                cmd.Blit(DenoisingBuffer_id_2, DenoisingBuffer_id_1, m_Material, 0);
            }
            cmd.Blit(DenoisingBuffer_id_1, source);

            context.ExecuteCommandBuffer(cmd);


            cmd.Clear();
            cmd.Release();
        }

        void Bilateral_Render(CommandBuffer cmd, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null)
                return;

            var source = renderingData.cameraData.renderer.cameraColorTarget;
            m_Material.SetVector(params_id, new Vector4(m_settings.sigma, m_settings.radius, m_settings.threshold, 0));


            cmd.Blit(source, DenoisingBuffer_id_1);
            for (int i = 0; i <= m_settings.iteration; i++)
            {
                cmd.Blit(DenoisingBuffer_id_1, DenoisingBuffer_id_2, m_Material, 1);
                cmd.Blit(DenoisingBuffer_id_2, DenoisingBuffer_id_1, m_Material, 1);
            }
            cmd.Blit(DenoisingBuffer_id_1, source);

            context.ExecuteCommandBuffer(cmd);


            cmd.Clear();
            cmd.Release();
        }
        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.Clear();
            cmd.ReleaseTemporaryRT(DenoisingBuffer_id_1);
            cmd.ReleaseTemporaryRT(DenoisingBuffer_id_2);
        }
    }

    GaussianFilteringRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new GaussianFilteringRenderPass(settings);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = settings.config.passEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


