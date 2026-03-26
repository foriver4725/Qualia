namespace MyScripts.Runtime
{
    internal sealed class ImageSequencePlayer : ASingletonMonoBehaviour<ImageSequencePlayer>
    {
        [SerializeField] private Image bg;
        [SerializeField] private Image targetImageFront;
        [SerializeField] private Image targetImageBack;
        [SerializeField] private Image progressBarFillImage;
        [SerializeField] private TextMeshProUGUI skipText;

        internal bool IsPlaying { get; private set; } = false;

        // Awake で初期化
        private float bgAlphaMax;
        private const float targetImageAlphaMax = 1.0f;

        // 画像が切り替わるのを待機するタスク群
        // 異なる待機方法を組み合わせるため、コレクションに入れて WhenAny する
        // 1つのリストをキャッシュして使いまわす
        private readonly List<UniTask> imageFlipWaitTasks = new(capacity: 2);

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

            progressBarFillImage.fillAmount = 0.0f;
            progressBarFillImage.enabled = false;

            skipText.enabled = false;
        }

        public async UniTask PlayAsync(
            IReadOnlyList<Sprite> sequence, IImageSequencePlayerPlayOptions playOptions, Ct ct)
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

            if (!playOptions.IsAutoStepEnabled && !playOptions.IsManualSkipEnabled)
            {
                "自動で次に流れていく設定も、ユーザーの操作で次に流れていく設定も両方とも無効になっています。どちらかを有効にしてください。".Print(LogSettings.Warning);
                return;
            }

            IsPlaying = true;
            OnBeginPlayAsync(playOptions.BgFadeDuration, ct).Forget();

            "再生を開始しました。".Print();

            // 2つのImageを交互にフェードイン・アウトさせて、滑らかなトランジションを実現する
            {
                // 再生中は、ずっとコンポーネントを有効にしておく

                SetAlpha(targetImageFront, 0.0f);
                targetImageFront.enabled = true;
                targetImageFront.sprite = null;

                SetAlpha(targetImageBack, 0.0f);
                targetImageBack.enabled = true;
                targetImageBack.sprite = null;

                progressBarFillImage.fillAmount = 0.0f;
                progressBarFillImage.enabled = true;

                skipText.enabled = playOptions.IsManualSkipEnabled;

                for (int i = 0; i < sequence.Count; i++)
                {
                    Sprite sprite = sequence[i];

                    progressBarFillImage.fillAmount = (float)i / sequence.Count;

                    bool isUsingFront = ((i & 1) == 0);
                    Image usingImage = isUsingFront ? targetImageFront : targetImageBack;
                    Image notUsingImage = isUsingFront ? targetImageBack : targetImageFront;

                    usingImage.sprite = sprite;
                    await UniTask.WhenAll(
                        FadeAsync(usingImage, targetImageAlphaMax, playOptions.StepFadeDuration, ct),
                        FadeAsync(notUsingImage, 0.0f, playOptions.StepFadeDuration, ct)
                    );

                    // 指定された方法で待機し、次の画像に切り替える
                    {
                        imageFlipWaitTasks.Clear();

                        // どちらかが完了するまで待機し、完了した時点でもう一方のタスク含め、全部キャンセルする
                        using Cts linkedCts = Cts.CreateLinkedTokenSource(ct);

                        if (playOptions.IsAutoStepEnabled)
                        {
                            imageFlipWaitTasks.Add(UniTask.WhenAll(
                                playOptions.AutoStepDuration.Await(ct: linkedCts.Token),
                                ChangeFillAmountAsync(progressBarFillImage, (float)(i + 1) / sequence.Count,
                                    playOptions.AutoStepDuration, linkedCts.Token)
                            ));
                        }

                        if (playOptions.IsManualSkipEnabled)
                        {
                            imageFlipWaitTasks.Add(UniTask.WaitUntil(playOptions.ManualSkipInputChecker,
                                static inputChecker => inputChecker(), cancellationToken: linkedCts.Token));
                        }

                        await UniTask.WhenAny(imageFlipWaitTasks);

                        linkedCts.Cancel();
                    }
                }

                SetAlpha(targetImageFront, 0.0f);
                targetImageFront.enabled = false;
                targetImageFront.sprite = null;

                SetAlpha(targetImageBack, 0.0f);
                targetImageBack.enabled = false;
                targetImageBack.sprite = null;

                progressBarFillImage.fillAmount = 0.0f;
                progressBarFillImage.enabled = false;

                skipText.enabled = false;
            }

            OnEndPlayAsync(playOptions.BgFadeDuration, ct).Forget();
            IsPlaying = false;

            "再生を終了しました。".Print();
        }

        private async UniTaskVoid OnBeginPlayAsync(TimeSpan bgFadeDuration, Ct ct)
        {
            InputManager.DisableAllInputs();

            SetAlpha(bg, 0.0f);
            bg.enabled = true;
            await FadeAsync(bg, bgAlphaMax, bgFadeDuration, ct);
        }

        private async UniTaskVoid OnEndPlayAsync(TimeSpan bgFadeDuration, Ct ct)
        {
            InputManager.EnableAllInputs();

            SetAlpha(bg, bgAlphaMax);
            await FadeAsync(bg, 0.0f, bgFadeDuration, ct);
            bg.enabled = false;
        }

        private static async UniTask FadeAsync(Image image, float targetAlpha, TimeSpan duration, Ct ct)
            => await LMotion.Create(image.color.a, targetAlpha, (float)duration.TotalSeconds)
                .WithEase(Ease.OutQuad)
                .Bind(alpha => SetAlpha(image, alpha))
                .ToUniTask(cancellationToken: ct);

        private static async UniTask ChangeFillAmountAsync(
            Image image, float targetFillAmount, TimeSpan duration, Ct ct)
            => await LMotion.Create(image.fillAmount, targetFillAmount, (float)duration.TotalSeconds)
                .WithEase(Ease.Linear)
                .Bind(fillAmount => image.fillAmount = fillAmount)
                .ToUniTask(cancellationToken: ct);

        private static void SetAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}