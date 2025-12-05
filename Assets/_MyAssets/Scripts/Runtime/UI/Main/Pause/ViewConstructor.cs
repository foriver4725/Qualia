using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ViewConstructor : AViewConstructor
    {
        [SerializeField] private ASelectableButtonManager resumeButtonManager;
        [SerializeField] private ASelectableButtonManager backToTitleButtonManager;
        [SerializeField] private ASelectableButtonManager backToDesktopButtonManager;

        internal sealed override void Construct()
        {
            resumeButtonManager.SelectThisForciblyUnsafe();
        }

        internal sealed override void Deconstruct()
        {
            if (resumeButtonManager.IsSelected) resumeButtonManager.DeselectThisForciblyUnsafe();
            if (backToTitleButtonManager.IsSelected) backToTitleButtonManager.DeselectThisForciblyUnsafe();
            if (backToDesktopButtonManager.IsSelected) backToDesktopButtonManager.DeselectThisForciblyUnsafe();
        }
    }
}
