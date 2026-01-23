namespace MyScripts.Runtime.UI.Main
{
    internal sealed class BackConfirmCtor : AViewConstructor
    {
        [SerializeField] private Button.ASelectableButtonManager defaultSelectedButton;
        [SerializeField] private Button.ASelectableButtonManager[] allButtons;

        [SerializeField] private TextMeshProUGUI confirmLabelText;

        internal sealed override void Construct()
        {
            confirmLabelText.text = BackOptions.ChosenBackType switch
            {
                BackType.BackToTitle => "ゲームを終了して\nタイトルに戻りますか？",
                BackType.BackToDesktop => "ゲームを終了して\nデスクトップに戻りますか？",
                _ => throw new ArgumentOutOfRangeException(nameof(BackOptions.ChosenBackType), BackOptions.ChosenBackType, null),
            };

            defaultSelectedButton.SelectThisForciblyUnsafe();
        }

        internal sealed override void Deconstruct()
        {
            foreach (var button in allButtons)
            {
                if (button.IsSelected)
                    button.DeselectThisForciblyUnsafe();
                button.OnExit(default);
            }
        }
    }
}
