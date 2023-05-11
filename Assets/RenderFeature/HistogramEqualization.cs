using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HistogramEqualization : ScriptableRendererFeature
{
    [Serializable]
    public class ConfigSetting
    {
        public FilterMode FilterMode;
        public RenderPassEvent passEvent;
    }

    [Serializable]
    public class Settings
    {
        [Range(0f, 1f)]
        public float intensity;
        public Texture2D image;
        public Texture2D modify_image;
        public ConfigSetting config;
    }

    public Settings settings;

    static readonly int resouceTex_id = Shader.PropertyToID("_Image");
    static readonly int modifyTex_id = Shader.PropertyToID("_Modify_Image");
    static readonly int intensity_id = Shader.PropertyToID("_Intensity");

    class HistogramequalizationRenderPass : ScriptableRenderPass
    {
        public Settings settings;
        private Material m_Material;


        public HistogramequalizationRenderPass(Settings settings)
        {
            this.settings = settings;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_Material = CoreUtils.CreateEngineMaterial(Shader.Find("Pineapple/HistogramEqualization"));

            if (m_Material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("Histogramequalization");
            var source = renderingData.cameraData.renderer.cameraColorTarget;
            m_Material.SetTexture(resouceTex_id, settings.image);
            m_Material.SetTexture(modifyTex_id, settings.modify_image);
            m_Material.SetFloat(intensity_id, settings.intensity);

            cmd.Blit(source, source, m_Material, 0);

            context.ExecuteCommandBuffer(cmd);


            cmd.Clear();
            cmd.Release();

        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    HistogramequalizationRenderPass m_renderPass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_renderPass = new HistogramequalizationRenderPass(settings);

        // Configures where the render pass should be injected.
        m_renderPass.renderPassEvent = settings.config.passEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_renderPass);
    }
}


