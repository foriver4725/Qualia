namespace MyScripts.Runtime
{
    internal sealed class SOSSoundPlayer : ASoundPlayerWithType<SSOSSound, SSOSSound.Situation>
    {
        private AudioSource[] audioSources = null;

        internal sealed override void LetPlay(SSOSSound.Situation type)
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
