using System.IO;
using UnityEngine.Rendering;

namespace MyScripts.Common;

internal static class Screenshot
{
    private static RenderTexture renderTexture = null;

    internal static void CaptureAndSaveAsync(Camera renderingCamera, string filePath)
    {
        int w = renderingCamera.pixelWidth;
        int h = renderingCamera.pixelHeight;

        // RenderTexture を再利用
        if (!renderTexture || renderTexture.width != w || renderTexture.height != h)
        {
            renderTexture?.Release();
            renderTexture = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        }

        // カメラで renderTexture に描画
        RenderTexture targetTexture = renderingCamera.targetTexture;
        renderingCamera.targetTexture = renderTexture;
        renderingCamera.Render();
        renderingCamera.targetTexture = targetTexture;

        "Screenshot captured".Print();

        // GPU → CPU 転送 (非同期)
        AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBA32, request =>
        {
            if (request.hasError)
            {
                "GPU Readback Error".Print(LogSettings.Error);
                return;
            }

            UniTask.Void(async () =>
            {
                await UniTask.RunOnThreadPool(() =>
                {
                    var data = request.GetData<byte>();

                    // RGBA32 → PNG
                    var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                    tex.LoadRawTextureData(data);
                    tex.Apply();

                    byte[] png = tex.EncodeToPNG();
                    UnityEngine.Object.Destroy(tex);

                    File.WriteAllBytes(filePath, png);
                });

                $"Screenshot saved: {filePath}".Print();
            });
        });
    }

    internal static async UniTask<Texture2D> LoadAsync(string filePath)
    {
        return await UniTask.RunOnThreadPool(() =>
        {
            if (!File.Exists(filePath))
            {
                $"Screenshot file not found: {filePath}".Print(LogSettings.Warning);
                return null;
            }

            byte[] bytes = File.ReadAllBytes(filePath);

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(bytes);  // PNG / JPG 内部で自動処理

            $"Screenshot loaded: {filePath}".Print();

            return tex;
        });
    }
}
