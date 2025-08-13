using MyScripts.Common;

namespace MyScripts.Runtime
{
    internal sealed class LoadManager : ASingletonMonoBehaviour<LoadManager>
    {
        [SerializeField] private TextMeshProUGUI loadingLabel;
        [SerializeField] private TextMeshProUGUI loadingText;

        private bool hasLoadStarted = false;

        private void Awake()
        {
            loadingLabel.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(false);
        }

        internal void BeginLoad(Scene scene)
        {
            if (hasLoadStarted) return;
            hasLoadStarted = true;

            loadingLabel.gameObject.SetActive(true);
            loadingText.gameObject.SetActive(true);
            loadingText.text = string.Empty;

            scene.LoadAsync(
                onProgressChanged: progress => loadingText.text = $"{progress * 100f:0.0}%",
                onDoingCleanupAsync: async ct =>
                {
                    while (!ct.IsCancellationRequested)
                    {
                        loadingText.text = "不要なリソースを削除中.";
                        await 0.2f.SecAwait(ct: ct);
                        loadingText.text = "不要なリソースを削除中..";
                        await 0.2f.SecAwait(ct: ct);
                        loadingText.text = "不要なリソースを削除中...";
                        await 0.2f.SecAwait(ct: ct);
                    }
                },
                onCompletedAsync: async () =>
                {
                    loadingText.text = "ファイナライズ中(すぐに完了します)...";
                    await 0.2f.SecAwait();
                }
            ).Forget();
        }
    }
}