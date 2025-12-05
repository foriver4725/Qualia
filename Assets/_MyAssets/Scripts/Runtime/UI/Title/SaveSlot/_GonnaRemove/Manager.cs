using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title.SaveSlot
{
    internal sealed class Manager : MonoBehaviour
    {
        [SerializeField] private Image slotPointer;
        [SerializeField] private Image optionPointer;
        [SerializeField] private TextMeshProUGUI slotDescText;
        [SerializeField] private OptionButtonManager optionContinueButton;
        [SerializeField] private SubmitConfirmYesButtonManager submitConfirmYesButtonManager;

        internal const int SlotIndexCount = Constants.SlotCount;
        internal const int OptionIndexCount = 2;
        private int slotIndex = 0;
        private int optionIndex = 0;

        private void Awake()
        {
            SetSlotIndex(slotIndex);
            SetOptionIndex(optionIndex);
            UpdateOptionContinueButtonsActiveness(slotIndex);
        }

        public void SetSlotIndex(int index)
        {
            slotIndex = index;

            slotPointer.rectTransform.SetAnchorY(index switch
            {
                0 => 200.0f,
                1 => 0.0f,
                2 => -200.0f,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            });

            SetOptionIndex(0); // スロット変更時にオプションをリセットする

            UpdateSlotDescText(index);
            UpdateOptionContinueButtonsActiveness(index);
        }

        public void SetOptionIndex(int index)
        {
            optionIndex = index;

            optionPointer.rectTransform.SetAnchorY(index switch
            {
                0 => 0.0f,
                1 => -200.0f,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            });
        }

        // 選択を決定し、シーン遷移する
        // 確認UIがあるので、実際はそこに処理を委譲する
        public void Submit()
        {
            // 必要な変数を注入してから、最終的にUIを表示する
            // これにより、ボタンの任意の処理が走る時点で、確実に必要な変数が揃っていることを保証する
            submitConfirmYesButtonManager.InjectIndices(slotIndex, optionIndex);
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.OptionConfirm, true);
        }

        private void UpdateOptionContinueButtonsActiveness(int slotIndex)
        {
            bool isActive = SaveLoadManager.Data.Slots[slotIndex].IsValid;
            optionContinueButton.SetActive(isActive);
        }

        private void UpdateSlotDescText(int slotIndex)
        {
            var slot = SaveLoadManager.Data.Slots[slotIndex];
            if (slot.IsValid)
            {
                // 穢れ度を計算する
                int leftCount = 0;
                foreach (bool hasFound in slot.HasFoundSOSSigns.AsSpan())
                {
                    if (!hasFound)
                        leftCount++;
                }
                float leftRatio = 100.0f * leftCount / Constants.SOSSignCount;

                slotDescText.SetTextFormat("穢れ度 : {0:F2}%", leftRatio);
            }
            else
            {
                slotDescText.text = "データ無し";
            }
        }
    }
}
