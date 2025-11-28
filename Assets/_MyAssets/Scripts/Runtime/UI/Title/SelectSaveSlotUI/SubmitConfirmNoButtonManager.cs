namespace MyScripts.Runtime.UI.Title.SelectSaveSlotUI
{
    internal sealed class SubmitConfirmNoButtonManager : Button.AButtonManager
    {
        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.OptionConfirm && InputManager.OutGame.Cancel)
            {
                InputManager.OutGame.MakeCancelInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
            => UIActivationManager.Instance.SetActive(UIActivationManager.UI.OptionConfirm, false);
    }
}
