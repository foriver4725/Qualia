namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_GameConfig", menuName = "SO/OutGame/Game Config")]
    internal sealed class SGameConfig : ScriptableObject
    {
        [SerializeField] private bool doesAlwaysShowGamepadButtonFrame = false;
        internal bool DoesAlwaysShowGamepadButtonFrame => doesAlwaysShowGamepadButtonFrame;
    }
}
