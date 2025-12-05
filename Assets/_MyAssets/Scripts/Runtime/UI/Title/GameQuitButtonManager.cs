namespace MyScripts.Runtime.UI.Title
{
    internal sealed class GameQuitButtonManager : Button.AButtonManager
    {
        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.None && InputManager.OutGame.Cancel)
            {
                InputManager.OutGame.MakeCancelInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            UIActivationManager.Instance.SetActive(UIActivationManager.UI.GameQuitConfirm, true);
        }
    }
}
