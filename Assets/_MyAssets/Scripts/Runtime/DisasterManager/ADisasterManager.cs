namespace MyScripts.Runtime
{
    internal abstract class ADisasterManager : MonoBehaviour, IDisasterManager
    {
        public bool Enabled { get; set; } = false;

        private void Awake() => Impl(destroyCancellationToken).Forget();

        private protected abstract UniTaskVoid Impl(Ct ct);
    }
}
