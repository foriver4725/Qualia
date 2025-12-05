using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class BackToDesktopButtonManager : ASelectableButtonWithFrameManager
    {
        [SerializeField] private ConfirmYesButtonManager confirmYesButtonManager;

        private protected sealed override bool IsFrontUI => UIActivationManager.Instance.Front == UIActivationManager.UI.Pause;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            if (this.IsSelected) this.DeselectThisForciblyUnsafe();
            confirmYesButtonManager.InjectInvokeAction(ConfirmYesButtonManager.InvokeAction.BackToDesktop);
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.OnPauseConfirm, true);
        }
    }
}
