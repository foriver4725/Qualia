namespace MyScripts.Runtime.UI.Title
{
    internal sealed class StartButtonManager : AButtonManager
    {
        [SerializeField] private Canvas ui;

        private protected sealed override void OnClickSucceeded()
            => ui.gameObject.SetActive(true);
    }
}
