using UnityEngine.EventSystems;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Select
{
    internal sealed class SlotManager : MonoBehaviour
    {
        [SerializeField] private EventTrigger raycastedBg;
        [SerializeField, Range(0, 10)] private int slotIndex = 0;

        private void Awake()
        {
            raycastedBg.AddListener(EventTriggerType.PointerClick, _ =>
            {
                StartSettings.SlotIndex = slotIndex;
                StateRootObjectManager.Instance.ChangeState(State.StartOption);
            });
        }
    }
}
