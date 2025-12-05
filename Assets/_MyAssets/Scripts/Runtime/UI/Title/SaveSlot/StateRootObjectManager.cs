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

        private State state = State.None;

        private void Awake()
        {
            ChangeState(State.None, doNothingIfSame: false);
        }

        internal void ChangeState(State newState, bool doNothingIfSame = true)
        {
            if (doNothingIfSame && this.state == newState)
                return;
            this.state = newState;

            this.selectRoot.SetActive(this.state == State.Select);
            this.startOptionRoot.SetActive(this.state == State.StartOption);
            this.confirmRoot.SetActive(this.state == State.Confirm);
            this.hideAllRoot.SetActive(this.state == State.HideAll);

            if (this.state == State.Select) this.selectViewConstructor.Construct();
            else if (this.state == State.StartOption) this.startOptionViewConstructor.Construct();
            else if (this.state == State.Confirm) this.confirmViewConstructor.Construct();
        }
    }
}
