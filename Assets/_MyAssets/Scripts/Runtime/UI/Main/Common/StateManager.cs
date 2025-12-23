namespace MyScripts.Runtime.UI.Main
{
    internal sealed class StateManager : ASingletonMonoBehaviour<StateManager>
    {
        [SerializeField] private Canvas defaultCanvas;
        [SerializeField] private Canvas pauseCanvas;
        [SerializeField] private Canvas backConfirmCanvas;

        [SerializeField] private AViewConstructor defaultCtor;
        [SerializeField] private AViewConstructor pauseCtor;
        [SerializeField] private AViewConstructor backConfirmCtor;

        internal State State { get; private set; } = State.Default;

        // Awake で初期化
        private Dictionary<State, Canvas> stateCanvasMap;
        private Dictionary<State, AViewConstructor> stateCtorMap;

        private void Awake()
        {
            stateCanvasMap = new()
            {
                { State.Default, defaultCanvas },
                { State.Pause, pauseCanvas },
                { State.Back_Confirm, backConfirmCanvas },
            };
            stateCtorMap = new()
            {
                { State.Default, defaultCtor },
                { State.Pause, pauseCtor },
                { State.Back_Confirm, backConfirmCtor },
            };

            ChangeState(State.Default, manually: true);
        }

        // manually を true にすると、諸々のチェックをスキップする
        internal void ChangeState(State newState, bool manually = false)
        {
            if (!manually && this.State == newState)
            {
                $"同じUI状態への遷移要求がありました. 何も行いません: {newState}".Print(LogSettings.Warning);
                return;
            }

            if (!manually && !IsAvailableTransition(this.State, newState))
            {
                $"有効なUI状態遷移ではありません. 何も行いません: {this.State} -> {newState}".Print(LogSettings.Error);
                return;
            }

            // 終了処理 → 非アクティブ → 状態変更 → アクティブ → 初期化処理
            stateCtorMap[this.State].Deconstruct();
            stateCanvasMap[this.State].gameObject.SetActive(false);
            this.State = newState;
            stateCanvasMap[this.State].gameObject.SetActive(true);
            stateCtorMap[this.State].Construct();
        }

        private static bool IsAvailableTransition(State from, State to) => (from, to) switch
        {
            (State.Default, State.Pause) => true,
            (State.Pause, State.Default or State.Back_Confirm) => true,
            (State.Back_Confirm, State.Pause) => true,
            _ => false,
        };
    }
}
