namespace MyScripts.Runtime.UI.Button
{
    /// <summary>
    /// OnJustBeforeAwake(), Update() を使用<br/>
    /// Previous は上のボタン<br/>
    /// Next は下のボタン<br/>
    /// </summary>
    internal abstract class ASelectableButtonWithFrameManager : ASelectableButtonManager
    {
        /// <summary>
        /// このUIが最前面の場合のみ入力を受け付ける<br/>
        /// </summary>
        private protected abstract bool IsFrontUI { get; }

        /// <summary>
        /// 選択されている状態で決定された<br/>
        /// 可能かどうか調べて、PlayClickSe(), OnClickSucceeded() を呼び出してほしい<br/>
        /// </summary>
        private protected abstract void OnSubmittedWithSelection();

        private protected sealed override void OnBecameSelected()
        {
            SelectFrameManager.Instance.Reselect(this);
        }

        private protected sealed override void OnBecameDeselected()
        {
            SelectFrameManager.Instance.Deselect(this);
        }

        private void Update()
        {
            if (!IsFrontUI) return;

            if (InputManager.OutGame.MoveLeft)
            {
                if (TrySelectLeft())
                    InputManager.OutGame.MakeMoveLeftInputDisabledUntilNextFrame();
            }
            else if (InputManager.OutGame.MoveRight)
            {
                if (TrySelectRight())
                    InputManager.OutGame.MakeMoveRightInputDisabledUntilNextFrame();
            }
            else if (InputManager.OutGame.MoveDown)
            {
                if (TrySelectDown())
                    InputManager.OutGame.MakeMoveDownInputDisabledUntilNextFrame();
            }
            else if (InputManager.OutGame.MoveUp)
            {
                if (TrySelectUp())
                    InputManager.OutGame.MakeMoveUpInputDisabledUntilNextFrame();
            }
            else if (InputManager.OutGame.Submit)
            {
                if (IsSelected)
                {
                    InputManager.OutGame.MakeSubmitInputDisabledUntilNextFrame();
                    this.OnSubmittedWithSelection();
                }
            }
        }
    }
}
