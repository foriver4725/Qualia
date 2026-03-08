namespace MyScripts.Runtime
{
    internal sealed class DownloadManager : ASingletonMonoBehaviour<DownloadManager>
    {
        [SerializeField] private TextMeshProUGUI downloadingLabel;
        [SerializeField] private TextMeshProUGUI downloadingText;

        internal bool OnDownload { get; private set; } = false;

        private void Awake()
        {
            downloadingLabel.gameObject.SetActive(false);
            downloadingText.gameObject.SetActive(false);
        }

        internal async UniTask DownloadFileAsync(string url, Ct ct)
        {
            ct.ThrowIfCancellationRequested();

            if (OnDownload) return;
            OnDownload = true;

            try
            {
                await url.DownloadFileAsync(
                    ct,
                    beforeDownloadBegin: () =>
                    {
                        downloadingLabel.gameObject.SetActive(true);
                        downloadingText.gameObject.SetActive(true);
                    },
                    onDownloadingAsync: async ct =>
                    {
                        while (!ct.IsCancellationRequested)
                        {
                            downloadingLabel.text = "ダウンロード中";
                            await 0.2f.SecAwait(ct: ct);
                            downloadingLabel.text = "ダウンロード中.";
                            await 0.2f.SecAwait(ct: ct);
                            downloadingLabel.text = "ダウンロード中..";
                            await 0.2f.SecAwait(ct: ct);
                            downloadingLabel.text = "ダウンロード中...";
                            await 0.2f.SecAwait(ct: ct);
                        }
                    },
                    onDownloadProgressChanged: p => downloadingText.SetTextFormat("{0:F2}%", p * 100.0f),
                    afterDownloadEnd: _ =>
                    {
                        downloadingLabel.gameObject.SetActive(false);
                        downloadingText.gameObject.SetActive(false);
                    }
                );
            }
            finally
            {
                OnDownload = false;
            }
        }
    }
}