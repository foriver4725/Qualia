namespace MyScripts.Runtime
{
    internal sealed class CharacterTriggerSoundPlayer : ASoundPlayerWithType<SCharacterTriggerSound, SCharacterTriggerSound.Timing>
    {
        [SerializeField, Range(0.0f, 5.0f), Tooltip("CloseToEnd の効果音を、開始何秒から鳴らすか")] private float timeOffsetOfCloseToEndSound = 1.0f;

        private AudioSource[] audioSources = null;

        internal float CloseToEndSoundLength => Param.CloseToEndSoundLength - timeOffsetOfCloseToEndSound;

        internal sealed override void LetPlay(SCharacterTriggerSound.Timing type)
        {
            AudioClip clip = Param.GetClip(type);
            if (clip == null)
            {
                "No valid clip exists to play.".LogWarning();
                return;
            }

            audioSources[(byte)type].LetPlay
            (
                clip,
                volume: Param.Volume,
                time: type == SCharacterTriggerSound.Timing.CloseToEnd ? timeOffsetOfCloseToEndSound : 0.0f
            );
        }

        private protected sealed override void Init()
        {
            audioSources = new AudioSource[3];

            for (int i = 0; i < 3; i++)
            {
                AudioSource source = Root.gameObject.AddComponent<AudioSource>();
                source.LetInit
                (
                    Param.Group
                );

                audioSources[i] = source;
            }
        }
    }
}
