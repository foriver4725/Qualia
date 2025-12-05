using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.StartOption
{
    internal sealed class ContinueButtonManager : ASelectableButtonWithFrameManager
    {
        private protected sealed override bool IsFrontUI =>
           UIActivationManager.Instance.Front == UIActivationManager.UI.SaveSlot
           && StateRootObjectManager.Instance.State == State.StartOption;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            StartSettings.IsNewGame = false;
            StateRootObjectManager.Instance.ChangeState(State.Confirm);
        }
    }
}
