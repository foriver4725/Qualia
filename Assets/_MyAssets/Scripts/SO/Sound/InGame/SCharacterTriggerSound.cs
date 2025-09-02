namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_CharacterTriggerSound", menuName = "SO/Sound/InGame/Character Trigger")]
    internal sealed class SCharacterTriggerSound : ASSound
    {
        [SerializeField] private AudioClip begin;
        [SerializeField] private AudioClip closeToEnd;
        [SerializeField] private AudioClip end;

        internal enum Timing : byte
        {
            Begin,
            CloseToEnd,
            End,
        }

        internal AudioClip GetClip(Timing timing) => timing switch
        {
            Timing.Begin => begin,
            Timing.CloseToEnd => closeToEnd,
            Timing.End => end,
            _ => throw new ArgumentOutOfRangeException(nameof(timing), timing, null)
        };
    }
}
