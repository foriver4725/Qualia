using MyScripts.SO.Reference;

namespace MyScripts.Runtime
{
    internal sealed class WalkSoundPlayer : MonoBehaviour
    {
        [SerializeField] private SWalkSound walkSoundRef;
        [SerializeField] private Transform walkSoundRoot;
        [SerializeField, Range(1, 16), Tooltip("同時に鳴る足音の最大数")] private byte maxSoundAmount = 8;
        [SerializeField, Range(0.0f, 0.5f), Tooltip("足音がフェードアウトするまでの時間")] private float fadeOutDuration = 0.2f;

        private AudioSource[] audioSources = null;
        private bool[] arePlaying = null;
        // 順番にサウンドを鳴らす
        // まだ鳴っていないものの先頭を指し示す
        private int headIndex = 0;
        // 現在鳴っているものの先頭を指し示す (無いなら、 head と同じ)
        private int tailIndex = 0;

        private SWalkSound.Surface currentSurface = SWalkSound.Surface.None;
        private List<UniTask> fadeOutTasks = null;
        private bool isDoingFadeOut = false; // フェードアウト中かどうか

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// プレイヤーの地面の、更新通知を送る
        /// </summary>
        internal void LetPlay(SWalkSound.Surface surface)
        {
            if (surface == currentSurface) return;
            currentSurface = surface;

            // PlayNewSound で headIndex が更新されるので、その前の値をコピーして受け渡す
            FadeoutAndStopAsync(headIndex, destroyCancellationToken).Forget();
            PlayNewSound();
        }

        private async UniTaskVoid FadeoutAndStopAsync(int headIndex, Ct ct)
        {
            //! 鳴らす → 鳴らさない → 鳴らす → 鳴らさない まで行くと、フェードアウト処理が重複するため、バグる。
            //! ただ、フェードアウト時間中にそれほどの処理が来ることはまずないと思うので、無視する。
            if (isDoingFadeOut) return;
            isDoingFadeOut = true;

            fadeOutTasks.Clear();
            for (int i = tailIndex; i != headIndex; i = (i + 1) % maxSoundAmount)
            {
                if (!arePlaying[i]) continue;

                fadeOutTasks.Add(
                    audioSources[i]
                        .DOFade(0.0f, fadeOutDuration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            audioSources[i].Stop();
                            audioSources[i].clip = null;
                            arePlaying[i] = false;
                        })
                        .OnKill(() =>
                        {
                            // 既に Complete が実行されたということなので、何もしない
                            if (!arePlaying[i])
                                return;

                            audioSources[i].Stop();
                            audioSources[i].clip = null;
                            arePlaying[i] = false;
                        })
                        .WithCancellation(ct)
                );
            }

            try
            {
                await UniTask.WhenAll(fadeOutTasks);
            }
            finally
            {
                fadeOutTasks.Clear();
                tailIndex = this.headIndex; // 追いつく
                isDoingFadeOut = false;
            }
        }

        private void PlayNewSound()
        {
            AudioClip clip = walkSoundRef.GetClip(currentSurface);
            if (clip == null)
                return; // 何も鳴らさない

            // 1回しか実行されないはずだが、念のため
            for (int i = 0; i < maxSoundAmount; i++)
            {
                headIndex = (headIndex + 1) % maxSoundAmount;
                if (headIndex == tailIndex)
                {
                    "Reached tail index. Cannot play new walk sound.".LogWarning();
                    return;
                }

                if (arePlaying[headIndex]) continue;
                arePlaying[headIndex] = true;

                audioSources[headIndex].clip = clip;
                audioSources[headIndex].Play();

                return;
            }

            "All audio sources are playing. Cannot play new walk sound.".LogWarning();
            return;
        }

        private void Initialize()
        {
            fadeOutTasks = new List<UniTask>(maxSoundAmount);
            audioSources = new AudioSource[maxSoundAmount];
            arePlaying = new bool[maxSoundAmount];

            for (int i = 0; i < maxSoundAmount; i++)
            {
                AudioSource source = walkSoundRoot.gameObject.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = walkSoundRef.Group;
                source.playOnAwake = false;
                source.loop = true;

                audioSources[i] = source;
                arePlaying[i] = false;
            }
        }
    }
}
