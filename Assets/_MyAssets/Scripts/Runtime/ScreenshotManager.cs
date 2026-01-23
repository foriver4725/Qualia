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

        private async UniTask CaptureAndSavePeriodicallyAsync(int slotIndex, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            while (true)
            {
                await UniTask.WaitForSeconds(captureDuration, ignoreTimeScale: true, cancellationToken: ct);
                await UniTask.WaitUntil(() => !pauseInvoker.IsPaused, cancellationToken: ct); // ポーズ中は撮らない

                // ファイルパス
                string filePath = Path.Combine(
                    Application.persistentDataPath,
                    ZString.Format("screenshot_{0}.png", slotIndex)
                );

                if (File.Exists(filePath))
                {
                    $"Screenshot file already exists, overwriting: {filePath}".Print();
                }

                ScreenCapture.CaptureScreenshot(filePath);
                SaveLoadManager.Data.Slots[slotIndex].LastScreenshotSavedPath = filePath;

                $"Screenshot captured and saved: {filePath}".Print();
            }
        }

        //! 新しくテクスチャを作成する
        //TODO: 非同期化したい
        internal static Texture2D Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                $"Screenshot file not found: {filePath}".Print(LogSettings.Warning);
                return null;
            }

            byte[] bytes = File.ReadAllBytes(filePath);

            Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(bytes.AsSpan());  // PNG / JPG 内部で自動処理

            $"Screenshot loaded: {filePath}".Print();

            return texture;
        }
    }
}
