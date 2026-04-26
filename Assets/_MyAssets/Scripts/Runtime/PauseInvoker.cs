using MyScripts.Runtime.UI.Main;

namespace MyScripts.Runtime
{
    /// <summary>
    /// ポーズのフラグを管理する<br/>
    /// プレイヤーの挙動制御・UIの表示切替も行う
    /// </summary>
    internal sealed class PauseInvoker : MonoBehaviour
    {
        internal bool IsPaused { get; private set; } = false;

        private void Update()
        {
            if (InputManager.InGame.Escape)
            {
                InputManager.InGame.MakeEscapeInputDisabledUntilNextFrame();

                if (IsPaused && StateManager.Instance.State == State.Pause)
                    _ = TryUnpause();
                else if (StateManager.Instance.State == State.Default)
                    _ = TryPause();
            }
        }

        internal bool TryPause()
        {
            if (IsPaused) return false;
            IsPaused = true;

            InputManager.PlayerControl.Enabled = false;
            InputManager.InGame.Enabled = false;

            StateManager.Instance.ChangeState(State.Pause);
            CursorAdjuster.SetCursorEnabled(true);

            return true;
        }

        internal bool TryUnpause()
        {
            if (!IsPaused) return false;
            IsPaused = false;

            InputManager.MakePlayerControlAndInGameInputsDisabledUntilNextFrame();
            InputManager.PlayerControl.Enabled = true;
            InputManager.InGame.Enabled = true;

            StateManager.Instance.ChangeState(State.Default);
            CursorAdjuster.SetCursorEnabled(false);

            return true;
        }
    }
}