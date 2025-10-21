using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WaterSystem
{
    public class WaterFxPass : ScriptableRenderPass
    {
        private const string k_RenderWaterFXTag = "Render Water FX";
        private ProfilingSampler m_WaterFX_Profile = new ProfilingSampler(k_RenderWaterFXTag);
        private readonly ShaderTagId m_WaterFXShaderTag = new ShaderTagId("WaterFX");
        private readonly Color m_ClearColor = new Color(0.0f, 0.5f, 0.5f, 0.5f);
        private FilteringSettings m_FilteringSettings;
        private RTHandle m_WaterFX;

        public WaterFxPass()
        {
            // Allocate RTHandle for the water FX map
            m_WaterFX = RTHandles.Alloc(
                width: 512, // arbitrary or dynamic later
                height: 512,
                colorFormat: GraphicsFormat.R8G8B8A8_UNorm,
                name: "_WaterFXMap"
);

            m_FilteringSettings = new FilteringSettings(RenderQueueRange.transparent);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // No need for depth
            cameraTextureDescriptor.depthBufferBits = 0;
            cameraTextureDescriptor.width /= 2;
            cameraTextureDescriptor.height /= 2;
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.Default;

            // Configure the target to this RTHandle
            ConfigureTarget(m_WaterFX);
            ConfigureClear(ClearFlag.Color, m_ClearColor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_WaterFX_Profile))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var drawSettings = CreateDrawingSettings(
                    m_WaterFXShaderTag,
                    ref renderingData,
                    SortingCriteria.CommonTransparent
                );

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // Release allocated RTHandle after use
            RTHandles.Release(m_WaterFX);
        }
    }
}