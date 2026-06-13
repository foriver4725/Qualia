namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_DisasterSound", menuName = "SO/Sound/InGame/Disaster")]
    internal sealed class SDisasterSound : ASSoundWithType<SDisasterSound.Disaster>
    {
        [SerializeField] private AudioClip windstorm;
        [SerializeField] private AudioClip blizzard;
        [SerializeField, Range(0.0f, 0.5f), Tooltip("フェードアウトするまでの時間")] private float fadeOutDuration = 0.2f;
        [SerializeField] private AK.Wwise.Event Play_Disaster;
        [SerializeField] private AK.Wwise.Switch Windstorm;
        [SerializeField] private AK.Wwise.Switch Blizzard;

        internal float FadeOutDuration => fadeOutDuration;

        internal enum Disaster : byte
        {
            Windstorm,
            Blizzard,
        }

        internal sealed override AudioClip GetClip(Disaster type) => type switch
        {
            Disaster.Windstorm => windstorm,
            Disaster.Blizzard => blizzard,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Switch GetSwitch(Disaster type) => type switch
        {
            Disaster.Windstorm => Windstorm,
            Disaster.Blizzard => Blizzard,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Event GetEvent()
        {
            return Play_Disaster;
        }
    }
}
