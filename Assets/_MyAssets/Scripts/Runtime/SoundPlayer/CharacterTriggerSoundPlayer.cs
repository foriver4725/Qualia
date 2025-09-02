namespace MyScripts.Runtime
{
    internal sealed class CharacterTriggerSoundPlayer : ASoundPlayerWithType<SCharacterTriggerSound, SCharacterTriggerSound.Timing>
    {
        private AudioSource[] audioSources = null;

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
                volume: Param.Volume
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
