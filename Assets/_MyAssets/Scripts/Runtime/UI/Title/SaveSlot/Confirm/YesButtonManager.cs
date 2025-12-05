using MyScripts.Common.SaveSystem;
using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Confirm
{
    internal sealed class YesButtonManager : AButtonManager
    {
        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.SaveSlot
                && StateRootObjectManager.Instance.State == State.Confirm
                && InputManager.OutGame.Submit)
            {
                InputManager.OutGame.MakeSubmitInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            // UI で隠すけど、一応多重クリック防止
            if (LoadManager.Instance.HasBegun) return;

            // UI で隠す
            StateRootObjectManager.Instance.ChangeState(State.HideAll);

            // セーブデータの状態を更新する
            {
                // インゲームなどで使うために、選択したセーブスロットを記録しておく
                // ここでのみ書き込みする想定
                Variables.CurrentSlotIndex = StartSettings.SlotIndex;

                // 最初からなので、セーブデータリセット
                if (StartSettings.IsNewGame)
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
