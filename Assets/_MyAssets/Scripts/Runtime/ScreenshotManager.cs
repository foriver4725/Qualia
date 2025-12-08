using System.IO;
using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime
{
    internal sealed class ScreenshotManager : MonoBehaviour
    {
        [SerializeField] private SGameConfig gameConfig;
        [SerializeField] private PauseInvoker pauseInvoker;

        // Awake で初期化
        private float captureDuration;

        private void Awake()
        {
            captureDuration = gameConfig.ScreenshotCaptureInterval;
            CaptureAndSavePeriodicallyAsync(Variables.CurrentSlotIndex, destroyCancellationToken).Forget();
        }

        private async UniTask CaptureAndSavePeriodicallyAsync(int fileId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            while (true)
            {
                await UniTask.WaitForSeconds(captureDuration, ignoreTimeScale: true, cancellationToken: ct);
                await UniTask.WaitUntil(() => !pauseInvoker.IsPaused, cancellationToken: ct); // ポーズ中は撮らない

                // ファイルパス
                string filePath = CreateFilePath(fileId);

                ScreenCapture.CaptureScreenshot(filePath);
                SaveLoadManager.Data.Slots[fileId].LastScreenshotSavedPath = filePath;

                $"Screenshot captured and saved: {filePath}".Print();
            }
        }

        internal static async UniTask<Texture2D> LoadAsync(int fileId)
        {
            // ファイルパス
            string filePath = CreateFilePath(fileId);

            if (!File.Exists(filePath))
            {
                $"Screenshot file not found: {filePath}".Print(LogSettings.Warning);
                return null;
            }

            byte[] bytes = File.ReadAllBytes(filePath);

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(bytes.AsSpan());  // PNG / JPG 内部で自動処理

            $"Screenshot loaded: {filePath}".Print();

            return tex;
        }

        private static string CreateFilePath(int fileId)
            => Path.Combine(
                Application.persistentDataPath,
                ZString.Format("screenshot_{0}_{1:yyyyMMdd_HHmmss}.png", fileId, DateTime.Now
            ));
    }
}
