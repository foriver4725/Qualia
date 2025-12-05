using MyScripts.Common.SaveSystem;
using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.StartOption
{
    // UIが有効になるたびに実行するべき
    // 現在の数値を基に、見た目を再構成する
    internal sealed class ViewConstructor : MonoBehaviour
    {
        [SerializeField] private NewButtonManager newButtonManager;
        [SerializeField] private ContinueButtonManager continueButtonManager;

        internal void Construct()
        {
            if (SaveLoadManager.Data.Slots[StartSettings.SlotIndex].IsValid)
            {
                continueButtonManager.Parent.gameObject.SetActive(true);
                SelectFrameManager.Instance.Reselect(continueButtonManager);
            }
            else
            {
                continueButtonManager.Parent.gameObject.SetActive(false);
                SelectFrameManager.Instance.Reselect(newButtonManager);
            }
        }
    }
}
