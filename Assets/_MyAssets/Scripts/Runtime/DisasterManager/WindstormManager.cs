namespace MyScripts.Runtime
{
    internal sealed class WindstormManager : ADisasterManager
    {
        public sealed override Disaster MyType => Disaster.Windstorm;

        internal interface IWindZoneParameters
        {
            /// <summary>
            /// 風の基本強度
            /// </summary>
            float Main { get; }

            /// <summary>
            /// タービュランス (ランダム揺れの強さ)
            /// </summary>
            float Turbulence { get; }

            /// <summary>
            /// 風の強弱の振れ幅
            /// </summary>
            float PulseMagnitude { get; }

            /// <summary>
            /// 風の強弱の変化速度
            /// </summary>
            float PulseFrequency { get; }
        }
        private struct WindZoneParameters : IWindZoneParameters
        {
            public float Main { get; set; }
            public float Turbulence { get; set; }
            public float PulseMagnitude { get; set; }
            public float PulseFrequency { get; set; }

            internal WindZoneParameters(IWindZoneParameters param)
            {
                Main = param.Main;
                Turbulence = param.Turbulence;
                PulseMagnitude = param.PulseMagnitude;
                PulseFrequency = param.PulseFrequency;
            }
        }
        private static void GetWindZoneParameters(WindZone windZone, out WindZoneParameters param) => param = new()
        {
            Main = windZone.windMain,
            Turbulence = windZone.windTurbulence,
            PulseMagnitude = windZone.windPulseMagnitude,
            PulseFrequency = windZone.windPulseFrequency,
        };
        private static void SetWindZoneParameters(WindZone windZone, WindZoneParameters param)
        {
            windZone.windMain = param.Main;
            windZone.windTurbulence = param.Turbulence;
            windZone.windPulseMagnitude = param.PulseMagnitude;
            windZone.windPulseFrequency = param.PulseFrequency;
        }

        [SerializeField] private WindZone windZone;
        [SerializeField] private PlayerController playerController; // プレイヤーを押すときに使う

        // Awake で初期化
        private SGameParameter.WindstormDisasterSettings param;
        private WindZoneParameters windZoneParametersInit; // 初期値を保存
        private Quaternion windDirectionInit; // 初期値を保存

        private Vector3 addedPlayerSpeedDelta = Vector3.zero; // プレイヤーに加算した速度ベクトル

        private protected sealed override void OnInitialize()
        {
            base.OnInitialize();

            param = InGameSOHolder.Instance.GameParameter.WindstormDisaster;

            GetWindZoneParameters(windZone, out windZoneParametersInit);
            windDirectionInit = windZone.transform.rotation;
        }

        private protected sealed override void OnBecameEnabled()
        {
            SetWindZoneParameters(windZone, new(param.WindZoneParameters));

            Quaternion windDirection = Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f);
            addedPlayerSpeedDelta = windDirection * new Vector3(0.0f, 0.0f, param.PlayerPushedSpeed);

            windZone.transform.rotation = windDirection;
            playerController.VelocityDelta += addedPlayerSpeedDelta;
        }

        private protected sealed override void OnBecameDisabled()
        {
            SetWindZoneParameters(windZone, windZoneParametersInit);

            windZone.transform.rotation = windDirectionInit;
            playerController.VelocityDelta -= addedPlayerSpeedDelta;

            addedPlayerSpeedDelta = Vector3.zero;
        }
    }
}
