namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class BackToDesktopButtonManager : ACustomFontSizedSelectableButtonManager
    {
        [SerializeField] private Canvas onPauseConfirmUi;
        [SerializeField] private ConfirmYesButtonManager confirmYesButtonManager;

        private protected sealed override void OnSubmittedWithSelection()
        {
            if (onPauseConfirmUi.gameObject.activeSelf == false)
            {
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
        {
            confirmYesButtonManager.InjectInvokeAction(ConfirmYesButtonManager.InvokeAction.BackToDesktop);
            onPauseConfirmUi.gameObject.SetActive(true);
        }
    }
}
