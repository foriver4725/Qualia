namespace MyScripts.Runtime
{
    internal sealed class ImageSequencePlayer : ASingletonMonoBehaviour<ImageSequencePlayer>
    {
        [Serializable]
        internal sealed class TransitDurations
        {
            [SerializeField, Range(0.0f, 5.0f)] private float bgFadeDuration = 0.5f;
            [SerializeField, Range(0.0f, 60.0f)] private float autoStepDuration = 5.0f;
            [SerializeField, Range(0.0f, 5.0f)] private float stepFadeDuration = 0.5f;

            internal float BgFadeDuration => bgFadeDuration;
            internal float AutoStepDuration => autoStepDuration;
            internal float StepFadeDuration => stepFadeDuration;
        }

        [SerializeField] private Image bg;
        [SerializeField] private Image targetImageFront;
        [SerializeField] private Image targetImageBack;

        internal bool IsPlaying { get; private set; } = false;

        // Awake で初期化
        private float bgAlphaMax;
        private const float targetImageAlphaMax = 1.0f;

        private void Awake()
        {
            SetAlpha(targetImageFront, 0.0f);
            targetImageFront.enabled = false;
            targetImageFront.sprite = null;

            SetAlpha(targetImageBack, 0.0f);
            targetImageBack.enabled = false;
            targetImageBack.sprite = null;

            bgAlphaMax = bg.color.a;
            SetAlpha(bg, 0.0f);
            bg.enabled = false;
        }

        public async UniTask PlayAsync(IReadOnlyList<Sprite> sequence, TransitDurations durations, Ct ct)
        {
            ct.ThrowIfCancellationRequested();

            if (sequence.Count is not (>= 1 and < 0xff))
            {
                $"シーケンスの画像の枚数が不正です。指定された枚数: {sequence.Count}".Print(LogSettings.Error);
                return;
            }

            if (IsPlaying)
            {
                "既にシーケンスが再生中です。".Print(LogSettings.Warning);
                return;
            }

            IsPlaying = true;
            OnBeginPlayAsync(durations.BgFadeDuration, ct).Forget();

            "再生を開始しました。".Print();

            // 2つのImageを交互にフェードイン・アウトさせて、滑らかなトランジションを実現する

            {
                // ずっとアクティブにする
                SetAlpha(targetImageFront, 0.0f);
                targetImageFront.enabled = true;
                targetImageFront.sprite = null;

                SetAlpha(targetImageBack, 0.0f);
                targetImageBack.enabled = true;
                targetImageBack.sprite = null;

                for (int i = 0; i < sequence.Count; i++)
                {
                    Sprite sprite = sequence[i];

                    bool isUsingFront = ((i & 1) == 0);
                    Image usingImage = isUsingFront ? targetImageFront : targetImageBack;
                    Image notUsingImage = isUsingFront ? targetImageBack : targetImageFront;

                    usingImage.sprite = sprite;
                    await UniTask.WhenAll(
                        FadeAsync(usingImage, targetImageAlphaMax, durations.StepFadeDuration, ct),
                        FadeAsync(notUsingImage, 0.0f, durations.StepFadeDuration, ct)
                    );

                    await durations.AutoStepDuration.SecAwait(ct: ct);
                }

                SetAlpha(targetImageFront, 0.0f);
                targetImageFront.enabled = false;
                targetImageFront.sprite = null;

                SetAlpha(targetImageBack, 0.0f);
                targetImageBack.enabled = false;
                targetImageBack.sprite = null;
            }

            OnEndPlayAsync(durations.BgFadeDuration, ct).Forget();
            IsPlaying = false;

            "再生を終了しました。".Print();
        }

        private async UniTaskVoid OnBeginPlayAsync(float bgFadeDuration, Ct ct)
        {
            InputManager.DisableAllInputs();

            SetAlpha(bg, 0.0f);
            bg.enabled = true;
            await FadeAsync(bg, bgAlphaMax, bgFadeDuration, destroyCancellationToken);
        }

        private async UniTaskVoid OnEndPlayAsync(float bgFadeDuration, Ct ct)
        {
            InputManager.EnableAllInputs();

            SetAlpha(bg, bgAlphaMax);
            await FadeAsync(bg, 0.0f, bgFadeDuration, destroyCancellationToken);
            bg.enabled = false;
        }

        // 重複実行はバグると思う
        private static async UniTask FadeAsync(Image image, float targetAlpha, float duration, Ct ct)
            => await LMotion.Create(image.color.a, targetAlpha, duration)
                .WithEase(Ease.OutQuad)
                .Bind(alpha => SetAlpha(image, alpha))
                .ToUniTask(cancellationToken: ct);

        private static void SetAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}