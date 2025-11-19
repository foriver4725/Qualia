using MyScripts.Common.Button;

namespace MyScripts.Runtime
{
    internal sealed class GameQuitConfirmYesButtonManager : ATextButtonManager
    {
        private protected sealed override void OnClickSucceeded()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
