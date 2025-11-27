namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class BackToTitleButtonManager : ACustomFontSizeButtonManager
    {
        private protected sealed override void OnClickSucceeded()
            => LoadManager.Instance.BeginLoad(Scene.Title);
    }
}
