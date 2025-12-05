namespace MyScripts.Runtime.UI.Title.SaveSlot
{
    internal sealed class SlotButtonManager : ASelectableButtonManager
    {
        [SerializeField, Range(0, Manager.SlotIndexCount - 1)] private int index = 0;
        [SerializeField] private Manager manager;

        internal int Index => index;

        private protected sealed override UIActivationManager.UI LocatedUI => UIActivationManager.UI.SelectSaveSlot;

        private protected sealed override void OnSelectChanged(ASelectableButtonManager currentlySelectedButton)
        {
            if (currentlySelectedButton is OptionButtonManager optionButton)
                manager.SetOptionIndex(optionButton.Index);
            else if (currentlySelectedButton is SlotButtonManager slotButton)
                manager.SetSlotIndex(slotButton.Index);
        }

        private protected sealed override void OnClickSucceeded()
            => manager.SetSlotIndex(index);
    }
}
