using System;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



[Serializable]
public enum SharpeningType
{
    Roberts,
    Prewitt,
    Sobel
}


public class ImageSharpening : ScriptableRendererFeature
{
    [Serializable]
    public class ConfigSetting
    {
        public SharpeningType sharpeningType = SharpeningType.Roberts;
        public FilterMode FilterMode;
        public RenderPassEvent passEvent;
    }

    [Serializable]
    public class Settings
    {
        [Range(0f, 0.99f)]
        public float threshold;
        [Range(0f, 10f)]
        public float lineWidth;

        [Range(0f, 1f)]
        public float lineSmooth;

        [ColorUsage(true,true)]
        public Color lineColor;
        public Color backgroundColor;

        [Range(0f, 1f)]
        public float enableBackgroundColor;

        public ConfigSetting config;
    }

    public Settings settings;

    private static readonly int lineColor_id = Shader.PropertyToID("_LineColor");
    private static readonly int backgroundColor_id = Shader.PropertyToID("_BackgroundColor");
    private static readonly int Params_id = Shader.PropertyToID("_Params");
    private static readonly int SharpeningBuffer_id = Shader.PropertyToID("SharpeningBuffer");
    class ImageSharpeningRenderPass : ScriptableRenderPass
    {
        Settings m_settings;
        private Material m_Material;
        public ImageSharpeningRenderPass(Settings settings)
        {
            m_settings = settings;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor cameraData = renderingData.cameraData.cameraTargetDescriptor;

            cmd.GetTemporaryRT(SharpeningBuffer_id, cameraData.width, cameraData.height, 0, m_settings.config.FilterMode);
        }

        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("ImageSharpening");

            m_Material = CoreUtils.CreateEngineMaterial(Shader.Find("Pineapple/ImageSharpening"));

            if (m_Material == null)
                return;

            var source = renderingData.cameraData.renderer.cameraColorTarget;
            m_Material.SetVector(Params_id, new Vector4(m_settings.threshold, m_settings.lineWidth, m_settings.lineSmooth, m_settings.enableBackgroundColor));
            m_Material.SetColor(lineColor_id, m_settings.lineColor);
            m_Material.SetColor(backgroundColor_id, m_settings.backgroundColor);

            switch(m_settings.config.sharpeningType)
            {
                case SharpeningType.Roberts:
                    cmd.Blit(source, SharpeningBuffer_id, m_Material, 0);
                    break;
                case SharpeningType.Prewitt:
                    cmd.Blit(source, SharpeningBuffer_id, m_Material, 1);
                    break;
                case SharpeningType.Sobel:
                    cmd.Blit(source, SharpeningBuffer_id, m_Material, 2);
                    break;
            }

            cmd.Blit(SharpeningBuffer_id,source );

            context.ExecuteCommandBuffer(cmd);


            cmd.Clear();
            cmd.Release();

        }



        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.Clear();
            cmd.ReleaseTemporaryRT(SharpeningBuffer_id);
        }
    }

    ImageSharpeningRenderPass m_RenderPass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_RenderPass = new ImageSharpeningRenderPass(settings);

        // Configures where the render pass should be injected.
        m_RenderPass.renderPassEvent = settings.config.passEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_RenderPass);
    }
}


