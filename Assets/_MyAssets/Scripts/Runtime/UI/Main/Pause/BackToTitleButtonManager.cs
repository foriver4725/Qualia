namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class BackToTitleButtonManager : ASelectableButtonManager
    {
        [SerializeField] private ConfirmYesButtonManager confirmYesButtonManager;

        private protected sealed override UIActivationManager.UI LocatedUI => UIActivationManager.UI.Pause;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            confirmYesButtonManager.InjectInvokeAction(ConfirmYesButtonManager.InvokeAction.BackToTitle);
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.OnPauseConfirm, true);
        }
    }
}
