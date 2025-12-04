namespace MyScripts.Runtime
{
    internal sealed class CursorAdjuster : ASingletonMonoBehaviour<CursorAdjuster>
    {
        [SerializeField, Tooltip("Awake()時、trueならアクティブに、falseなら非アクティブにする")] private bool enabledOnAwake = true;

        private void Awake() => SetCursorEnabled(enabledOnAwake);

        internal static void SetCursorEnabled(bool enabled)
        {
            if (enabled)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
