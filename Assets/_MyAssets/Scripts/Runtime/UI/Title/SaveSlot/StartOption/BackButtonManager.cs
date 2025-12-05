using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.StartOption
{
    internal sealed class BackButtonManager : AButtonManager
    {
        private protected sealed override void OnClickSucceeded()
        {
            StateRootObjectManager.Instance.ChangeState(State.Select);
        }
    }
}
