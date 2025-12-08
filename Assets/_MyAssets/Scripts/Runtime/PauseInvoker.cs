using MyScripts.Runtime.UI;
using MyScripts.Runtime.UI.Main;

namespace MyScripts.Runtime
{
    /// <summary>
    /// ポーズのフラグを管理する<br/>
    /// プレイヤーの挙動制御・UIの表示切替も行う
    /// </summary>
    internal sealed class PauseInvoker : MonoBehaviour
    {
        [SerializeField] private AViewConstructor pauseViewConstructor;

        internal bool IsPaused { get; private set; } = false;

        private void Update()
        {
            if (InputManager.InGame.Escape)
            {
                InputManager.InGame.MakeEscapeInputDisabledUntilNextFrame();

                if (IsPaused)
                    _ = TryUnpause();
                else
                    _ = TryPause();
            }
        }

        internal bool TryPause()
        {
            if (IsPaused) return false;
            IsPaused = true;

            InputManager.PlayerControl.Enabled = false;
            InputManager.InGame.Enabled = false;

            UIActivationManager.Instance.SetActive(UIActivationManager.UI.Pause, true);
            pauseViewConstructor.Construct();
            CursorAdjuster.SetCursorEnabled(true);

            return true;
        }

        internal bool TryUnpause()
        {
            if (!IsPaused) return false;
            IsPaused = false;

            InputManager.PlayerControl.Enabled = true;
            InputManager.InGame.Enabled = true;

            UIActivationManager.Instance.SetActive(UIActivationManager.UI.Pause, false);
            pauseViewConstructor.Deconstruct();
            CursorAdjuster.SetCursorEnabled(false);

            return true;
        }
    }
}
