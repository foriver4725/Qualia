namespace MyScripts.Runtime
{
    internal sealed class SOSSoundPlayer : ASoundPlayerWithType<SSOSSound, SSOSSound.Situation>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected sealed override byte GetTypeAmount() => (byte)SSOSSound.Situation.Count;
    }
}
