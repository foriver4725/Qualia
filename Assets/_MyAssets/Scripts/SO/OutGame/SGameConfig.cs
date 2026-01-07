namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_GameConfig", menuName = "SO/OutGame/Game Config")]
    internal sealed class SGameConfig : ScriptableObject
    {
        [SerializeField, Range(0.1f, 600.0f)] private float screenshotCaptureInterval = 30.0f;
        internal float ScreenshotCaptureInterval => screenshotCaptureInterval;

        [SerializeField] private bool doesAlwaysShowGamepadButtonFrame = false;
        internal bool DoesAlwaysShowGamepadButtonFrame => doesAlwaysShowGamepadButtonFrame;

        [SerializeField] private bool doesPlayIntroCutScene = true;
        internal bool DoesPlayIntroCutScene => doesPlayIntroCutScene;

        [SerializeField] private bool doesPlayAnimaDescCutScene = true;
        internal bool DoesPlayAnimaDescCutScene => doesPlayAnimaDescCutScene;
    }
}
