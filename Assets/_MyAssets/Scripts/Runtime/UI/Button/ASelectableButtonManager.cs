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

        // ボタンの選択に制限を課する
        // 適宜必要なものを使う
        private protected virtual bool CanBeSelected => true;
        private protected virtual bool CanSelectAny => true;
        private protected virtual bool CanSelectLeft => true;
        private protected virtual bool CanSelectRight => true;
        private protected virtual bool CanSelectDown => true;
        private protected virtual bool CanSelectUp => true;
        private protected virtual bool CanSelect(ASelectableButtonManager button) => true;

        // 必要なら使ってね
        private protected ASelectableButtonManager Left => left;
        private protected ASelectableButtonManager Right => right;
        private protected ASelectableButtonManager Down => down;
        private protected ASelectableButtonManager Up => up;

        private protected abstract void OnBecameSelected();
        private protected abstract void OnBecameDeselected();

        private protected bool TrySelectLeft()
        {
            if (!CanSelectAny) return false;
            if (!CanSelectLeft) return false;
            if (!IsSelected) return false;
            if (left == null) return false;
            if (!left.CanBeSelected) return false;
            if (!CanSelect(left)) return false;

            this.IsSelected = false;
            left.IsSelected = true;

            this.PlayHoverSe();

            this.OnBecameDeselected();
            left.OnBecameSelected();

            return true;
        }

        private protected bool TrySelectRight()
        {
            if (!CanSelectAny) return false;
            if (!CanSelectRight) return false;
            if (!IsSelected) return false;
            if (right == null) return false;
            if (!right.CanBeSelected) return false;
            if (!CanSelect(right)) return false;

            this.IsSelected = false;
            right.IsSelected = true;

            this.PlayHoverSe();

            this.OnBecameDeselected();
            right.OnBecameSelected();

            return true;
        }

        private protected bool TrySelectDown()
        {
            if (!CanSelectAny) return false;
            if (!CanSelectDown) return false;
            if (!IsSelected) return false;
            if (down == null) return false;
            if (!down.CanBeSelected) return false;
            if (!CanSelect(down)) return false;

            this.IsSelected = false;
            down.IsSelected = true;

            this.PlayHoverSe();

            this.OnBecameDeselected();
            down.OnBecameSelected();

            return true;
        }

        private protected bool TrySelectUp()
        {
            if (!CanSelectAny) return false;
            if (!CanSelectUp) return false;
            if (!IsSelected) return false;
            if (up == null) return false;
            if (!up.CanBeSelected) return false;
            if (!CanSelect(up)) return false;

            this.IsSelected = false;
            up.IsSelected = true;

            this.PlayHoverSe();

            this.OnBecameDeselected();
            up.OnBecameSelected();

            return true;
        }

        /// <summary>
        /// 諸々の条件をガン無視して、強制的に自身を選択する<br/>
        /// 一つもボタンが選択されていない時にのみ、実行するべき<br/>
        /// 例えば初期化処理など<br/>
        /// </summary>
        internal void SelectThisForciblyUnsafe(bool playSe = false)
        {
            this.IsSelected = true;

            if (playSe)
                this.PlayHoverSe();

            this.OnBecameSelected();
        }

        /// <summary>
        /// 諸々の条件をガン無視して、強制的に自身の選択を解除する<br/>
        /// 現在一つのみボタンが選択されている時にのみ、実行するべき<br/>
        /// 例えばUIが閉じられる直前など<br/>
        /// </summary>
        internal void DeselectThisForciblyUnsafe(bool playSe = false)
        {
            this.IsSelected = false;

            if (playSe)
                this.PlayHoverSe();

            this.OnBecameDeselected();
        }
    }
}
