namespace MyScripts.Runtime.UI.Title.GameQuit
{
    internal sealed class GameQuitConfirmYesButtonManager : AButtonManager
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
