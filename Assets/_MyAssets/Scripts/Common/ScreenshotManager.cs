using System.IO;
using UnityEngine.Rendering;
using MyScripts.Common.SaveSystem;

namespace MyScripts.Common
{
    internal sealed class ScreenshotManager : ASingletonMonoBehaviour<ScreenshotManager>
    {
        [SerializeField] private Camera renderingCamera;
        [SerializeField] private RenderTexture renderTextureReadOnly;
        [SerializeField] private Camera playerCamera;

        private const float CaptureDuration = 30.0f; // ゲーム中、この秒数おきにスクリーンショットを撮る

        private static string CreateFilePath(int fileId)
            => Path.Combine(
                Application.persistentDataPath,
                ZString.Format("screenshot_{0}_{1:yyyyMMdd_HHmmss}.png", fileId, DateTime.Now
            ));

        private void Awake()
            => CaptureAndSavePeriodicallyAsync(Variables.CurrentSlotIndex, destroyCancellationToken).Forget();

        private void LateUpdate()
        {
            // プレイヤーのカメラと同じ位置・向きにする
            renderingCamera.transform.SetPositionAndRotation(
                playerCamera.transform.position,
                playerCamera.transform.rotation
            );
        }

        private async UniTask CaptureAndSavePeriodicallyAsync(int fileId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            while (true)
            {
                await CaptureAndSaveAsync(fileId);
                await UniTask.WaitForSeconds(CaptureDuration, ignoreTimeScale: true, cancellationToken: ct);
            }
        }

        private async UniTask CaptureAndSaveAsync(int fileId)
        {
            int w = renderTextureReadOnly.width;
            int h = renderTextureReadOnly.height;

            // 一瞬だけレンダリング
            renderingCamera.Render();

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

            await UniTask.SwitchToMainThread();

            // ファイルパス
            string filePath = CreateFilePath(fileId);

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.LoadRawTextureData(raw);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            Destroy(tex);

            await UniTask.RunOnThreadPool(() =>
            {
                File.WriteAllBytes(filePath, png);
            });

            $"Screenshot saved: {filePath}".Print();
        }

        internal static async UniTask<Texture2D> LoadAsync(int fileId)
        {
            // ファイルパス
            string filePath = CreateFilePath(fileId);

            byte[] bytes = await UniTask.RunOnThreadPool(() =>
            {
                if (!File.Exists(filePath))
                {
                    $"Screenshot file not found: {filePath}".Print(LogSettings.Warning);
                    return null;
                }

                return File.ReadAllBytes(filePath);
            });

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(bytes);  // PNG / JPG 内部で自動処理

            $"Screenshot loaded: {filePath}".Print();

            return tex;
        }
    }
}
