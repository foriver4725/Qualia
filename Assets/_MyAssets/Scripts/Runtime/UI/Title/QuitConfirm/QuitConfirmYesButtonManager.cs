namespace MyScripts.Runtime.UI.Title.GameQuit
{
    internal sealed class QuitConfirmYesButtonManager : Button.ASelectableButtonWithFrameManager
    {
        private protected sealed override bool IsFrontUI => StateManager.Instance.State == State.Quit_Confirm;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            GameQuitter.InvokeQuit();
        }
    }
}
