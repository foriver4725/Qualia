using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title.SaveSlot.StartOption
{
    // UIが有効になるたびに実行するべき
    // 現在の数値を基に、見た目を再構成する
    internal sealed class ViewConstructor : MonoBehaviour
    {
        // 最初に選択したり、無効なものを非表示にしたりするため
        [SerializeField] private NewButtonManager newButtonManager;
        [SerializeField] private ContinueButtonManager continueButtonManager;

        internal void Construct()
        {
            if (SaveLoadManager.Data.Slots[StartSettings.SlotIndex].IsValid)
            {
                continueButtonManager.Parent.gameObject.SetActive(true);
                continueButtonManager.SelectThisForciblyUnsafe();
            }
            else
            {
                continueButtonManager.Parent.gameObject.SetActive(false);
                newButtonManager.SelectThisForciblyUnsafe();
            }
        }
    }
}
