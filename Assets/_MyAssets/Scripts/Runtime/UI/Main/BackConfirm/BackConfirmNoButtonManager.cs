namespace MyScripts.Runtime.UI.Main
{
    internal sealed class BackConfirmNoButtonManager : Button.ASelectableButtonWithFrameManager
    {
        private protected sealed override bool IsFrontUI => StateManager.Instance.State == State.Back_Confirm;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            StateManager.Instance.ChangeState(State.Pause);
        }
    }
}
