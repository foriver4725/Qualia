namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_CharacterTriggerSound", menuName = "SO/Sound/InGame/Character Trigger")]
    internal sealed class SCharacterTriggerSound : ASSoundWithType<SCharacterTriggerSound.Timing>
    {
        [SerializeField] private AudioClip begin;
        [SerializeField] private AudioClip closeToEnd;

        internal enum Timing : byte
        {
            Begin,
            CloseToEnd,

            Count,
        }

        internal sealed override AudioClip GetClip(Timing timing) => timing switch
        {
            Timing.Begin => begin,
            Timing.CloseToEnd => closeToEnd,
            _ => throw new ArgumentOutOfRangeException(nameof(timing), timing, null)
        };

        private float closeToEndSoundLength = -1.0f;
        internal float CloseToEndSoundLength => closeToEndSoundLength >= 0.0f ? closeToEndSoundLength : closeToEnd.length;
    }
}
