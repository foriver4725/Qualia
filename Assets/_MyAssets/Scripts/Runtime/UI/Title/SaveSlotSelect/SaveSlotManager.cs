namespace MyScripts.Runtime.UI.Title
{
    internal sealed class SaveSlotManager : Button.ASelectableButtonWithFrameManager
    {
        [SerializeField, Range(0, 10)] private int slotIndex = 0;

        private protected sealed override bool IsFrontUI => StateManager.Instance.State == State.SaveSlot_Select;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            PlayOptions.SlotIndex = slotIndex;
            StateManager.Instance.ChangeState(State.SaveSlot_Select_PlayOption);
        }
    }
}
