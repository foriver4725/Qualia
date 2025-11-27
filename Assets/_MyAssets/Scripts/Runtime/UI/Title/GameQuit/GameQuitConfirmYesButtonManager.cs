namespace MyScripts.Runtime.UI.Title.GameQuit
{
    internal sealed class GameQuitConfirmYesButtonManager : AButtonManager
    {
        private protected sealed override void OnClickSucceeded()
            => GameQuitter.InvokeQuit();
    }
}
