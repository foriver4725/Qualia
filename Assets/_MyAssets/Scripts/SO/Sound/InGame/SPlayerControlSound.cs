namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_PlayerControlSound", menuName = "SO/Sound/InGame/Player Control")]
    internal sealed class SPlayerControlSound : ASSoundWithType<SPlayerControlSound.Action>
    {
        [SerializeField] private AudioClip inertiaJump;

        internal enum Action : byte
        {
            InertiaJump,
        }

        internal sealed override AudioClip GetClip(Action type) => type switch
        {
            Action.InertiaJump => inertiaJump,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
