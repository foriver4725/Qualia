using System.IO;
using UnityEngine.Rendering;

namespace MyScripts.Common
{
    internal sealed class ScreenshotManager : ASingletonMonoBehaviour<ScreenshotManager>
    {
        [SerializeField] private Camera renderingCamera;
        [SerializeField] private RenderTexture renderTextureReadOnly;
        [SerializeField] private Camera playerCamera;

        private void LateUpdate()
        {
            // プレイヤーのカメラと同じ位置・向きにする
            renderingCamera.transform.SetPositionAndRotation(
                playerCamera.transform.position,
                playerCamera.transform.rotation
            );
        }

        internal async UniTask CaptureAndSaveAsync(string filePath)
        {
            int w = renderTextureReadOnly.width;
            int h = renderTextureReadOnly.height;

            // 一瞬だけレンダリング
            renderingCamera.enabled = true;
            renderingCamera.Render();
            renderingCamera.enabled = false;

            "Screenshot captured".Print();

            // GPU → CPU 非同期転送
            var tcs = new UniTaskCompletionSource<byte[]>();

            _ = AsyncGPUReadback.Request(renderTextureReadOnly, 0, TextureFormat.RGBA32, request =>
            {
                if (request.hasError)
                {
                    tcs.TrySetException(new("GPU Readback Error"));
                }
                else
                {
                    tcs.TrySetResult(request.GetData<byte>().ToArray());
                }
            });

            // GPU転送をawait
            byte[] raw = await tcs.Task;

            // PNG化 & 保存
            await UniTask.RunOnThreadPool(() =>
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.LoadRawTextureData(raw);
                tex.Apply();

                byte[] png = tex.EncodeToPNG();
                Destroy(tex);

                File.WriteAllBytes(filePath, png);
            });

            $"Screenshot saved: {filePath}".Print();
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
}
