namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class BackToDesktopButtonManager : ACustomFontSizeButtonManager
    {
        private protected sealed override void OnClickSucceeded()
        {
            if (GuardFlag.IsLocked) return;
            GuardFlag.IsLocked = true;

            GameQuitter.InvokeQuit();
        }
    }
}
