namespace MyScripts.Runtime
{
    /// <summary>
    /// OnJustBeforeAwake() を使用<br/>
    /// previous, next が存在しないなら、設定せずに null のままでOK<br/>
    /// </summary>
    internal abstract class ASelectableButtonManager : AButtonManager
    {
        [SerializeField] private ASelectableButtonManager previous;
        [SerializeField] private ASelectableButtonManager next;
        [SerializeField] private bool isOnlySelectedAtFirst = false;

        private protected bool IsSelected { get; private set; } = false;

        private protected void SelectPrevious()
        {
            // 単なるUnityのnullチェックではない
            // 前のボタンが無い場合はnullが設定されるので、その確認という意味もある
            if (previous == null) return;

            this.IsSelected = false;
            previous.IsSelected = true;
        }

        private protected void SelectNext()
        {
            // 単なるUnityのnullチェックではない
            // 次のボタンが無い場合はnullが設定されるので、その確認という意味もある
            if (next == null) return;

            this.IsSelected = false;
            next.IsSelected = true;
        }

        private protected sealed override void OnJustBeforeAwake()
        {
            if (isOnlySelectedAtFirst)
                this.IsSelected = true;
        }
    }
}
