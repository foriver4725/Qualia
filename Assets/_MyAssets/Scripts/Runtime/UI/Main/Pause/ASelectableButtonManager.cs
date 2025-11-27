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
        /// 選択されている状態で決定された<br/>
        /// 可能かどうか調べて、PlayClickSe(), OnClickSucceeded() を呼び出してほしい<br/>
        /// </summary>
        private protected abstract void OnSubmittedWithSelection();

        private void Update()
        {
            if (InputManager.OutGame.MoveUp)
            {
                SelectPrevious();
            }
            else if (InputManager.OutGame.MoveDown)
            {
                SelectNext();
            }

            if (IsSelected == true && InputManager.OutGame.Submit)
            {
                this.OnSubmittedWithSelection();
            }
        }
    }
}
