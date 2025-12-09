namespace MyScripts.Runtime
{
    internal sealed class WalkSoundPlayer : ASoundPlayerWithTypeAndOptions<SWalkSound, SWalkSound.Surface, WalkSoundPlayer.Options>
    {
        internal struct Options : ISoundPlayerOptions
        {
            internal bool IsSprinting { get; init; }
        }

        private AudioSource[] audioSources = null;
        private MotionHandle[] fadeOutTweens = null;
        private bool[] arePlaying = null;

        private SWalkSound.Surface currentSurface = SWalkSound.Surface.None;
        private bool isCurrentSprinting = false;

        /// <summary>
        /// プレイヤーの地面の、更新通知を送る
        /// </summary>
        internal sealed override void LetPlay(SWalkSound.Surface type, Options options)
        {
            if (type == currentSurface && options.IsSprinting == isCurrentSprinting) return;
            currentSurface = type;
            isCurrentSprinting = options.IsSprinting;

            bool couldPlay = false;
            for (int _i = 0; _i < Param.MaxSoundAmount; _i++)
            {
                int i = _i;

                if (arePlaying[i])
                {
                    if (fadeOutTweens[i].IsActive())
                        fadeOutTweens[i].Cancel();
                    fadeOutTweens[i] = LMotion.Create(audioSources[i].volume, 0.0f, Param.FadeOutDuration)
                        .WithEase(Ease.OutQuad)
                        .WithOnComplete(() =>
                        {
                            audioSources[i].LetStop();
                            fadeOutTweens[i] = default;

                            arePlaying[i] = false;
                        })
                        .BindToVolume(audioSources[i]);
                }
                else if (!couldPlay)
                {
                    AudioClip clip = Param.GetClip(currentSurface);
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
                        pitch: isCurrentSprinting ? Param.SprintPitch : Param.WalkPitch
                    );
                    arePlaying[i] = true;

                    couldPlay = true;
                    continue;
                }
            }

            if (!couldPlay)
                "All audio sources are playing. Cannot play new walk sound.".Print(LogSettings.Warning);
        }

        private protected sealed override void Init()
        {
            audioSources = new AudioSource[Param.MaxSoundAmount];
            fadeOutTweens = new MotionHandle[Param.MaxSoundAmount];
            arePlaying = new bool[Param.MaxSoundAmount];

            for (int i = 0; i < Param.MaxSoundAmount; i++)
            {
                AudioSource source = Root.gameObject.AddComponent<AudioSource>();
                source.LetInit
                (
                    Param.Group,
                    doLoop: true
                );

                audioSources[i] = source;
                fadeOutTweens[i] = default;
                arePlaying[i] = false;
            }
        }
    }
}
