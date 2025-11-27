namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class BackToTitleButtonManager : ACustomFontSizeButtonManager
    {
        private protected sealed override void OnClickSucceeded()
        {
            if (GuardFlag.IsLocked) return;
            GuardFlag.IsLocked = true;

            LoadManager.Instance.BeginLoad(Scene.Title);
        }
    }
}
