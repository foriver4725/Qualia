using UnityEngine.Video;

namespace MyScripts.Runtime
{
    internal sealed class CutScenePlayer : ASingletonMonoBehaviour<CutScenePlayer>
    {
        [SerializeField, Range(0.0f, 5.0f)] private float bgFadeDuration = 0.5f;
        [SerializeField] private Image bg;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private SCloudFileUrl cloudFileUrl;
        [SerializeField] private SGameConfig gameConfig;

        private bool isPlaying = false;
        private SCloudFileUrl.FileType currentPlayingType; // 動画のみが入る想定

        // Awake で初期化
        private float bgAlphaMax;

        internal bool IsPlaying => isPlaying;

        private void Awake()
        {
            rawImage.enabled = false;
            videoPlayer.source = VideoSource.Url;

            bgAlphaMax = bg.color.a;
            SetBgAlpha(0.0f);
            bg.enabled = false;

            videoPlayer.prepareCompleted += OnPrepareCompleted;
            videoPlayer.started += OnStarted;
            videoPlayer.loopPointReached += OnLoopPointReached;
        }

        private void OnDestroy()
        {
            videoPlayer.prepareCompleted -= OnPrepareCompleted;
            videoPlayer.started -= OnStarted;
            videoPlayer.loopPointReached -= OnLoopPointReached;
        }

        public async UniTask PlayAsync(SCloudFileUrl.FileType type, Ct ct)
        {
            ct.ThrowIfCancellationRequested();

            if (isPlaying)
            {
                $"既に{currentPlayingType}のカットシーンが再生中です。".Print(LogSettings.Warning);
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!gameConfig.DoesPlayIntroCutScene && type == SCloudFileUrl.FileType.Movie_Intro)
            {
                "イントロカットシーンの再生は設定で無効化されています。".Print(LogSettings.Warning);
                return;
            }
#endif

            // 状態を更新
            isPlaying = true;
            currentPlayingType = type; // 元に戻すことはない
            OnBeginPlay();

            string url = cloudFileUrl.Get(type);
            string saveName = ZString.Format("CutScene_{0}", type);
            (bool success, string savePath) = await FileDownloader.DownloadAsync(
                url, saveName, FileDownloader.Extension.MP4, ct);
            if (!success)
            {
                "カットシーンのダウンロードに失敗したため、再生を中止します。".Print(LogSettings.Error);

                // 状態をリセット
                OnEndPlay();
                isPlaying = false;

                return;
            }

            // 再生する
            videoPlayer.Stop(); // 念のため、明示的にストップ
            videoPlayer.url = ""; // URLをクリアしないとPrepareが動作しない場合がある
            videoPlayer.url = ZString.Format("file://{0}", savePath); // 元に戻すことはない
            videoPlayer.Prepare();

            try
            {
                await UniTask.WaitUntil(() => !isPlaying, cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                "カットシーンの再生がキャンセルされました。".Print(LogSettings.Warning);
            }
            // キャンセル時、確実に状態をリセットする
            finally
            {
                if (isPlaying)
                {
                    OnEndPlay();
                    isPlaying = false;
                }
            }
        }

        #region 内部デリゲート

        private void OnPrepareCompleted(VideoPlayer _) => OnPrepareCompletedInternal();
        private void OnStarted(VideoPlayer _) => OnStartedInternal();
        private void OnLoopPointReached(VideoPlayer _) => OnLoopPointReachedInternal();

        private void OnPrepareCompletedInternal()
        {
            rawImage.texture = videoPlayer.texture;
            videoPlayer.Play();
        }

        private void OnStartedInternal()
        {
            rawImage.enabled = true;
        }

        private void OnLoopPointReachedInternal()
        {
            rawImage.enabled = false;

            OnEndPlay();
            isPlaying = false;
        }

        #endregion

        private void OnBeginPlay()
        {
            InputManager.DisableAllInputs();
            FadeInBgAsync(destroyCancellationToken).Forget();
        }

        private void OnEndPlay()
        {
            InputManager.EnableAllInputs();
            FadeOutBgAsync(destroyCancellationToken).Forget();
        }

        // 重複実行はバグると思う
        private async UniTaskVoid FadeInBgAsync(Ct ct)
        {
            SetBgAlpha(0.0f);
            bg.enabled = true;

            await LMotion.Create(0.0f, bgAlphaMax, bgFadeDuration)
                        .WithEase(Ease.OutQuad)
                        .Bind(SetBgAlpha)
                        .ToUniTask(cancellationToken: ct);
        }

        // 重複実行はバグると思う
        private async UniTaskVoid FadeOutBgAsync(Ct ct)
        {
            SetBgAlpha(bgAlphaMax);

            await LMotion.Create(bgAlphaMax, 0.0f, bgFadeDuration)
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
