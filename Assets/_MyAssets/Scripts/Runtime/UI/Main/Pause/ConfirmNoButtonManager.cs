namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ConfirmNoButtonManager : AButtonManager
    {
        [SerializeField] private Canvas onPauseConfirmUi;

        private void Update()
        {
            if (onPauseConfirmUi.gameObject.activeSelf == true && InputManager.OutGame.Submit)
            {
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
            => onPauseConfirmUi.gameObject.SetActive(false);
    }
}
