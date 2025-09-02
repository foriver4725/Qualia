namespace MyScripts.Runtime
{
    internal sealed class WalkSoundPlayer : ASoundPlayer
    {
        private SWalkSound myParam = null;
        private SWalkSound MyParam => (myParam != null) ? myParam : myParam = (Param as SWalkSound);

        private AudioSource[] audioSources = null;
        private Tween[] fadeOutTweens = null;
        private bool[] arePlaying = null;

        private SWalkSound.Surface currentSurface = SWalkSound.Surface.None;
        private bool isCurrentSprinting = false;

        /// <summary>
        /// プレイヤーの地面の、更新通知を送る
        /// </summary>
        internal void LetPlay(SWalkSound.Surface surface, bool isSprinting)
        {
            if (surface == currentSurface && isSprinting == isCurrentSprinting) return;
            currentSurface = surface;
            isCurrentSprinting = isSprinting;

            bool couldPlay = false;
            for (int _i = 0; _i < MyParam.MaxSoundAmount; _i++)
            {
                int i = _i;

                if (arePlaying[i])
                {
                    fadeOutTweens[i]?.Kill(complete: false);
                    fadeOutTweens[i] = audioSources[i]
                        .DOFade(0.0f, MyParam.FadeOutDuration)
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
                    AudioClip clip = MyParam.GetClip(currentSurface);
                    if (clip == null)
                    {
                        // 何も鳴らさない
                        couldPlay = true;
                        continue;
                    }

                    audioSources[i].LetPlay
                    (
                        clip,
                        volume: Param.Volume,
                        pitch: isCurrentSprinting ? MyParam.SprintPitch : MyParam.WalkPitch
                    );
                    arePlaying[i] = true;

                    couldPlay = true;
                    continue;
                }
            }

            if (!couldPlay)
                "All audio sources are playing. Cannot play new walk sound.".LogWarning();
        }

        private protected sealed override void Init()
        {
            audioSources = new AudioSource[MyParam.MaxSoundAmount];
            fadeOutTweens = new Tween[MyParam.MaxSoundAmount];
            arePlaying = new bool[MyParam.MaxSoundAmount];

            for (int i = 0; i < MyParam.MaxSoundAmount; i++)
            {
                AudioSource source = Root.gameObject.AddComponent<AudioSource>();
                source.LetInit
                (
                    Param.Group,
                    doLoop: true,
                    pitch: MyParam.WalkPitch
                );

                audioSources[i] = source;
                fadeOutTweens[i] = null;
                arePlaying[i] = false;
            }
        }
    }
}
