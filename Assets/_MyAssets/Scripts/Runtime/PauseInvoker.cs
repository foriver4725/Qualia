namespace MyScripts.Runtime
{
    /// <summary>
    /// ポーズのフラグを管理する<br/>
    /// プレイヤーの挙動制御・UIの表示切替も行う
    /// </summary>
    internal sealed class PauseInvoker : MonoBehaviour
    {
        [SerializeField] private Canvas pauseUi;

        private bool isPaused = false;

        private void Update()
        {
            if (InputManager.InGame.Escape)
            {
                if (isPaused)
                    _ = TryUnpause();
                else
                    _ = TryPause();
            }
        }

        internal bool TryPause()
        {
            if (isPaused) return false;
            isPaused = true;

            InputManager.PlayerControl.Enabled = false;
            pauseUi.gameObject.SetActive(true);
            CursorAdjuster.SetCursorEnabled(true);

            return true;
        }

        internal bool TryUnpause()
        {
            if (!isPaused) return false;
            isPaused = false;

            InputManager.PlayerControl.Enabled = true;
            pauseUi.gameObject.SetActive(false);
            CursorAdjuster.SetCursorEnabled(false);

            return true;
        }
    }
}
