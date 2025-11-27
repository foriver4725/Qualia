namespace MyScripts.Runtime.OutGame.Title.SelectSaveSlotUI
{
    internal sealed class SubmitConfirmNoButtonManager : AButtonManager
    {
        [SerializeField] private Canvas optionConfirmCanvas;

        private protected sealed override void OnClickSucceeded()
            => optionConfirmCanvas.gameObject.SetActive(false);
    }
}
