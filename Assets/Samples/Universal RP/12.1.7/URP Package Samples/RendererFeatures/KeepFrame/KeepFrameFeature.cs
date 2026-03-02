using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KeepFrameFeature : ScriptableRendererFeature
{
    class CopyFramePass : ScriptableRenderPass
    {
        private RTHandle source;
        private RTHandle destination;

        public void Setup(RTHandle source, RTHandle destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera.cameraType != CameraType.Game)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("CopyFramePass");

            Blitter.BlitCameraTexture(cmd, source, destination);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    class DrawOldFramePass : ScriptableRenderPass
    {
        private Material drawMaterial;
        private RTHandle frameHandle;
        private string textureName;

        public void Setup(Material material, RTHandle handle, string textureName)
        {
            drawMaterial = material;
            frameHandle = handle;
            this.textureName = textureName;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (drawMaterial == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("DrawOldFramePass");

            cmd.SetGlobalTexture(textureName, frameHandle);

            // Rita fullscreen quad
            Blitter.BlitCameraTexture(cmd, frameHandle, renderingData.cameraData.renderer.cameraColorTargetHandle, drawMaterial, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [Serializable]
    public class Settings
    {
        [Tooltip("Material som används för att rita gamla frame-texturen.")]
        public Material displayMaterial;

        [Tooltip("Namnet på global shader-textur. Default: _FrameCopyTex")]
        public string textureName = "_FrameCopyTex";
    }

    public Settings settings = new Settings();

    private CopyFramePass copyPass;
    private DrawOldFramePass drawPass;

    private RTHandle oldFrameHandle;

    public override void Create()
    {
        copyPass = new CopyFramePass
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };

        drawPass = new DrawOldFramePass
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques
        };

        oldFrameHandle = RTHandles.Alloc(
            Vector2.one,
            name: "_OldFrameTexture",
            dimension: TextureDimension.Tex2D,
            useDynamicScale: true
        );
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.displayMaterial == null)
            return;

        copyPass.Setup(renderer.cameraColorTargetHandle, oldFrameHandle);
        renderer.EnqueuePass(copyPass);

        drawPass.Setup(settings.displayMaterial, oldFrameHandle, settings.textureName);
        renderer.EnqueuePass(drawPass);
    }

    protected override void Dispose(bool disposing)
    {
        oldFrameHandle?.Release();
    }
}