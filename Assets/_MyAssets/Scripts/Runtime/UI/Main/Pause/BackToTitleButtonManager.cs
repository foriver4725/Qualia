namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class BackToTitleButtonManager : ACustomFontSizedSelectableButtonManager
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
            confirmYesButtonManager.InjectInvokeAction(ConfirmYesButtonManager.InvokeAction.BackToTitle);
            onPauseConfirmUi.gameObject.SetActive(true);
        }
    }
}
