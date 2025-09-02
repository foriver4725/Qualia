namespace MyScripts.SO.Reference
{
    [CreateAssetMenu(fileName = "_WalkSound", menuName = "SO/Reference/WalkSound")]
    internal sealed class SWalkSound : ScriptableObject
    {
        [SerializeField] private AudioMixerGroup group;
        [Space(10)]
        [SerializeField] private AudioClip grass;
        [SerializeField] private AudioClip sand;
        [SerializeField] private AudioClip rock;
        [SerializeField] private AudioClip water;
        [Space(10)]
        [SerializeField, Range(1, 16), Tooltip("同時に鳴る足音の最大数")] private byte maxSoundAmount = 8;
        [SerializeField, Range(0.0f, 0.5f), Tooltip("足音がフェードアウトするまでの時間")] private float fadeOutDuration = 0.2f;
        [Space(10)]
        [SerializeField, Range(0.0f, 1.0f)] private float volume = 1.0f;
        [SerializeField, Range(0.5f, 2.0f)] private float walkPitch = 1.2f;
        [SerializeField, Range(0.5f, 2.0f)] private float sprintPitch = 1.5f;

        internal AudioMixerGroup Group => group;
        internal byte MaxSoundAmount => maxSoundAmount;
        internal float FadeOutDuration => fadeOutDuration;
        internal float Volume => volume;
        internal float WalkPitch => walkPitch;
        internal float SprintPitch => sprintPitch;

        internal enum Surface : byte
        {
            None,
            Grass,
            Sand,
            Rock,
            Water,
        }

        internal AudioClip GetClip(Surface surface) => surface switch
        {
            Surface.None => null,
            Surface.Grass => grass,
            Surface.Sand => sand,
            Surface.Rock => rock,
            Surface.Water => water,
            _ => throw new ArgumentOutOfRangeException(nameof(surface), surface, null)
        };
    }
}
