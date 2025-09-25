namespace MyScripts.Runtime
{
    internal sealed class CameraFOVManager : MonoBehaviour
    {
        [Flags]
        internal enum Mode : byte
        {
            Default = 0,
            OnSprint = 1,
        }

        [SerializeField] private Camera playerCamera;

        // Awake で初期化
        private SGameParameter.CameraFOVSettings param;
        private Dictionary<Mode, float> fovDeltas;
        private Mode mode;

        private void Awake()
        {
            param = InGameSOHolder.Instance.GameParameter.CameraFOV;

            fovDeltas = new()
            {
                { Mode.OnSprint, param.OnSprintDelta },
            };

            UpdateFOV(this.mode = Mode.Default);
        }

        /// <summary>
        /// モードを追加する<br/>
        /// 既に存在する場合は、何も起こらない<br/>
        /// </summary>
        internal void AddMode(Mode mode) => UpdateFOV(this.mode |= mode);

        /// <summary>
        /// モードを削除する<br/>
        /// 存在しない場合は、何も起こらない<br/>
        /// </summary>
        internal void RemoveMode(Mode mode) => UpdateFOV(this.mode &= ~mode);

        private void UpdateFOV(Mode currentMode)
        {
            float deltas = 0.0f;
            foreach ((Mode mode, float delta) in fovDeltas)
            {
                if (currentMode.HasFlag(mode))
                    deltas += delta;
            }

            playerCamera.fieldOfView = param.Default + deltas;
        }
    }
}
