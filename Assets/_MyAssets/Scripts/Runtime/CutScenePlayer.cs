using UnityEngine.Video;

namespace MyScripts.Runtime
{
    internal sealed class CutScenePlayer : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private VideoPlayer videoPlayer;

        private bool isPlaying = false;
        private SCutScene.CutSceneType currentPlayingType;

        internal bool IsPlaying => isPlaying;

        private void Awake()
        {
            rawImage.enabled = false;
            videoPlayer.source = VideoSource.VideoClip;

            3.0f.SecAwaitThenDo(() => Play(SCutScene.CutSceneType.Intro)).Forget();
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
        }

        private void OnEndPlay()
        {
            InputManager.EnableAllInputs();
        }
    }
}
