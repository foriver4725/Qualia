namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_PlayerControlSound", menuName = "SO/Sound/InGame/Player Control")]
    internal sealed class SPlayerControlSound : ASSoundWithType<SPlayerControlSound.Action>
    {
        [SerializeField] private AudioClip inertiaJump;
        [SerializeField] private AK.Wwise.Event Play_InertiaJump;
        [SerializeField] private AK.Wwise.Switch InertiaJump;

        internal enum Action : byte
        {
            InertiaJump,
        }

        internal sealed override AudioClip GetClip(Action type) => type switch
        {
            Action.InertiaJump => inertiaJump,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Switch GetSwitch(Action type) => type switch
        {
            Action.InertiaJump => InertiaJump,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Event GetEvent()
        {
            return Play_InertiaJump;
        }


    }
}
