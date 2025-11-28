namespace MyScripts.Runtime.UI.Main.Pause
{
    /// <summary>
    /// OnJustBeforeAwake(), OnJustAfterAwake(), Update() を使用
    /// </summary>
    internal abstract class ACustomFontSizedSelectableButtonManager : ASelectableButtonManager
    {
        [SerializeField, Range(0.0f, 360.0f)] private float fontSize = 120.0f;

        private protected override void OnJustAfterAwake()
            => this.Text.fontSize = fontSize;
    }
}
