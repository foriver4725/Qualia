namespace MyScripts.SO
{
    internal abstract class ASSound : ScriptableObject
    {
        [SerializeField] private AudioMixerGroup group;
        [SerializeField, Range(0.0f, 1.0f)] private float volume = 1.0f;

        internal AudioMixerGroup Group => group;
        internal float Volume => volume;
    }

    // 最もよく使う
    internal abstract class ASSoundWithType<TClipType> : ASSound
        where TClipType : Enum
    {
        internal abstract AudioClip GetClip(TClipType type);
    }
}
