using UnityEngine.Video;

namespace MyScripts.Runtime
{
    internal sealed class CutScenePlayer : ASingletonMonoBehaviour<CutScenePlayer>
    {
        [SerializeField, Range(0.0f, 5.0f)] private float bgFadeDuration = 0.5f;
        [SerializeField] private Image bg;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private VideoPlayer videoPlayer;

        private bool isPlaying = false;
        private SCutScene.CutSceneType currentPlayingType;

        // Awake で初期化
        private float bgAlphaMax;

        internal bool IsPlaying => isPlaying;

        private void Awake()
        {
            rawImage.enabled = false;
            videoPlayer.source = VideoSource.VideoClip;

            bgAlphaMax = bg.color.a;
            SetBgAlpha(0.0f);
            bg.enabled = false;
        }

        private void OnDestroy()
        {
            videoPlayer.prepareCompleted -= _ => OnPrepareCompleted();
            videoPlayer.started -= _ => OnStarted();
            videoPlayer.loopPointReached -= _ => OnLoopPointReached();
        }

        public void Play(SCutScene.CutSceneType type)
        {
            if (isPlaying)
            {
                $"既に{currentPlayingType}のカットシーンが再生中です。".Print(LogSettings.Warning);
                return;
            }

            isPlaying = true;
            currentPlayingType = type;

            OnBeginPlay();

            // 再生する
            {
                videoPlayer.clip = InGameSOHolder.Instance.CutScene.Get(type);

                videoPlayer.prepareCompleted += _ => OnPrepareCompleted();
                videoPlayer.loopPointReached += _ => OnLoopPointReached();
                videoPlayer.Prepare();
            }
        }

        #region 内部デリゲート

        private void OnPrepareCompleted()
        {
            rawImage.texture = videoPlayer.texture;

            videoPlayer.started += _ => OnStarted();
            videoPlayer.Play();
        }

        private void OnStarted()
        {
            rawImage.enabled = true;
        }

        private void OnLoopPointReached()
        {
            ResetFlags();
        }

        private void ResetFlags()
        {
            OnEndPlay();

            // currentPlayingType は何もしない
            videoPlayer.clip = null;
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
