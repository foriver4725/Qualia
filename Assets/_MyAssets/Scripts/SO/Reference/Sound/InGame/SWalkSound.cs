namespace MyScripts.SO.Reference
{
    [CreateAssetMenu(fileName = "_WalkSound", menuName = "SO/Reference/WalkSound")]
    internal sealed class SWalkSound : ScriptableObject
    {
        [SerializeField] private AudioClip grass;
        [SerializeField] private AudioClip sand;
        [SerializeField] private AudioClip rock;
        [SerializeField] private AudioClip water;

        internal enum Surface : byte
        {
            Unknown = 0,
            Grass = 1,
            Sand = 2,
            Rock = 3,
            Water = 4,
        }

        internal AudioClip GetClip(Surface surface) => surface switch
        {
            Surface.Grass => grass,
            Surface.Sand => sand,
            Surface.Rock => rock,
            Surface.Water => water,
            _ => grass, // default
        };
    }
}
