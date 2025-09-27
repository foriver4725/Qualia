namespace MyScripts.Runtime
{
    internal sealed class BlizzardManager : ADisasterManager
    {
        public sealed override Disaster MyType => Disaster.Blizzard;

        private protected sealed override void OnBecameEnabled() { }
        private protected sealed override void OnBecameDisabled() { }
    }
}
