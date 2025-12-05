namespace MyScripts.Runtime.UI.Title.SaveSlot
{
    internal sealed class StateRootObjectManager : ASingletonMonoBehaviour<StateRootObjectManager>
    {
        [SerializeField] private GameObject selectRoot;
        [SerializeField] private GameObject startOptionRoot;
        [SerializeField] private GameObject confirmRoot;
        [SerializeField] private GameObject hideAllRoot;

        [SerializeField] private AViewConstructor selectViewConstructor;
        [SerializeField] private AViewConstructor startOptionViewConstructor;
        [SerializeField] private AViewConstructor confirmViewConstructor;

        internal State State { get; private set; } = State.None;

        private void Awake()
        {
            ChangeState(State.None, doNothingIfSame: false);
        }

        internal void ChangeState(State newState, bool doNothingIfSame = true)
        {
            if (doNothingIfSame && this.State == newState)
                return;

            if (this.State == State.Select) this.selectViewConstructor.Deconstruct();
            else if (this.State == State.StartOption) this.startOptionViewConstructor.Deconstruct();
            else if (this.State == State.Confirm) this.confirmViewConstructor.Deconstruct();

            this.State = newState;

            this.selectRoot.SetActive(this.State == State.Select);
            this.startOptionRoot.SetActive(this.State == State.StartOption);
            this.confirmRoot.SetActive(this.State == State.Confirm);
            this.hideAllRoot.SetActive(this.State == State.HideAll);

            if (this.State == State.Select) this.selectViewConstructor.Construct();
            else if (this.State == State.StartOption) this.startOptionViewConstructor.Construct();
            else if (this.State == State.Confirm) this.confirmViewConstructor.Construct();
        }
    }
}
