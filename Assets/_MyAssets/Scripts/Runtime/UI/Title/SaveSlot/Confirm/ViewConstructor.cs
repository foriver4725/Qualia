using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Confirm
{
    // UIが有効になるたびに実行するべき
    // 現在の数値を基に、見た目を再構成する
    internal sealed class ViewConstructor : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;

        internal void Construct()
        {
            labelText.text = StartSettings.IsNewGame switch
            {
                true => SaveLoadManager.Data.Slots[StartSettings.SlotIndex].IsValid switch
                {
                    true => "既にあるデータは上書きされます。\n本当に最初から始めますか？",
                    _ => "本当に最初から始めますか？",
                },
                _ => "このデータで続きから始めますか？",
            };
        }
    }
}
