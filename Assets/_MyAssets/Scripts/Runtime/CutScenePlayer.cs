using UnityEngine.Video;

namespace MyScripts.Runtime
{
    internal sealed class CutScenePlayer : ASingletonMonoBehaviour<CutScenePlayer>
    {
        [SerializeField, Range(0.0f, 5.0f)] private float bgFadeDuration = 0.5f;
        [SerializeField] private Image bg;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private VideoClip videoClip;

        internal bool IsPlaying { get; private set; } = false;

        // Awake で初期化
        private float bgAlphaMax;

        private void Awake()
        {
            rawImage.enabled = false;
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = videoClip;

            bgAlphaMax = bg.color.a;
            SetBgAlpha(0.0f);
            bg.enabled = false;

            videoPlayer.prepareCompleted += OnPrepareCompleted;
            videoPlayer.started          += OnStarted;
            videoPlayer.loopPointReached += OnLoopPointReached;
            videoPlayer.errorReceived    += OnErrorReceived;
        }

        private void OnDestroy()
        {
            videoPlayer.prepareCompleted -= OnPrepareCompleted;
            videoPlayer.started          -= OnStarted;
            videoPlayer.loopPointReached -= OnLoopPointReached;
            videoPlayer.errorReceived    -= OnErrorReceived;
        }

        public async UniTask PlayAsync(Ct ct)
        {
            ct.ThrowIfCancellationRequested();

            if (IsPlaying)
            {
                "既にカットシーンが再生中です。".Print(LogSettings.Warning);
                return;
            }

            if (videoClip == null)
            {
                "VideoClip が設定されていません。".Print(LogSettings.Error);
                return;
            }

            IsPlaying = true;

            // フェードイン中に呼び出し元がキャンセルした場合、フェードインを止めてからフェードアウトへ移行するため
            // ct と destroyCancellationToken の両方でキャンセルできるリンクトークンを渡す
            using Cts linkedBeginCts = Cts.CreateLinkedTokenSource(ct, destroyCancellationToken);
            OnBeginPlayAsync(bgFadeDuration, linkedBeginCts.Token).Forget();

            "カットシーンの再生準備中...".Print();

            // VideoClip を設定して準備する
            videoPlayer.Stop();
            videoPlayer.clip = videoClip;
            videoPlayer.Prepare();

            try
            {
                await UniTask.WaitUntil(() => !IsPlaying, cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                "カットシーンの再生がキャンセルされました。".Print(LogSettings.Warning);
            }
            finally
            {
                if (IsPlaying)
                {
                    // フェードインが進行中ならここで停止させ、フェードアウトと競合しないようにする
                    linkedBeginCts.Cancel();
                    videoPlayer.Stop();
                    rawImage.enabled = false;
                    OnEndPlayAsync(bgFadeDuration, destroyCancellationToken).Forget();
                    IsPlaying = false;
                }
            }
        }

        #region 内部デリゲート

        private void OnPrepareCompleted(VideoPlayer _) => OnPrepareCompletedInternal();
        private void OnStarted(VideoPlayer _)          => OnStartedInternal();
        private void OnLoopPointReached(VideoPlayer _) => OnLoopPointReachedInternal();
        private void OnErrorReceived(VideoPlayer _, string message) => OnErrorReceivedInternal(message);

        private void OnPrepareCompletedInternal()
        {
            rawImage.texture = videoPlayer.texture;
            videoPlayer.Play();
        }

        private void OnStartedInternal()
        {
            rawImage.enabled = true;
            "カットシーンの再生を開始しました。".Print();
        }

        private void OnLoopPointReachedInternal()
        {
            rawImage.enabled = false;

            OnEndPlayAsync(bgFadeDuration, destroyCancellationToken).Forget();
            IsPlaying = false;
        }

        // Prepare 失敗など VideoPlayer 内部エラー時の後始末
        // IsPlaying を false にすることで PlayAsync の WaitUntil を解除し、無限待機を防ぐ
        private void OnErrorReceivedInternal(string message)
        {
            $"VideoPlayer でエラーが発生したため、再生を中止します。エラー: {message}".Print(LogSettings.Error);

            if (!IsPlaying) return;

            videoPlayer.Stop();
            rawImage.enabled = false;
            OnEndPlayAsync(bgFadeDuration, destroyCancellationToken).Forget();
            IsPlaying = false;
        }

        #endregion

        private async UniTaskVoid OnBeginPlayAsync(float duration, Ct ct)
        {
            InputManager.DisableAllInputs();

            SetBgAlpha(0.0f);
            bg.enabled = true;

            await LMotion.Create(0.0f, bgAlphaMax, duration)
                        .WithEase(Ease.OutQuad)
                        .Bind(SetBgAlpha)
                        .ToUniTask(cancellationToken: ct);
        }

        private async UniTaskVoid OnEndPlayAsync(float duration, Ct ct)
        {
            InputManager.EnableAllInputs();

            SetBgAlpha(bgAlphaMax);

            await LMotion.Create(bgAlphaMax, 0.0f, duration)
                        .WithEase(Ease.InQuad)
                        .Bind(SetBgAlpha)
                        .ToUniTask(cancellationToken: ct);

            bg.enabled = false;
        }

        private void SetBgAlpha(float alpha)
        {
            Color color = bg.color;
            color.a = alpha;
            bg.color = color;
        }
    }
}
