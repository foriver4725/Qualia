namespace MyScripts.Runtime
{
    internal sealed class CharacterTriggerSoundPlayer : ASoundPlayerWithType<SCharacterTriggerSound, SCharacterTriggerSound.Timing>
    {
        [SerializeField, Range(0.0f, 5.0f), Tooltip("CloseToEnd の効果音を、開始何秒から鳴らすか")] private float timeOffsetOfCloseToEndSound = 1.0f;
        internal float CloseToEndSoundLength => Param.CloseToEndSoundLength - timeOffsetOfCloseToEndSound;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected sealed override byte TypeToByte(SCharacterTriggerSound.Timing type) => (byte)type;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected sealed override byte GetTypeAmount() => (byte)SCharacterTriggerSound.Timing.Count;

        internal sealed override void LetPlay(SCharacterTriggerSound.Timing type)
        {
            AudioClip clip = Param.GetClip(type);
            if (clip == null)
            {
                "No valid clip exists to play.".LogWarning();
                return;
            }

            AudioSources[TypeToByte(type)].LetPlay
            (
                clip,
                volume: Param.Volume,
                time: type == SCharacterTriggerSound.Timing.CloseToEnd ? timeOffsetOfCloseToEndSound : 0.0f
            );
        }
    }
}
