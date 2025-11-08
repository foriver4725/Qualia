namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_GameParameter", menuName = "SO/Game Parameter")]
    internal sealed class SGameParameter : ScriptableObject
    {
        [SerializeField] private BarrierBorderSettings barrierBorder;
        internal BarrierBorderSettings BarrierBorder => barrierBorder;
        [Serializable]
        internal sealed class BarrierBorderSettings
        {
            [SerializeField, MinMaxRange(0.0f, 1000.0f), Tooltip("透明度が変化する距離区間(プレイヤーとの距離) [m]\n段々と見えるようになり、完全に表示する")] private Vector2 alphaChangingRange = new(5.0f, 50.0f);
            internal float AlphaChangingDistanceMin => alphaChangingRange.x;
            internal float AlphaChangingDistanceMax => alphaChangingRange.y;
        }

        [SerializeField] private CameraFOVSettings cameraFOV;
        internal CameraFOVSettings CameraFOV => cameraFOV;
        [Serializable]
        internal sealed class CameraFOVSettings
        {
            [SerializeField, Range(0.0f, 180.0f), Tooltip("デフォルト値 [度]")] private float @default = 60.0f;
            [SerializeField, Range(-10.0f, 10.0f), Tooltip("変化速度 (係数)")] private float changeSpeed = 5.0f;
            [SerializeField, Range(-60.0f, 60.0f), Tooltip("ダッシュ時の変化量 [度]")] private float onSprintDelta = 15.0f;
            internal float Default => @default;
            internal float ChangeSpeed => changeSpeed;
            internal float OnSprintDelta => onSprintDelta;
        }

        [SerializeField, Range(0, 16), Tooltip("足音の更新処理を行う間隔 [フレーム]")] private byte walkSoundUpdateIntervalFrames = 4;
        internal byte WalkSoundUpdateIntervalFrames => walkSoundUpdateIntervalFrames;

        [SerializeField] private WindstormDisasterSettings windstormDisaster;
        internal WindstormDisasterSettings WindstormDisaster => windstormDisaster;
        [Serializable]
        internal sealed class WindstormDisasterSettings
        {
            [Serializable]
            internal sealed class WindZoneParametersSettings : WindstormManager.IWindZoneParameters
            {
                [SerializeField, Range(0.0f, 20.0f), Tooltip("風の基本強度")] private float main = 10f;
                [SerializeField, Range(0.0f, 5.0f), Tooltip("タービュランス（ランダム揺れの強さ）")] private float turbulence = 3f;
                [SerializeField, Range(0.0f, 5.0f), Tooltip("風の強弱の振れ幅")] private float pulseMagnitude = 2.5f;
                [SerializeField, Range(0.0f, 5.0f), Tooltip("風の強弱の変化速度")] private float pulseFrequency = 1.2f;

                public float Main => main;
                public float Turbulence => turbulence;
                public float PulseMagnitude => pulseMagnitude;
                public float PulseFrequency => pulseFrequency;
            }

            [SerializeField] private WindZoneParametersSettings windZoneParameters;
            [SerializeField, Range(0.0f, 100.0f), Tooltip("プレイヤーが押される速度 [m/s]")] private float playerPushedSpeed = 10.0f;
            internal WindZoneParametersSettings WindZoneParameters => windZoneParameters;
            internal float PlayerPushedSpeed => playerPushedSpeed;
        }
    }
}
