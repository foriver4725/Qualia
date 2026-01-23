namespace MyScripts.Runtime.UI.Main
{
    internal sealed class PauseCtor : AViewConstructor
    {
        [SerializeField] private Button.ASelectableButtonManager defaultSelectedButton;
        [SerializeField] private Button.ASelectableButtonManager[] allButtons;

        internal sealed override void Construct()
        {
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
