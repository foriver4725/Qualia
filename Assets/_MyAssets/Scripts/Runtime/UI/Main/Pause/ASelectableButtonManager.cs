namespace MyScripts.Runtime.UI.Main.Pause
{
    /// <summary>
    /// OnJustBeforeAwake(), Update() を使用<br/>
    /// Previous は上のボタン<br/>
    /// Next は下のボタン<br/>
    /// </summary>
    internal abstract class ASelectableButtonManager : Runtime.ASelectableButtonManager
    {
        /// <summary>
        /// このボタンがどのUIに所属しているか<br/>
        /// このUIが最前面の場合のみ入力を受け付ける<br/>
        /// </summary>
        private protected abstract UIActivationManager.UI LocatedUI { get; }

        /// <summary>
        /// 選択されている状態で決定された<br/>
        /// 可能かどうか調べて、PlayClickSe(), OnClickSucceeded() を呼び出してほしい<br/>
        /// </summary>
        private protected abstract void OnSubmittedWithSelection();

        private void Update()
        {
            if (UIActivationManager.Instance.Front == LocatedUI)
            {
                if (InputManager.OutGame.MoveUp)
                {
                    InputManager.OutGame.MakeMoveUpInputDisabledUntilNextFrame();
                    SelectPrevious();
                }
                else if (InputManager.OutGame.MoveDown)
                {
                    InputManager.OutGame.MakeMoveDownInputDisabledUntilNextFrame();
                    SelectNext();
                }

                if (IsSelected == true && InputManager.OutGame.Submit)
                {
                    InputManager.OutGame.MakeSubmitInputDisabledUntilNextFrame();
                    this.OnSubmittedWithSelection();
                }
            }
        }
    }
}
