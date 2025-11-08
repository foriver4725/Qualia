namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_Button", menuName = "SO/Sound/OutGame/Button")]
    internal sealed class SButton : ASSoundWithType<SButton.Action>
    {
        [SerializeField] private AudioClip hover;
        [SerializeField] private AudioClip click;

        internal enum Action : byte
        {
            Hover,
            Click,

            Count,
        }

        internal sealed override AudioClip GetClip(Action type) => type switch
        {
            Action.Hover => hover,
            Action.Click => click,
            _            => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
