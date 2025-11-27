using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title.SelectSaveSlotUI
{
    /// <summary>
    /// 「はい」ボタンを押すとゲームが開始するので、ある種マネージャー的な感じ
    /// </summary>
    internal sealed class SubmitConfirmYesButtonManager : AButtonManager
    {
        [SerializeField] private TextMeshProUGUI labelText;

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

        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.OptionConfirm && InputManager.OutGame.Submit)
            {
                InputManager.OutGame.MakeSubmitInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
        {
            if (LoadManager.Instance.HasBegun) return;

            // ロードには時間がかかるので、その間にクリックされて選択状態が変わらないように、
            // この時点の値を保存しておく
            int slotIndex = this.slotIndex;
            int optionIndex = this.optionIndex;

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

            LoadManager.Instance.BeginLoad(Scene.Main);
        }
    }
}
