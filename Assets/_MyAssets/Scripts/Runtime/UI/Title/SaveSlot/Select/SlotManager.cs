namespace MyScripts.Runtime.UI.Title.SaveSlot.Select
{
    internal sealed class SlotManager : MonoBehaviour
    {
        [SerializeField, Range(0, 10)] private int slotIndex = 0;

        private void OnClicked()
        {
            StartSettings.SlotIndex = slotIndex;
            StateRootObjectManager.Instance.ChangeState(State.StartOption);
        }
    }
}
