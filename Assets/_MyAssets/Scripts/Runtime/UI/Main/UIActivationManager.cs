namespace MyScripts.Runtime.UI.Main
{
    internal sealed class UIActivationManager : ASingletonMonoBehaviour<UIActivationManager>
    {
        internal enum UI : byte
        {
            None,
            Pause,
            OnPauseConfirm,
        }

        [SerializeField] private Canvas pauseUi;
        [SerializeField] private Canvas onPauseConfirmUi;

        // 後ろほど前面にある
        private readonly List<UI> uiActiveStates = new(2);

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
                UI.Pause => pauseUi,
                UI.OnPauseConfirm => onPauseConfirmUi,
                _ => throw new ArgumentOutOfRangeException(nameof(ui), ui, null),
            };
            uiCanvas.gameObject.SetActive(value);
        }
    }
}
