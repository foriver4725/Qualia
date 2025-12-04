namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ConfirmNoButtonManager : Button.AButtonManager
    {
        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.OnPauseConfirm && InputManager.OutGame.Cancel)
            {
                InputManager.OutGame.MakeCancelInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
            => UIActivationManager.Instance.SetActive(UIActivationManager.UI.OnPauseConfirm, false);
    }
}
