namespace MyScripts.Runtime
{
    internal abstract class ADisasterManager : MonoBehaviour, IDisasterManager
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private DisasterSoundPlayer soundPlayer;

        // 派生クラスではなく、外部から見る想定
        public abstract Disaster MyType { get; }
        public bool Enabled { get; set; } = false;

        private static readonly Dictionary<Disaster, string> DisasterNames = new()
        {
            { Disaster.Windstorm, "強風" },
            { Disaster.Blizzard, "吹雪" },
        };

        private void SetTextEnabled(bool enabled)
        {
            if (enabled && DisasterNames.TryGetValue(MyType, out var name))
            {
                text.SetTextFormat("<color=#ffe03e>{0}</color> に注意", name);
            }
            else
            {
                text.text = string.Empty;
            }
        }

        private void Awake() => Impl(destroyCancellationToken).Forget();
        private async UniTaskVoid Impl(Ct ct)
        {
            OnInitialize();

            while (true)
            {
                await UniTask.WaitUntil(() => Enabled, cancellationToken: ct);
                OnBecameEnabled();
                SetTextEnabled(true);
                await UniTask.WaitUntil(() => !Enabled, cancellationToken: ct);
                OnBecameDisabled();
                SetTextEnabled(false);
            }
        }

        // ==================================================
        // 派生クラスでオーバーライドできるもの
        private protected virtual void OnInitialize() // Awake で一度だけ呼ばれる
        {
            SetTextEnabled(false);
        }
        private protected abstract void OnBecameEnabled();
        private protected abstract void OnBecameDisabled();

        // 派生クラスに公開する変数
        private protected DisasterSoundPlayer SoundPlayer => soundPlayer;
        // ==================================================
    }
}
