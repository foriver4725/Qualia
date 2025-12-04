namespace MyScripts.Runtime.UI.Button
{
    /// <summary>
    /// OnJustBeforeAwake() を使用<br/>
    /// 切り替え先のボタンが無いなら、設定せずに null のままでOK<br/>
    /// </summary>
    internal abstract class ASelectableButtonManager : AButtonManager
    {
        [SerializeField] private ASelectableButtonManager left;
        [SerializeField] private ASelectableButtonManager right;
        [SerializeField] private ASelectableButtonManager down;
        [SerializeField] private ASelectableButtonManager up;
        [SerializeField] private bool isOnlySelectedAtFirst = false;

        // Try... で処理しているから、あまり見る必要はない
        private protected bool IsSelected { get; private set; } = false;

        private protected virtual bool CanSelectLeft => true;
        private protected virtual bool CanSelectRight => true;
        private protected virtual bool CanSelectDown => true;
        private protected virtual bool CanSelectUp => true;

        // 必要なら使ってね
        private protected ASelectableButtonManager Left => left;
        private protected ASelectableButtonManager Right => right;
        private protected ASelectableButtonManager Down => down;
        private protected ASelectableButtonManager Up => up;
        private protected bool IsOnlySelectedAtFirst => isOnlySelectedAtFirst;

        private protected bool TrySelectLeft()
        {
            if (!CanSelectLeft) return false;
            if (!IsSelected) return false;
            if (left == null) return false;

            this.IsSelected = false;
            left.IsSelected = true;
            return true;
        }

        private protected bool TrySelectRight()
        {
            if (!CanSelectRight) return false;
            if (!IsSelected) return false;
            if (right == null) return false;

            this.IsSelected = false;
            right.IsSelected = true;
            return true;
        }

        private protected bool TrySelectDown()
        {
            if (!CanSelectDown) return false;
            if (!IsSelected) return false;
            if (down == null) return false;

            this.IsSelected = false;
            down.IsSelected = true;
            return true;
        }

        private protected bool TrySelectUp()
        {
            if (!CanSelectUp) return false;
            if (!IsSelected) return false;
            if (up == null) return false;

            this.IsSelected = false;
            up.IsSelected = true;
            return true;
        }

        private protected override void OnJustBeforeAwake()
        {
            if (isOnlySelectedAtFirst)
                this.IsSelected = true;
        }
    }
}
