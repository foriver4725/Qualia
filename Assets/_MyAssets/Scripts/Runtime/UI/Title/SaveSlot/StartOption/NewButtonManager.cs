using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.StartOption
{
    internal sealed class NewButtonManager : AButtonManager
    {
        private protected sealed override void OnClickSucceeded()
        {
            StartSettings.IsNewGame = true;
            StateRootObjectManager.Instance.ChangeState(State.Confirm);
        }
    }
}
