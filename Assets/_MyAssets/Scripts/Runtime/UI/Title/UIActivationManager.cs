namespace MyScripts.Runtime.UI.Title
{
    internal sealed class UIActivationManager : ASingletonMonoBehaviour<UIActivationManager>
    {
        internal enum UI : byte
        {
            None,
            SelectSaveSlot,
            OptionConfirm,
            GameQuitConfirm,
        }

        [SerializeField] private Canvas selectSaveSlotUi;
        [SerializeField] private Canvas optionConfirmUi;
        [SerializeField] private Canvas gameQuitConfirmUi;

        // 後ろほど前面にある
        private readonly List<UI> uiActiveStates = new(3);

        internal UI Front
        {
            get
            {
                if (uiActiveStates.Count == 0)
                    return UI.None;

                return uiActiveStates[^1];
            }
        }

        internal void SetActive(UI ui, bool value)
        {
            if (uiActiveStates.Contains(ui) == value)
                return;

            if (value)
                uiActiveStates.Add(ui);
            else
                uiActiveStates.Remove(ui);

            Canvas uiCanvas = ui switch
            {
                UI.SelectSaveSlot => selectSaveSlotUi,
                UI.OptionConfirm => optionConfirmUi,
                UI.GameQuitConfirm => gameQuitConfirmUi,
                _ => throw new ArgumentOutOfRangeException(nameof(ui), ui, null),
            };
            uiCanvas.gameObject.SetActive(value);
        }
    }
}
