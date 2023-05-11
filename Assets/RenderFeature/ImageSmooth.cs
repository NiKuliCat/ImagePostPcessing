using System;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[Serializable]
public enum SmoothType
{
    MedianFiltering,
    MeanFiltering
}

public class ImageSmooth : ScriptableRendererFeature
{

    [Serializable]
    public class ConfigSetting
    {
        public SmoothType smoothType = SmoothType.MeanFiltering;
        public FilterMode FilterMode;
        public RenderPassEvent passEvent;
    }

    [Serializable]
    public class Settings
    {
        [Range(1, 50)]
        public int iteration;
        public ConfigSetting config;
    }

    public Settings settings;

    static readonly int smoothBuffer_id_1 = Shader.PropertyToID("_SmoothBuffer1");
    static readonly int smoothBuffer_id_2 = Shader.PropertyToID("_SmoothBuffer2");
    class ImageSmoothRenderPass : ScriptableRenderPass
    {
        public Settings settings;
        private Material m_Material;


        public ImageSmoothRenderPass(Settings settings)
        {
            this.settings = settings;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor cameraData = renderingData.cameraData.cameraTargetDescriptor;

            cmd.GetTemporaryRT(smoothBuffer_id_1, cameraData.width, cameraData.height, 0, settings.config.FilterMode);
            cmd.GetTemporaryRT(smoothBuffer_id_2, cameraData.width, cameraData.height, 0, settings.config.FilterMode);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("ImageSmooth"); 

            switch(settings.config.smoothType)
            {
                case SmoothType.MeanFiltering:
                    m_Material = CoreUtils.CreateEngineMaterial(Shader.Find("Pineapple/MeanFiltering"));
                    MeanFiltering_Render(cmd, context, ref renderingData);
                    break;

                case SmoothType.MedianFiltering:
                    m_Material = CoreUtils.CreateEngineMaterial(Shader.Find("Pineapple/MedianFiltering"));
                    MedianFiltering_Render(cmd, context, ref renderingData);
                    break;
            }
           


          
        }

        void MeanFiltering_Render(CommandBuffer cmd, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null)
                return;

            var source = renderingData.cameraData.renderer.cameraColorTarget;


            cmd.Blit(source, smoothBuffer_id_1);

            //多次渲染处理
            for (int i = 0; i < settings.iteration; i++)
            {
                cmd.Blit(smoothBuffer_id_1, smoothBuffer_id_2, m_Material, 0);//水平方向渲染
                cmd.Blit(smoothBuffer_id_2, smoothBuffer_id_1, m_Material, 1);//垂直方向渲染
            }

            cmd.Blit(smoothBuffer_id_1, source);


            context.ExecuteCommandBuffer(cmd);


            cmd.Clear();
            cmd.Release();
        }

        void MedianFiltering_Render(CommandBuffer cmd, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null)
                return;

            var source = renderingData.cameraData.renderer.cameraColorTarget;


            cmd.Blit(source, smoothBuffer_id_1);

            //多次渲染处理
            for (int i = 0; i < settings.iteration; i++)
            {
                cmd.Blit(smoothBuffer_id_1, smoothBuffer_id_2, m_Material, 0);//0 ： min  1: median 2 : max
                cmd.Blit(smoothBuffer_id_2, smoothBuffer_id_1, m_Material, 0);
            }

            cmd.Blit(smoothBuffer_id_1, source);


            context.ExecuteCommandBuffer(cmd);


            cmd.Clear();
            cmd.Release();
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.Clear();
            cmd.ReleaseTemporaryRT(smoothBuffer_id_1);
            cmd.ReleaseTemporaryRT(smoothBuffer_id_2);
        }
    }

    ImageSmoothRenderPass m_RenderPass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_RenderPass = new ImageSmoothRenderPass(settings);


        m_RenderPass.renderPassEvent = settings.config.passEvent;
    }


    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_RenderPass);
    }
}


