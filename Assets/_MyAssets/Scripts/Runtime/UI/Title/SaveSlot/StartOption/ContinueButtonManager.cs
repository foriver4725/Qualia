using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.StartOption
{
    internal sealed class ContinueButtonManager : AButtonManager
    {
        private protected sealed override void OnClickSucceeded()
        {
            StartSettings.IsNewGame = false;
            StateRootObjectManager.Instance.ChangeState(State.Confirm);
        }
    }
}
