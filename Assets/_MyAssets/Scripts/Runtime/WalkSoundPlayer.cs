namespace MyScripts.Runtime
{
    internal sealed class WalkSoundPlayer : MonoBehaviour
    {
        [SerializeField] private SWalkSound walkSoundRef;
        [SerializeField] private Transform walkSoundRoot;

        private AudioSource[] audioSources = null;
        private Tween[] fadeOutTweens = null;
        private bool[] arePlaying = null;

        private SWalkSound.Surface currentSurface = SWalkSound.Surface.None;
        private bool isCurrentSprinting = false;

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// プレイヤーの地面の、更新通知を送る
        /// </summary>
        internal void LetPlay(SWalkSound.Surface surface, bool isSprinting)
        {
            if (surface == currentSurface && isSprinting == isCurrentSprinting) return;
            currentSurface = surface;
            isCurrentSprinting = isSprinting;

            bool couldPlay = false;
            for (int _i = 0; _i < walkSoundRef.MaxSoundAmount; _i++)
            {
                int i = _i;

                if (arePlaying[i])
                {
                    fadeOutTweens[i]?.Kill(complete: false);
                    fadeOutTweens[i] = audioSources[i]
                        .DOFade(0.0f, walkSoundRef.FadeOutDuration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            audioSources[i].LetStop();
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

                    audioSources[i].LetPlay
                    (
                        clip,
                        volume: walkSoundRef.Volume,
                        pitch: isCurrentSprinting ? walkSoundRef.SprintPitch : walkSoundRef.WalkPitch
                    );
                    arePlaying[i] = true;

                    couldPlay = true;
                    continue;
                }
            }

            if (!couldPlay)
                "All audio sources are playing. Cannot play new walk sound.".LogWarning();
        }

        private void Initialize()
        {
            audioSources = new AudioSource[walkSoundRef.MaxSoundAmount];
            fadeOutTweens = new Tween[walkSoundRef.MaxSoundAmount];
            arePlaying = new bool[walkSoundRef.MaxSoundAmount];

            for (int i = 0; i < walkSoundRef.MaxSoundAmount; i++)
            {
                AudioSource source = walkSoundRoot.gameObject.AddComponent<AudioSource>();
                source.LetInit
                (
                    walkSoundRef.Group,
                    doLoop: true,
                    pitch: walkSoundRef.WalkPitch
                );

                audioSources[i] = source;
                fadeOutTweens[i] = null;
                arePlaying[i] = false;
            }
        }
    }
}
