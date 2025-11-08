namespace MyScripts.Runtime
{
    internal sealed class CharacterTriggerSoundPlayer : ASoundPlayerWithType<SCharacterTriggerSound, SCharacterTriggerSound.Timing>
    {
        /// <summary>
        /// CloseToEnd の効果音を実際に鳴らす秒数
        /// </summary>
        internal float CloseToEndPlayLength => Param.CloseToEndLength - Param.CloseToEndTimeOffset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected sealed override byte TypeToByte(SCharacterTriggerSound.Timing type) => (byte)type;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected sealed override byte GetTypeAmount() => (byte)SCharacterTriggerSound.Timing.Count;

        internal sealed override void LetPlay(SCharacterTriggerSound.Timing type)
        {
            AudioClip clip = Param.GetClip(type);
            if (clip == null)
            {
                "No valid clip exists to play.".Print(LogSettings.Warning);
                return;
            }

            AudioSources[TypeToByte(type)].LetPlay
            (
                clip,
                volume: Param.Volume,
                time: type == SCharacterTriggerSound.Timing.CloseToEnd ? Param.CloseToEndTimeOffset : 0.0f
            );
        }
    }
}
