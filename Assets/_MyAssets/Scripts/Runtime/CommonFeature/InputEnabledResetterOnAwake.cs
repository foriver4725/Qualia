namespace MyScripts.Runtime
{
    /// <summary>
    /// Awake()時にInputManagerのEnabledフラグをリセットする
    /// </summary>
    internal sealed class InputEnabledResetterOnAwake : MonoBehaviour
    {
        private void Awake()
        {
            InputManager.PlayerControl.Enabled = true;
            InputManager.InGame.Enabled = true;
            InputManager.OutGame.Enabled = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            InputManager.Debug.Enabled = true;
#endif
        }
    }
}
