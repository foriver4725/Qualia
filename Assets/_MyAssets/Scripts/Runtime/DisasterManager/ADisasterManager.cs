namespace MyScripts.Runtime
{
    internal abstract class ADisasterManager : MonoBehaviour, IDisasterManager
    {
        [SerializeField] private TextMeshProUGUI text;
        private protected TextMeshProUGUI Text => text;

        public abstract Disaster MyType { get; }
        public bool Enabled { get; set; } = false;

        private static readonly Dictionary<Disaster, string> DisasterNames = new()
        {
            { Disaster.Windstorm, "強風" },
            { Disaster.Blizzard, "吹雪" },
        };

        private void Awake() => Impl(destroyCancellationToken).Forget();

        private protected virtual async UniTaskVoid Impl(Ct ct)
        {
            SetTextEnabled(false);

            while (true)
            {
                await UniTask.WaitUntil(() => Enabled, cancellationToken: ct);
                SetTextEnabled(true);
                await UniTask.WaitUntil(() => !Enabled, cancellationToken: ct);
                SetTextEnabled(false);
            }
        }

        private protected virtual void SetTextEnabled(bool enabled)
        {
            if (enabled && DisasterNames.TryGetValue(MyType, out var name))
            {
                Text.SetTextFormat("<color=#ffe03e>{0}</color> に注意", name);
            }
            else
            {
                Text.text = string.Empty;
            }
        }
    }
}
