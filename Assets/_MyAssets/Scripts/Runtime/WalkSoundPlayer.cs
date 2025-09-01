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
        private Tween[] fadeOutTweens = null;
        private bool[] arePlaying = null;

        private SWalkSound.Surface currentSurface = SWalkSound.Surface.None;

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

            bool couldPlay = false;
            for (int _i = 0; _i < maxSoundAmount; _i++)
            {
                int i = _i;

                if (arePlaying[i])
                {
                    fadeOutTweens[i]?.Kill(complete: false);
                    fadeOutTweens[i] = audioSources[i]
                        .DOFade(0.0f, fadeOutDuration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            StopSound(audioSources[i]);
                            fadeOutTweens[i] = null;

                            arePlaying[i] = false;
                        });
                }
                else if (!couldPlay)
                {
                    AudioClip clip = walkSoundRef.GetClip(currentSurface);
                    if (clip == null)
                    {
                        // 何も鳴らさない
                        couldPlay = true;
                        continue;
                    }

                    PlaySound(audioSources[i], clip);
                    arePlaying[i] = true;

                    couldPlay = true;
                    continue;
                }
            }

            if (!couldPlay)
                "All audio sources are playing. Cannot play new walk sound.".LogWarning();
        }

        private static void StopSound(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.volume = 0.0f;
        }

        private static void PlaySound(AudioSource source, AudioClip clip)
        {
            source.clip = clip;
            source.volume = 1.0f;
            source.Play();
        }

        private void Initialize()
        {
            audioSources = new AudioSource[maxSoundAmount];
            fadeOutTweens = new Tween[maxSoundAmount];
            arePlaying = new bool[maxSoundAmount];

            for (int i = 0; i < maxSoundAmount; i++)
            {
                AudioSource source = walkSoundRoot.gameObject.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = walkSoundRef.Group;
                source.playOnAwake = false;
                source.loop = true;
                source.pitch = 1.2f;
                source.volume = 0.0f;

                audioSources[i] = source;
                fadeOutTweens[i] = null;
                arePlaying[i] = false;
            }
        }
    }
}
