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

            Cts labelCts = new();
            BeginLabelAnimationAsync(labelCts.Token).Forget();
            loadingText.text = "0.00%";

            scene.LoadAsync(
                // 0-50 %
                afterCleanupEnd: () => loadingText.text = "50.00%",
                // 50-100%
                onLoadProgressChanged: p => loadingText.SetTextFormat("{0:F2}%", p.RemapClamped(0.0f, 1.0f, 50.0f, 100.0f)),
                afterLoadEnd: () =>
                {
                    labelCts.Cancel();
                    labelCts.Dispose();
                    loadingLabel.text = "ロード完了";
                },
                afterLoadEndBeforeSceneTriggerInvokeBegin: () => loadingText.text = "ファイナライズ中"
            ).Forget();
        }

        private async UniTaskVoid BeginLabelAnimationAsync(Ct ct)
        {
            while (!ct.IsCancellationRequested)
            {
                loadingLabel.text = "ロード中";
                await 0.2f.SecAwait(ct: ct);
                loadingLabel.text = "ロード中.";
                await 0.2f.SecAwait(ct: ct);
                loadingLabel.text = "ロード中..";
                await 0.2f.SecAwait(ct: ct);
                loadingLabel.text = "ロード中...";
                await 0.2f.SecAwait(ct: ct);
            }
        }
    }
}