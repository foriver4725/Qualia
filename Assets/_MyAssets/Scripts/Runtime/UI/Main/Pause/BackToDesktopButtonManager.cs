using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class BackToDesktopButtonManager : ASelectableButtonWithFrameManager
    {
        [SerializeField] private AViewConstructor pauseViewConstructor;
        [SerializeField] private ConfirmYesButtonManager confirmYesButtonManager;
        [SerializeField] private RectTransform parentRow;

        private protected sealed override float HoveredScaleCoef => 1.5f;
        private protected sealed override Vector2 AnchoredPositionOffset => parentRow.anchoredPosition;

        private protected sealed override bool IsFrontUI => UIActivationManager.Instance.Front == UIActivationManager.UI.Pause;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            pauseViewConstructor.Deconstruct();
            confirmYesButtonManager.InjectInvokeAction(ConfirmYesButtonManager.InvokeAction.BackToDesktop);
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.OnPauseConfirm, true);
        }
    }
}
