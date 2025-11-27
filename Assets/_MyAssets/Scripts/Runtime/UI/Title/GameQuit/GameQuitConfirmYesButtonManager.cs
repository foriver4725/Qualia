namespace MyScripts.Runtime.UI.Title.GameQuit
{
    internal sealed class GameQuitConfirmYesButtonManager : AButtonManager
    {
        private void Update()
        {
            if (GameQuitter.HasInvoked == false && InputManager.OutGame.Submit)
            {
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
            => GameQuitter.InvokeQuit();
    }
}
