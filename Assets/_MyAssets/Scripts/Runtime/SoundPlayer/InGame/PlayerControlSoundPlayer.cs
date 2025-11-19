namespace MyScripts.Runtime
{
    internal sealed class PlayerControlSoundPlayer : ASoundPlayerWithType<SPlayerControlSound, SPlayerControlSound.Action>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected sealed override byte GetTypeAmount() => (byte)SPlayerControlSound.Action.Count;
    }
}
