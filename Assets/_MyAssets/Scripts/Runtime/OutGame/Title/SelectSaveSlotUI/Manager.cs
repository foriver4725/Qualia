namespace MyScripts.Runtime.OutGame.Title.SelectSaveSlotUI
{
    internal sealed class Manager : MonoBehaviour
    {
        [SerializeField] private Image slotPointer;
        [SerializeField] private Image optionPointer;
        [SerializeField] private GameObject bgRaycastedAfterSubmitted;

        internal const int SlotIndexCount = 3;
        internal const int OptionIndexCount = 2;
        private int slotIndex = 0;
        private int optionIndex = 0;

        private void Awake()
        {
            bgRaycastedAfterSubmitted.SetActive(false);

            SetSlotIndex(0);
            SetOptionIndex(0);
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

            // いい感じの処理
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

            // いい感じの処理
        }

        // 選択を決定し、シーン遷移する
        public void Submit()
        {
            bgRaycastedAfterSubmitted.SetActive(true);

            // いい感じの処理

            LoadManager.Instance.BeginLoad(Scene.Main);
        }
    }
}
