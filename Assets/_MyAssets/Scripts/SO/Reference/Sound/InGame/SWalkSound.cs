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

        internal AudioMixerGroup Group => group;

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
