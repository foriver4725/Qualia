namespace MyScripts.Runtime
{
    internal sealed class CursorAdjusterOnAwake : MonoBehaviour
    {
        [SerializeField, Tooltip("trueならアクティブに、falseなら非アクティブにする")] private bool makeEnable;

        private void Awake()
        {
            if (makeEnable)
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
