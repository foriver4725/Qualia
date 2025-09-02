namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_SOSSound", menuName = "SO/Sound/InGame/SOS Sound")]
    internal sealed class SSOSSound : ASSoundWithType<SSOSSound.Situation>
    {
        [SerializeField] private AudioClip couldRemove;
        [SerializeField] private AudioClip couldNotRemove;

        internal enum Situation : byte
        {
            CouldRemove,
            CouldNotRemove,

            Count,
        }

        internal sealed override AudioClip GetClip(Situation situation) => situation switch
        {
            Situation.CouldRemove => couldRemove,
            Situation.CouldNotRemove => couldNotRemove,
            _ => throw new ArgumentOutOfRangeException(nameof(situation), situation, null)
        };
    }
}
