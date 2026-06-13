namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_ButtonSound", menuName = "SO/Sound/OutGame/Button")]
    internal sealed class SButtonSound : ASSoundWithType<SButtonSound.Action>
    {
        [SerializeField] private AudioClip hover;
        [SerializeField] private AudioClip click;
        [SerializeField] private AK.Wwise.Event Play_Button;
        [SerializeField] private AK.Wwise.Switch Hover;
        [SerializeField] private AK.Wwise.Switch Click;
        internal enum Action : byte
        {
            Hover,
            Click,
        }

        internal sealed override AudioClip GetClip(Action type) => type switch
        {
            Action.Hover => hover,
            Action.Click => click,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Switch GetSwitch(Action type) => type switch
        {
            Action.Hover => Hover,
            Action.Click => Click,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Event GetEvent()
        {
            return Play_Button;
        }
    }
}
