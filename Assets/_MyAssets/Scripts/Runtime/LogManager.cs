namespace MyScripts.Runtime
{
    internal sealed class LogManager : ASingletonMonoBehaviour<LogManager>
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image bg;

        private const float SHOW_DURATION_DEFAULT = 3;
        private const float FADEOUT_DURATION_DEFAULT = 1;

        private float bgAlphaDefault; // 初期化時に設定される
        private bool isShowingForcibly = false;
        private Cts cts = null;

        private void Awake()
        {
            bgAlphaDefault = bg.color.a;
            // 初期化部なので、nullチェックは無くて良さそう
            {
                this.text.text = string.Empty;
                SetTransparency(0);
            }
            RefreshCts();
        }

        private void OnDestroy()
        {
            CancelCts();
            cts = null;
        }

        private void CancelCts()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        private void RefreshCts()
        {
            CancelCts();
            {
                this.text.text = string.Empty;
                SetTransparency(0);
            }
            cts = new();
        }

        // nullチェックはしない！
        // alpha[0, 1]
        private void SetTransparency(float alpha)
        {
            this.text.alpha = alpha;
            this.bg.SetAlpha(alpha.Remap(1, 0, bgAlphaDefault, 0));
        }

        /// <summary>
        /// 手動でログの表示と非表示を行う。<br/>
        /// textが null or Empty の場合、ログテキストを非表示にしたとみなす。<br/>
        /// NewlyShowLogText()を強制的に止め、ログを表示する。<br/>
        /// 表示している間、NewlyShowLogText()の実行は無効化される。<br/>
        /// </summary>
        internal void ShowManually(string text)
        {
            if (this.text == null || this.bg == null) return;

            bool doStop = string.IsNullOrEmpty(text);

            isShowingForcibly = !doStop;

            RefreshCts();

            this.text.text = text;
            SetTransparency(doStop ? 0 : 1);
        }

        /// <summary>
        /// 自動でログの表示と非表示を行う。<br/>
        /// </summary>
        internal void ShowAutomatically(
            string text, float duration = SHOW_DURATION_DEFAULT, float fadeoutDuration = FADEOUT_DURATION_DEFAULT, bool doGetOffInput = false)
        {
            if (this.text == null || this.bg == null) return;
            if (isShowingForcibly) return;

            RefreshCts();
            ShowAutomaticallyImpl(text, duration, fadeoutDuration, cts.Token, doGetOffInput).Forget();
        }

        private async UniTaskVoid ShowAutomaticallyImpl(string text, float duration, float fadeoutDuration, Ct ct, bool isGetOffInput = false)
        {
            if (this.text == null || this.bg == null) return;

            this.text.text = text;
            SetTransparency(1);

            bool hasStoppedByOffInput = false;
            if (isGetOffInput)
            {
                int i = await UniTask.WhenAny(
                    WaitUntilOffInput(ct),
                    duration.SecAwait(ct: ct)
                );
                if (i == 0)
                    hasStoppedByOffInput = true;
            }
            else
                await duration.SecAwait(ct: ct);

            if (!hasStoppedByOffInput)
                await DOVirtual.Float(1, 0, fadeoutDuration, SetTransparency)
                    .SetEase(Ease.InQuad).WithCancellation(ct);

            // キャンセルしたら、ここまで処理が走らないことに注意！
            this.text.text = string.Empty;
            SetTransparency(0);
        }

        private static async UniTask WaitUntilOffInput(Ct ct)
            => await UniTask.WaitUntil(() => InputManager.Instance.InGameCancel.Bool, cancellationToken: ct);
    }
}