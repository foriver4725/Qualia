namespace MyScripts.Runtime.UI.Title.GameQuit
{
    internal sealed class GameQuitConfirmNoButtonManager : AButtonManager
    {
        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.GameQuitConfirm && InputManager.OutGame.Cancel)
            {
                InputManager.OutGame.MakeCancelInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
        {
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.GameQuitConfirm, false);
        }
    }
}
