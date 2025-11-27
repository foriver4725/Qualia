namespace MyScripts.Runtime
{
    /// <summary>
    /// ポーズのフラグを管理する<br/>
    /// プレイヤーの挙動制御・UIの表示切替も行う
    /// </summary>
    internal sealed class PauseInvoker : MonoBehaviour
    {
        [SerializeField] private PlayerController pc;
        [SerializeField] private Canvas pauseUi;

        private bool isPaused = false;

        private void Update()
        {
            if (InputManager.InGamePause.Bool)
            {
                _ = TryPause();
            }
        }

        internal bool TryPause()
        {
            if (isPaused) return false;
            isPaused = true;

            CursorAdjuster.SetCursorEnabled(true);
            pauseUi.gameObject.SetActive(true);

            return true;
        }

        internal bool TryUnpause()
        {
            if (!isPaused) return false;
            isPaused = false;

            CursorAdjuster.SetCursorEnabled(false);
            pauseUi.gameObject.SetActive(false);

            return true;
        }
    }
}
