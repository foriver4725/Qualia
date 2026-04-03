namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_SOSAnimaArrangement", menuName = "SO/InGame/SOS & Anima Arrangement")]
    internal sealed class SSOSAnimaArrangement : ScriptableObject
    {
        internal enum Group : byte
        {
            SOS_Land,
            SOS_Sea,
            SOS_Sky,
            Anima_Land,
            Anima_Sea,
            Anima_Sky,

            Count,
        }

        // @formatter:off
        [Header("Prefabs")]
        [SerializeField] private GameObject sosLandPrefab;
        [SerializeField] private GameObject sosSeaPrefab;
        [SerializeField] private GameObject sosSkyPrefab;
        [SerializeField] private GameObject animaLandPrefab;
        [SerializeField] private GameObject animaSeaPrefab;
        [SerializeField] private GameObject animaSkyPrefab;

        [Header("Counts")]
        [SerializeField, Range(0, 1000)] private int sosLandCount = 100;
        [SerializeField, Range(0, 1000)] private int sosSeaCount = 100;
        [SerializeField, Range(0, 1000)] private int sosSkyCount = 100;
        [SerializeField, Range(0, 1000)] private int animaLandCount = 100;
        [SerializeField, Range(0, 1000)] private int animaSeaCount = 100;
        [SerializeField, Range(0, 1000)] private int animaSkyCount = 100;

        [Serializable]
        internal sealed class PositionCreateSettings
        {
            [SerializeField, Range(0, 1000), Tooltip("1つの配置につき、配置場所を最大何回探索するか")]
            private int maxAttempts = 100;
            [SerializeField] private Vector2 center = new Vector2(-500f, 350f);
            [SerializeField, Range(0.0f, 1000.0f)] private float maxRange = 600.0f;
            [SerializeField, Range(0.0f, 5.0f)] private float heightAboveGround = 0.1f;
        
            internal int MaxAttempts => maxAttempts;
            internal Vector2 Center => center;
            internal float MaxRange => maxRange;
            internal float HeightAboveGround => heightAboveGround;
        }

        [SerializeField] private PositionCreateSettings positionCreate;
        internal PositionCreateSettings PositionCreate => positionCreate;

        [Serializable]
        internal sealed class FixedPositionCreateSettings
        {
            [SerializeField, MinMaxRange(0.0f, 20.0f), Tooltip("プレイヤーから 〇[m] 離す")]
            private Vector2 distanceRangeFromPlayer = new(3.0f, 10.0f);
            [SerializeField, MinMaxRange(-30.0f, 30.0f), Tooltip("プレイヤーの正面方向から 〇[度] ずらす")]
            private Vector2 angleErrorRangeFromPlayerForward = new(-30.0f, 30.0f);

            internal float DistanceFromPlayerMin => distanceRangeFromPlayer.x;
            internal float DistanceFromPlayerMax => distanceRangeFromPlayer.y;
            internal float AngleErrorFromPlayerForwardMin => angleErrorRangeFromPlayerForward.x;
            internal float AngleErrorFromPlayerForwardMax => angleErrorRangeFromPlayerForward.y;
        }

        [SerializeField] private FixedPositionCreateSettings fixedPositionCreate;
        internal FixedPositionCreateSettings FixedPositionCreate => fixedPositionCreate;
        // @formatter:on

        internal GameObject GetPrefab(Group group) => group switch
        {
            Group.SOS_Land   => sosLandPrefab,
            Group.SOS_Sea    => sosSeaPrefab,
            Group.SOS_Sky    => sosSkyPrefab,
            Group.Anima_Land => animaLandPrefab,
            Group.Anima_Sea  => animaSeaPrefab,
            Group.Anima_Sky  => animaSkyPrefab,
            _                => throw new ArgumentOutOfRangeException(nameof(group), group, null)
        };

        internal int GetCount(Group group) => group switch
        {
            Group.SOS_Land   => sosLandCount,
            Group.SOS_Sea    => sosSeaCount,
            Group.SOS_Sky    => sosSkyCount,
            Group.Anima_Land => animaLandCount,
            Group.Anima_Sea  => animaSeaCount,
            Group.Anima_Sky  => animaSkyCount,
            _                => throw new ArgumentOutOfRangeException(nameof(group), group, null)
        };

        // プレハブからの計算が難しそうなので、調整数値を直書きしておく
        internal static readonly Dictionary<string, float> TreeNameHeightMap = new()
        {
            { "Conifer", 29.0f },
            { "Cypress", 10.8f },
            { "Pine_A", 29.0f },
            { "Pine_B", 28.0f },
            { "Pine_C", 20.3f },
            { "Pine_D", 12.7f },
        };
    }
}