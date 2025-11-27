namespace MyScripts.Runtime.UI.Title
{
    internal sealed class StartButtonManager : AButtonManager
    {
        [SerializeField] private Canvas ui;

        private void Update()
        {
            if (ui.gameObject.activeSelf == false && InputManager.OutGame.Submit)
            {
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
            => ui.gameObject.SetActive(true);
    }
}
