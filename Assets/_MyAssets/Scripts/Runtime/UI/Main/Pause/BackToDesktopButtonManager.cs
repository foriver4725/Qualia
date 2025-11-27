namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class BackToDesktopButtonManager : ACustomFontSizeButtonManager
    {
        [SerializeField] private Canvas onPauseConfirmUi;
        [SerializeField] private ConfirmYesButtonManager confirmYesButtonManager;

        private protected sealed override void OnClickSucceeded()
        {
            confirmYesButtonManager.InjectInvokeAction(ConfirmYesButtonManager.InvokeAction.BackToDesktop);
            onPauseConfirmUi.gameObject.SetActive(true);
        }
    }
}
