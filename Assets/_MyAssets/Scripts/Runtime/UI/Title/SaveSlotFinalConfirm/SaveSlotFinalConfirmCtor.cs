using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title
{
    internal sealed class SaveSlotFinalConfirmCtor : AViewConstructor
    {
        [SerializeField] private Button.ASelectableButtonManager defaultSelectedButton;
        [SerializeField] private Button.ASelectableButtonManager[] allButtons;

        [SerializeField] private TextMeshProUGUI labelText;

        internal sealed override void Construct()
        {
            defaultSelectedButton.SelectThisForciblyUnsafe();

            labelText.text = PlayOptions.IsNewGame switch
            {
                true => SaveLoadManager.Data.Slots[PlayOptions.SlotIndex].IsValid switch
                {
                    true => "既にあるデータは上書きされます。\n本当に最初から始めますか？",
                    _ => "本当に最初から始めますか？",
                },
                _ => "このデータで続きから始めますか？",
            };
        }

        internal sealed override void Deconstruct()
        {
            foreach (var button in allButtons)
            {
                if (button.IsSelected)
                    button.DeselectThisForciblyUnsafe();
            }
        }
    }
}
