using UnityEngine;

namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_WalkSound", menuName = "SO/Sound/InGame/Walk")]
    internal sealed class SWalkSound : ASSoundWithType<SWalkSound.Surface>
    {
        [SerializeField] private AudioClip grass;
        [SerializeField] private AudioClip sand;
        [SerializeField] private AudioClip rock;
        [SerializeField] private AudioClip water;
        [Space(10)]
        [SerializeField, Range(1, 16), Tooltip("同時に鳴る足音の最大数")] private byte maxSoundAmount = 8;
        [SerializeField, Range(0.0f, 0.5f), Tooltip("足音がフェードアウトするまでの時間")] private float fadeOutDuration = 0.2f;
        [Space(10)]
        [SerializeField, Range(0.5f, 2.0f)] private float walkPitch = 1.2f;
        [SerializeField, Range(0.5f, 2.0f)] private float sprintPitch = 1.5f;
        [SerializeField] private AK.Wwise.Event play_Walk;
        [SerializeField] private AK.Wwise.Event stop_Walk;
        [SerializeField] private AK.Wwise.Switch Grass;
        [SerializeField] private AK.Wwise.Switch Sand;
        [SerializeField] private AK.Wwise.Switch Rock;
        [SerializeField] private AK.Wwise.Switch Water;
        [SerializeField] private AK.Wwise.RTPC w_walkPitch;

        internal byte MaxSoundAmount => maxSoundAmount;
        internal float FadeOutDuration => fadeOutDuration;
        internal float WalkPitch => walkPitch;
        internal float SprintPitch => sprintPitch;
        internal AK.Wwise.Event Play_Walk => play_Walk;
        internal AK.Wwise.Event Stop_Walk => stop_Walk;
        internal AK.Wwise.RTPC W_WalkPitch => w_walkPitch;

        internal enum Surface : byte
        {
            None = 0,
            Grass = 1,
            Sand = 2,
            Rock = 3,
            Water = 4,
            Default = Grass,
        }

        internal sealed override AudioClip GetClip(Surface type) => type switch
        {
            Surface.None => null,
            Surface.Grass => grass,
            Surface.Sand => sand,
            Surface.Rock => rock,
            Surface.Water => water,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Switch GetSwitch(Surface type) => type switch
        {
            Surface.None => null,
            Surface.Grass => Grass,
            Surface.Sand => Sand,
            Surface.Rock => Rock,
            Surface.Water => Water,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Event GetEvent()
        {
            return play_Walk;
        }

    }
}
