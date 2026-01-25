namespace MyScripts.Runtime.UI.Title
{
    internal sealed class CreditBackButtonManager : Button.ASelectableButtonWithFrameManager
    {
        private protected sealed override bool IsFrontUI => StateManager.Instance.State == State.Credit;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            StateManager.Instance.ChangeState(State.Default);
        }
    }
}
