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

        // Try... で処理しているから、あまり見る必要はない
        internal bool IsSelected { get; private set; } = false;

        private protected virtual bool CanSelectLeft => true;
        private protected virtual bool CanSelectRight => true;
        private protected virtual bool CanSelectDown => true;
        private protected virtual bool CanSelectUp => true;

        // 必要なら使ってね
        private protected ASelectableButtonManager Left => left;
        private protected ASelectableButtonManager Right => right;
        private protected ASelectableButtonManager Down => down;
        private protected ASelectableButtonManager Up => up;

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

        /// <summary>
        /// 諸々の条件をガン無視して、強制的に自身を選択する<br/>
        /// 一つもボタンが選択されていない時にのみ、実行するべき<br/>
        /// 例えば初期化処理など<br/>
        /// </summary>
        internal void SelectThisForciblyUnsafe()
        {
            this.IsSelected = true;
        }

        /// <summary>
        /// 諸々の条件をガン無視して、強制的に自身の選択を解除する<br/>
        /// 現在一つのみボタンが選択されている時にのみ、実行するべき<br/>
        /// 例えばUIが閉じられる直前など<br/>
        /// </summary>
        internal void DeselectThisForciblyUnsafe()
        {
            this.IsSelected = false;
        }
    }
}
