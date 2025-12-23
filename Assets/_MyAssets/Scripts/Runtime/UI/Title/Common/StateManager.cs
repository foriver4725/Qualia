namespace MyScripts.Runtime.UI.Title
{
    internal sealed class StateManager : ASingletonMonoBehaviour<StateManager>
    {
        [SerializeField] private Canvas defaultCanvas;
        [SerializeField] private Canvas quitConfirmCanvas;
        [SerializeField] private Canvas saveSlotSelectCanvas;
        [SerializeField] private Canvas saveSlotSelectPlayOptionCanvas;
        [SerializeField] private Canvas saveSlotFinalConfirmCanvas;
        [SerializeField] private Canvas hidingAllCanvas;

        [SerializeField] private AViewConstructor defaultCtor;
        [SerializeField] private AViewConstructor quitConfirmCtor;
        [SerializeField] private AViewConstructor saveSlotSelectCtor;
        [SerializeField] private AViewConstructor saveSlotSelectPlayOptionCtor;
        [SerializeField] private AViewConstructor saveSlotFinalConfirmCtor;
        [SerializeField] private AViewConstructor hidingAllCtor;

        internal State State { get; private set; } = State.Default;

        // Awake で初期化
        private Dictionary<State, Canvas> stateCanvasMap;
        private Dictionary<State, AViewConstructor> stateCtorMap;

        private void Awake()
        {
            stateCanvasMap = new()
            {
                { State.Default, defaultCanvas },
                { State.Quit_Confirm, quitConfirmCanvas },
                { State.SaveSlot_Select, saveSlotSelectCanvas },
                { State.SaveSlot_Select_PlayOption, saveSlotSelectPlayOptionCanvas },
                { State.SaveSlot_FinalConfirm, saveSlotFinalConfirmCanvas },
                { State.HidingAll, hidingAllCanvas },
            };
            stateCtorMap = new()
            {
                { State.Default, defaultCtor },
                { State.Quit_Confirm, quitConfirmCtor },
                { State.SaveSlot_Select, saveSlotSelectCtor },
                { State.SaveSlot_Select_PlayOption, saveSlotSelectPlayOptionCtor },
                { State.SaveSlot_FinalConfirm, saveSlotFinalConfirmCtor },
                { State.HidingAll, hidingAllCtor },
            };

            ChangeState(State.Default, doNothingIfSame: false);
        }

        internal void ChangeState(State newState, bool doNothingIfSame = true)
        {
            if (doNothingIfSame && this.State == newState)
            {
                $"同じUI状態への遷移要求がありました. 何も行いません: {newState}".Print(LogSettings.Warning);
                return;
            }

            if (!IsAvailableTransition(this.State, newState))
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
            (State.Default, State.Quit_Confirm or State.SaveSlot_Select) => true,
            (State.Quit_Confirm, State.Default) => true,
            (State.SaveSlot_Select, State.Default or State.SaveSlot_Select_PlayOption) => true,
            (State.SaveSlot_Select_PlayOption, State.SaveSlot_Select or State.SaveSlot_FinalConfirm) => true,
            (State.SaveSlot_FinalConfirm, State.SaveSlot_Select_PlayOption) => true,
            _ => false,
        };
    }
}
