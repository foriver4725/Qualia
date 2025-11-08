namespace MyScripts.Runtime
{
    internal sealed class ButtonSoundPlayer : ASoundPlayerWithType<SButtonSound, SButtonSound.Action>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected sealed override byte TypeToByte(SButtonSound.Action type) => (byte)type;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected sealed override byte GetTypeAmount() => (byte)SButtonSound.Action.Count;
    }
}
