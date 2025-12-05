using MyScripts.Common.SaveSystem;
using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Confirm
{
    internal sealed class YesButtonManager : AButtonManager
    {
        // 何回でも再設定可能
        // ボタンクリック時、その瞬間の値を見て処理する
        internal int SlotIndex { get; set; } = 0;
        internal bool IsNewGame { get; set; } = true;

        private protected sealed override void OnClickSucceeded()
        {
            // UI で隠すけど、一応多重クリック防止
            if (LoadManager.Instance.HasBegun) return;

            // UI で隠す
            StateRootObjectManager.Instance.ChangeState(State.HideAll);

            // セーブデータの状態を更新する
            {
                // インゲームなどで使うために、選択したセーブスロットを記録しておく
                // ここでのみ書き込みする想定
                Variables.CurrentSlotIndex = SlotIndex;

                // 最初からなので、セーブデータリセット
                if (IsNewGame)
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
