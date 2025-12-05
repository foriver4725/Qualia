namespace MyScripts.Runtime.UI.Title.SaveSlot
{
    internal sealed class StateRootObjectManager : ASingletonMonoBehaviour<StateRootObjectManager>
    {
        [SerializeField] private GameObject selectRoot;
        [SerializeField] private GameObject startOptionRoot;
        [SerializeField] private GameObject confirmRoot;
        [SerializeField] private GameObject hideAllRoot;

        [SerializeField] private Select.ViewConstructor selectViewConstructor;
        [SerializeField] private StartOption.ViewConstructor startOptionViewConstructor;
        [SerializeField] private Confirm.ViewConstructor confirmViewConstructor;

        internal State State { get; private set; } = State.Select;

        private void Awake()
        {
            ChangeState(State.Select, doNothingIfSame: false);
        }

        internal void ChangeState(State newState, bool doNothingIfSame = true)
        {
            if (doNothingIfSame && this.State == newState)
                return;
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
