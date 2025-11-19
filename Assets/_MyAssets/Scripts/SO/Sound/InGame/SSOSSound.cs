namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_SOSSound", menuName = "SO/Sound/InGame/SOS")]
    internal sealed class SSOSSound : ASSoundWithType<SSOSSound.Situation>
    {
        [SerializeField] private AudioClip couldRemove;
        [SerializeField] private AudioClip couldNotRemove;

        internal enum Situation : byte
        {
            CouldRemove,
            CouldNotRemove,
        }

        internal sealed override AudioClip GetClip(Situation type) => type switch
        {
            Situation.CouldRemove => couldRemove,
            Situation.CouldNotRemove => couldNotRemove,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
