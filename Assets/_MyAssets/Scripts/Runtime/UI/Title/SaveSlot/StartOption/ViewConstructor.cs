using MyScripts.Common.SaveSystem;
using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.StartOption
{
    internal sealed class ViewConstructor : AViewConstructor
    {
        // 最初に選択したり、無効なものを非表示にしたりするため
        [SerializeField] private ASelectableButtonManager newButtonManager;
        [SerializeField] private ASelectableButtonManager continueButtonManager;

        internal sealed override void Construct()
        {
            if (SaveLoadManager.Data.Slots[StartSettings.SlotIndex].IsValid)
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
            if (newButtonManager.IsSelected) newButtonManager.DeselectThisForciblyUnsafe();
            if (continueButtonManager.IsSelected) continueButtonManager.DeselectThisForciblyUnsafe();
        }
    }
}
