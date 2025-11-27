namespace MyScripts.Runtime.UI.Main.Pause
{
    internal abstract class ACustomFontSizeButtonManager : AButtonManager
    {
        [SerializeField, Range(0.0f, 360.0f)] private float fontSize = 120.0f;

        private protected override void OnJustAfterAwake()
            => this.Text.fontSize = fontSize;
    }
}
