using UnityEngine.Video;

namespace MyScripts.Runtime
{
    internal sealed class CutScenePlayer : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private VideoPlayer videoPlayer;

        private bool isPlaying = false;
        private SCutScene.CutSceneType currentPlayingType;

        private void Awake()
        {
            rawImage.enabled = false;
            videoPlayer.source = VideoSource.VideoClip;
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

            // 再生する
            {
                videoPlayer.clip = InGameSOHolder.Instance.CutScene.Get(type);

                videoPlayer.prepareCompleted += _ => OnPrepareCompleted();
                videoPlayer.loopPointReached += _ => OnLoopPointReached();
                videoPlayer.Prepare();
            }
        }

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
            // currentPlayingType は何もしない
            videoPlayer.clip = null;
            isPlaying = false;
        }
    }
}
