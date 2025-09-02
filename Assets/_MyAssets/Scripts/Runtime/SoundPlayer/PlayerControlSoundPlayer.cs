namespace MyScripts.Runtime
{
    internal sealed class PlayerControlSoundPlayer : ASoundPlayerWithType<SPlayerControlSound, SPlayerControlSound.Action>
    {
        private AudioSource[] audioSources = null;

        internal sealed override void LetPlay(SPlayerControlSound.Action type)
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
            byte soundAmount = (byte)SSOSSound.Situation.Count;

            audioSources = new AudioSource[soundAmount];

            for (int i = 0; i < soundAmount; i++)
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
