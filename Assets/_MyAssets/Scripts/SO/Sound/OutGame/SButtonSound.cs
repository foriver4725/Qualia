namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_ButtonSound", menuName = "SO/Sound/OutGame/Button")]
    internal sealed class SButtonSound : ASSoundWithType<SButtonSound.Action>
    {
        [SerializeField] private AudioClip hover;
        [SerializeField] private AudioClip click;

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
    }
}
