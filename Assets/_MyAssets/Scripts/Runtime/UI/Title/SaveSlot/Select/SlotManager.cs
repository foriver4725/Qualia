using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Select
{
    internal sealed class SlotManager : AButtonManager
    {
        [SerializeField, Range(0, 10)] private int slotIndex = 0;

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            StartSettings.SlotIndex = slotIndex;
            StateRootObjectManager.Instance.ChangeState(State.StartOption);
        }
    }
}
