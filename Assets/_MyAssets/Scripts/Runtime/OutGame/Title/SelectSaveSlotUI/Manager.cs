using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.OutGame.Title.SelectSaveSlotUI
{
    internal sealed class Manager : MonoBehaviour
    {
        [SerializeField] private Image slotPointer;
        [SerializeField] private Image optionPointer;
        [SerializeField] private GameObject optionContinueButton;
        [SerializeField] private GameObject bgRaycastedAfterSubmitted;

        internal const int SlotIndexCount = Constants.SlotCount;
        internal const int OptionIndexCount = 2;
        private int slotIndex = 0;
        private int optionIndex = 0;

        private void Awake()
        {
            bgRaycastedAfterSubmitted.SetActive(false);

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
        public void Submit()
        {
            bgRaycastedAfterSubmitted.SetActive(true);

            // セーブデータの状態を更新する
            {
                // インゲームなどで使うために、選択したセーブスロットを記録しておく
                // ここでのみ書き込みする想定
                Variables.CurrentSlotIndex = slotIndex;

                // 最初からなので、セーブデータリセット
                if (optionIndex == 0)
                {
                    SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex] = SaveLoadInvoker.CreateDefaultSingleData();
                }

                // セーブデータがリセットされようとされまいと、
                // 最終的にこのセーブスロットは「セーブデータが入っている」状態となる
                SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].IsValid = true;
            }

            LoadManager.Instance.BeginLoad(Scene.Main);
        }

        private void UpdateOptionContinueButtonsActiveness(int slotIndex)
        {
            bool isActive = SaveLoadManager.Data.Slots[slotIndex].IsValid;
            optionContinueButton.SetActive(isActive);
        }
    }
}
