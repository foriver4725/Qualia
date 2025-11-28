namespace MyScripts.Runtime.UI.Title.SelectSaveSlotUI
{
    /// <summary>
    /// OnJustBeforeAwake(), Update() を使用<br/>
    /// Previous は上のボタン<br/>
    /// Next は下のボタン<br/>
    /// </summary>
    internal abstract class ASelectableButtonManager : Button.ASelectableButtonManager
    {
        /// <summary>
        /// このボタンがどのUIに所属しているか<br/>
        /// このUIが最前面の場合のみ入力を受け付ける<br/>
        /// </summary>
        private protected abstract UIActivationManager.UI LocatedUI { get; }

        /// <summary>
        /// 選択しているものが変わった時に呼び出される<br/>
        /// 新しく選択されたボタンが引数として渡される<br/>
        /// </summary>
        private protected abstract void OnSelectChanged(ASelectableButtonManager currentlySelectedButton);

        private bool isActive = true;

        internal void SetActive(bool value)
        {
            // どのコンポーネントから辿っても良いが、
            // とにかくボタンのルートごとアクティブ状態を切り替える
            Transform buttonRoot = EventTrigger.transform.parent;
            buttonRoot.gameObject.SetActive(value);

            isActive = value;
        }

        private protected sealed override bool CanSelectLeft => Left != null && Left is ASelectableButtonManager button && button.isActive;
        private protected sealed override bool CanSelectRight => Right != null && Right is ASelectableButtonManager button && button.isActive;
        private protected sealed override bool CanSelectDown => Down != null && Down is ASelectableButtonManager button && button.isActive;
        private protected sealed override bool CanSelectUp => Up != null && Up is ASelectableButtonManager button && button.isActive;

        private void Update()
        {
            if (UIActivationManager.Instance.Front == LocatedUI)
            {
                if (InputManager.OutGame.MoveLeft)
                {
                    if (TrySelectLeft())
                    {
                        InputManager.OutGame.MakeMoveLeftInputDisabledUntilNextFrame();
                        TryInvokeOnSelectChanged(Left);
                    }
                }
                else if (InputManager.OutGame.MoveRight)
                {
                    if (TrySelectRight())
                    {
                        InputManager.OutGame.MakeMoveRightInputDisabledUntilNextFrame();
                        TryInvokeOnSelectChanged(Right);
                    }
                }
                else if (InputManager.OutGame.MoveDown)
                {
                    if (TrySelectDown())
                    {
                        InputManager.OutGame.MakeMoveDownInputDisabledUntilNextFrame();
                        TryInvokeOnSelectChanged(Down);
                    }
                }
                else if (InputManager.OutGame.MoveUp)
                {
                    if (TrySelectUp())
                    {
                        InputManager.OutGame.MakeMoveUpInputDisabledUntilNextFrame();
                        TryInvokeOnSelectChanged(Up);
                    }
                }
            }
        }

        private void TryInvokeOnSelectChanged(Button.ASelectableButtonManager currentlySelectedButton)
        {
            if (currentlySelectedButton is ASelectableButtonManager button)
                this.OnSelectChanged(button);
        }
    }
}
