using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Confirm
{
    internal sealed class NoButtonManager : AButtonManager
    {
        private protected sealed override void OnClickSucceeded()
        {
            StateRootObjectManager.Instance.ChangeState(State.StartOption);
        }
    }
}
