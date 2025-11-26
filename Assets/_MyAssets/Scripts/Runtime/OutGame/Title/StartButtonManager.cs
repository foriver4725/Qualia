namespace MyScripts.Runtime.OutGame.Title
{
    internal sealed class StartButtonManager : AButtonManager
    {
        [SerializeField] private Canvas ui;

        private protected sealed override void OnClickSucceeded()
            => ui.gameObject.SetActive(true);
    }
}
