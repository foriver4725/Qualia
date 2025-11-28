namespace MyScripts.Runtime.UI.Title.GameQuit
{
    internal sealed class GameQuitConfirmYesButtonManager : Button.AButtonManager
    {
        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.GameQuitConfirm && InputManager.OutGame.Submit)
            {
                if (GameQuitter.HasInvoked == false)
                {
                    InputManager.OutGame.MakeSubmitInputDisabledUntilNextFrame();
                    base.PlayClickSe();
                    this.OnClickSucceeded();
                }
            }
        }

        private protected sealed override void OnClickSucceeded()
            => GameQuitter.InvokeQuit();
    }
}
