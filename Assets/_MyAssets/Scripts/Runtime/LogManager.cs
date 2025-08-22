namespace MyScripts.Runtime
{
    internal sealed class LogManager : ASingletonMonoBehaviour<LogManager>
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image bg;

        private const float SHOW_DURATION_DEFAULT = 3;
        private const float FADEOUT_DURATION_DEFAULT = 1;

        private float bgAlphaDefault; // 初期化時に設定される. 1行分の高さ
        private float bgHeightDefault; // 初期化時に設定される
        private bool isShowingForcibly = false;
        private Cts cts = null;

        private void Awake()
        {
            bgAlphaDefault = bg.color.a;
            bgHeightDefault = bg.rectTransform.sizeDelta.y;

            SetText(string.Empty);
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
            SetText(string.Empty);
            cts = new();
        }

        private void SetText(string text)
        {
            if (this.text == null || this.bg == null) return;

            this.text.text = text;
            this.text.ForceMeshUpdate();

            if (string.IsNullOrEmpty(text))
            {
                this.text.alpha = 0;
                this.bg.SetAlpha(0);
            }
            else
            {
                this.text.alpha = 1;
                this.bg.SetAlpha(bgAlphaDefault);
            }

            this.bg.rectTransform.SetHeight(this.text.textInfo.lineCount * bgHeightDefault);
        }

        private void SetText(Utf16ValueStringBuilder text)
        {
            if (this.text == null || this.bg == null) return;

            this.text.SetText(text); // ここが違う
            this.text.ForceMeshUpdate();

            if (text.Length <= 0) // ここが違う
            {
                this.text.alpha = 0;
                this.bg.SetAlpha(0);
            }
            else
            {
                this.text.alpha = 1;
                this.bg.SetAlpha(bgAlphaDefault);
            }

            this.bg.rectTransform.SetHeight(this.text.textInfo.lineCount * bgHeightDefault);
        }

        // [0.0, 1.0]
        private void SetTransparencyByAlpha(float alpha)
        {
            if (this.text == null || this.bg == null) return;

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

            isShowingForcibly = !string.IsNullOrEmpty(text);
            RefreshCts();
            SetText(text);
        }

        /// <summary>
        /// 手動でログの表示と非表示を行う。<br/>
        /// textが null or Empty の場合、ログテキストを非表示にしたとみなす。<br/>
        /// NewlyShowLogText()を強制的に止め、ログを表示する。<br/>
        /// 表示している間、NewlyShowLogText()の実行は無効化される。<br/>
        /// </summary>
        internal void ShowManually(Utf16ValueStringBuilder text)
        {
            if (this.text == null || this.bg == null) return;

            isShowingForcibly = !(text.Length <= 0); // ここが違う
            RefreshCts();
            SetText(text);
        }

        /// <summary>
        /// 自動でログの表示と非表示を行う。<br/>
        /// </summary>
        internal void ShowAutomatically(
            string text, float duration = SHOW_DURATION_DEFAULT, float fadeoutDuration = FADEOUT_DURATION_DEFAULT, bool doGetOffInput = false)
        {
            if (this.text == null || this.bg == null) return;
            if (isShowingForcibly) return;
            if (string.IsNullOrEmpty(text))
            {
                $"automatic text must not be null or empty. text: {text}".LogWarning();
                return;
            }

            RefreshCts();
            ShowAutomaticallyImpl(text, duration, fadeoutDuration, cts.Token, doGetOffInput).Forget();
        }

        /// <summary>
        /// 自動でログの表示と非表示を行う。<br/>
        /// </summary>
        internal void ShowAutomatically(
            Utf16ValueStringBuilder text, float duration = SHOW_DURATION_DEFAULT, float fadeoutDuration = FADEOUT_DURATION_DEFAULT, bool doGetOffInput = false)
        {
            if (this.text == null || this.bg == null) return;
            if (isShowingForcibly) return;
            // ここが違う
            if (text.Length <= 0)
            {
                $"automatic text must not be empty. text: {text}".LogWarning();
                return;
            }

            RefreshCts();
            ShowAutomaticallyImpl(text, duration, fadeoutDuration, cts.Token, doGetOffInput).Forget();
        }

        // nullチェックはしない！
        private async UniTaskVoid ShowAutomaticallyImpl(string text, float duration, float fadeoutDuration, Ct ct, bool isGetOffInput = false)
        {
            SetText(text);

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
                await DOVirtual.Float(1, 0, fadeoutDuration, SetTransparencyByAlpha)
                    .SetEase(Ease.InQuad).WithCancellation(ct);

            // キャンセルしたら、ここまで処理が走らないことに注意！
            SetText(string.Empty);
        }

        // nullチェックはしない！
        private async UniTaskVoid ShowAutomaticallyImpl(Utf16ValueStringBuilder text, float duration, float fadeoutDuration, Ct ct, bool isGetOffInput = false)
        {
            SetText(text);

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
                await DOVirtual.Float(1, 0, fadeoutDuration, SetTransparencyByAlpha)
                    .SetEase(Ease.InQuad).WithCancellation(ct);

            // キャンセルしたら、ここまで処理が走らないことに注意！
            SetText(string.Empty);
        }

        private static async UniTask WaitUntilOffInput(Ct ct)
            => await UniTask.WaitUntil(() => InputManager.Instance.InGameCancel.Bool, cancellationToken: ct);
    }
}