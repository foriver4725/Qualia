using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title.SelectSaveSlotUI
{
    /// <summary>
    /// 「はい」ボタンを押すとゲームが開始するので、ある種マネージャー的な感じ
    /// </summary>
    internal sealed class SubmitConfirmYesButtonManager : AButtonManager
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private Canvas blockRaycastUI;

        private int slotIndex = 0;
        private int optionIndex = 0;

        // ボタンの処理が走る前に、確実に呼んでほしい
        // ボタンを非アクティブ → これを呼ぶ → ボタンをアクティブ とか
        internal void InjectIndices(int slotIndex, int optionIndex)
        {
            this.slotIndex = slotIndex;
            this.optionIndex = optionIndex;

            // ラベルの文字を適切に変更する
            labelText.text = optionIndex switch
            {
                0 => "既にあるデータは上書きされます。\n本当に最初から始めますか？",
                1 => "このデータで続きから始めますか？",
                _ => throw new ArgumentOutOfRangeException(nameof(optionIndex), optionIndex, null)
            };
        }

        private protected sealed override void OnClickSucceeded()
        {
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
                SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].IsValid |= true;
            }

            blockRaycastUI.gameObject.SetActive(true);

            LoadManager.Instance.BeginLoad(Scene.Main);
        }
    }
}
