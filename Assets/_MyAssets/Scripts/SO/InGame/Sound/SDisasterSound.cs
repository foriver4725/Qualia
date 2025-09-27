namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_DisasterSound", menuName = "SO/Sound/InGame/Disaster")]
    internal sealed class SDisasterSound : ASSoundWithType<SDisasterSound.Disaster>
    {
        [SerializeField] private AudioClip windstorm;
        [SerializeField] private AudioClip blizzard;
        [SerializeField, Range(0.0f, 0.5f), Tooltip("フェードアウトするまでの時間")] private float fadeOutDuration = 0.2f;

        internal float FadeOutDuration => fadeOutDuration;

        internal enum Disaster : byte
        {
            Windstorm,
            Blizzard,

            Count,
        }

        internal sealed override AudioClip GetClip(Disaster type) => type switch
        {
            Disaster.Windstorm => windstorm,
            Disaster.Blizzard => blizzard,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
