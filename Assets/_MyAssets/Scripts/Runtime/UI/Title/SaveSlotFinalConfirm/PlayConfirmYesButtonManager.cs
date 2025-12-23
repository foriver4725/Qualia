using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title
{
    internal sealed class PlayConfirmYesButtonManager : Button.ASelectableButtonWithFrameManager
    {
        private protected sealed override bool IsFrontUI => StateManager.Instance.State == State.SaveSlot_FinalConfirm;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            // UI で隠すけど、一応多重クリック防止
            if (LoadManager.Instance.HasBegun) return;

            // UI で隠す
            StateManager.Instance.ChangeState(State.HidingAll);

            // セーブデータの状態を更新する
            {
                // インゲームなどで使うために、選択したセーブスロットを記録しておく
                // ここでのみ書き込みする想定
                Variables.CurrentSlotIndex = PlayOptions.SlotIndex;

                // 最初からなので、セーブデータリセット
                if (PlayOptions.IsNewGame)
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
