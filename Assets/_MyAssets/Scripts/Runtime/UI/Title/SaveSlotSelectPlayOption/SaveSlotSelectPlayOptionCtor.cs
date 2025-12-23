using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title
{
    internal sealed class SaveSlotSelectPlayOptionCtor : AViewConstructor
    {
        // コンティニュー・新規の優先度で、初期選択する
        [SerializeField] private Button.ASelectableButtonManager newButtonManager;
        [SerializeField] private Button.ASelectableButtonManager continueButtonManager;

        internal sealed override void Construct()
        {
            if (SaveLoadManager.Data.Slots[PlayOptions.SlotIndex].IsValid)
            {
                continueButtonManager.SelectThisForciblyUnsafe();
                continueButtonManager.Parent.gameObject.SetActive(true);
            }
            else
            {
                newButtonManager.SelectThisForciblyUnsafe();
                continueButtonManager.Parent.gameObject.SetActive(false);
            }
        }

        internal sealed override void Deconstruct()
        {
            if (newButtonManager.IsSelected)
                newButtonManager.DeselectThisForciblyUnsafe();
            if (continueButtonManager.IsSelected)
                continueButtonManager.DeselectThisForciblyUnsafe();
        }
    }
}
