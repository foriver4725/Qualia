namespace MyScripts.Runtime.UI.Title.SaveSlot.Select
{
    internal sealed class SlotManager : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField, Range(0, 10)] private int slotIndex = 0;

        private void Awake()
        {
            image.OnPointerDownAsObservable()
                .Select(slotIndex, static (_, slotIndex) => slotIndex)
                .Subscribe(static slotIndex =>
                {
                    StartSettings.SlotIndex = slotIndex;
                    StateRootObjectManager.Instance.ChangeState(State.StartOption);
                })
                .AddTo(image);
        }
    }
}
