using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.StartOption
{
    internal sealed class NewButtonManager : AButtonManager
    {
        // New or Continue を伝えるために、参照を持つ
        [SerializeField] private Confirm.YesButtonManager yesButtonManager;

        private protected sealed override void OnClickSucceeded()
        {
            StateRootObjectManager.Instance.ChangeState(State.Confirm);
        }
    }
}
