namespace MyScripts.Runtime.UI.Title
{
    internal sealed class DefaultCtor : AViewConstructor
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
