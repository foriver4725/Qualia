namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_CharacterTriggerSound", menuName = "SO/Sound/InGame/Character Trigger")]
    internal sealed class SCharacterTriggerSound : ASSoundWithType<SCharacterTriggerSound.Timing>
    {
        [SerializeField] private AudioClip begin;
        [SerializeField] private AudioClip closeToEnd;
        [SerializeField, Range(0.0f, 5.0f), Tooltip("CloseToEnd の効果音を、開始何秒から鳴らすか")] private float closeToEndTimeOffset = 1.0f;
        [SerializeField] private AK.Wwise.Event Play_CharacterTrigger;
        [SerializeField] private AK.Wwise.Switch Begin;
        [SerializeField] private AK.Wwise.Switch CloseToEnd;

        internal float CloseToEndLength => closeToEnd.length;
        internal float CloseToEndTimeOffset => closeToEndTimeOffset;

        internal enum Timing : byte
        {
            Begin,
            CloseToEnd,
        }

        internal sealed override AudioClip GetClip(Timing type) => type switch
        {
            Timing.Begin => begin,
            Timing.CloseToEnd => closeToEnd,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Switch GetSwitch(Timing type) => type switch
        {
            Timing.Begin => Begin,
            Timing.CloseToEnd => CloseToEnd,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Event GetEvent()
        {
            return Play_CharacterTrigger;
        }
    }
}
