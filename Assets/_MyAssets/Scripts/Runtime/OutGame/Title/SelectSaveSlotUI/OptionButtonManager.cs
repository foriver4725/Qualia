namespace MyScripts.Runtime.OutGame.Title.SelectSaveSlotUI
{
    internal sealed class OptionButtonManager : AButtonManager
    {
        [SerializeField, Range(0, Manager.OptionIndexCount - 1)] private int index = 0;
        [SerializeField] private Manager manager;

        private protected sealed override void OnClickSucceeded()
            => manager.SetOptionIndex(index);
    }
}
