using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Confirm
{
    internal sealed class ViewConstructor : AViewConstructor
    {
        [SerializeField] private TextMeshProUGUI labelText;

        internal sealed override void Construct()
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

        internal sealed override void Deconstruct()
        {
        }
    }
}
