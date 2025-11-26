namespace MyScripts.Runtime.OutGame.Title.SelectSaveSlotUI
{
    internal sealed class BackButtonManager : AButtonManager
    {
        [SerializeField] private Canvas ui;

        private protected sealed override void OnClickSucceeded()
            => ui.gameObject.SetActive(false);
    }
}
