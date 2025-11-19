namespace MyScripts.Runtime
{
    internal sealed class ButtonSoundPlayer : ASoundPlayerWithType<SButtonSound, SButtonSound.Action>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected sealed override byte GetTypeAmount() => (byte)SButtonSound.Action.Count;
    }
}
