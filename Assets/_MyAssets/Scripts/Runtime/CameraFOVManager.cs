namespace MyScripts.Runtime
{
    internal sealed class CameraFOVManager : MonoBehaviour
    {
        [Flags]
        internal enum Mode : byte
        {
            None = 0,
            OnSprint = 1,
        }

#pragma warning disable CS0618 // Type or member is obsolete
        // 非推奨のコンポーネント. 現状はまだこれを使う
        [SerializeField] private Unity.Cinemachine.CinemachineVirtualCamera playerCamera;
#pragma warning restore CS0618 // Type or member is obsolete

        // Awake で初期化
        private SGameParameter.CameraFOVSettings param;
        private Dictionary<Mode, float> fovDeltas;
        private Mode mode;
        private float targetDeltas; // デフォルト値 + この値 に向かって、毎フレーム Lerp する

        private void Awake()
        {
            param = InGameSOHolder.Instance.GameParameter.CameraFOV;

            fovDeltas = new()
            {
                { Mode.OnSprint, param.OnSprintDelta },
            };

            UpdateTargetDeltas(this.mode = Mode.None);
        }

        private void Update()
        {
            UpdateCamera();
        }

        /// <summary>
        /// モードを追加する<br/>
        /// 既に存在する場合は、何も起こらない<br/>
        /// </summary>
        internal void AddMode(Mode mode) => UpdateTargetDeltas(this.mode |= mode);

        /// <summary>
        /// モードを削除する<br/>
        /// 存在しない場合は、何も起こらない<br/>
        /// </summary>
        internal void RemoveMode(Mode mode) => UpdateTargetDeltas(this.mode &= ~mode);

        private void UpdateTargetDeltas(Mode currentMode)
        {
            float deltas = 0.0f;
            foreach ((Mode mode, float delta) in fovDeltas)
            {
                if ((currentMode & mode) != 0)
                    deltas += delta;
            }

            targetDeltas = deltas;
        }

        private void UpdateCamera()
        {
            var lens = playerCamera.m_Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, param.Default + targetDeltas, Time.deltaTime * param.ChangeSpeed);
            playerCamera.m_Lens = lens;
        }
    }
}
