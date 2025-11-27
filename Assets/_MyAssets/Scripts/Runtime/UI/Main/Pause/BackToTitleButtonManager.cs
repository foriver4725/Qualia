namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class BackToTitleButtonManager : ACustomFontSizeButtonManager
    {
        [SerializeField] private Canvas onPauseConfirmUi;
        [SerializeField] private ConfirmYesButtonManager confirmYesButtonManager;

        private protected sealed override void OnClickSucceeded()
        {
            confirmYesButtonManager.InjectInvokeAction(ConfirmYesButtonManager.InvokeAction.BackToTitle);
            onPauseConfirmUi.gameObject.SetActive(true);
        }
    }
}
