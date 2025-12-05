namespace MyScripts.Runtime.UI.Title.SaveSlot
{
    internal sealed class StateRootObjectManager : MonoBehaviour
    {
        [SerializeField] private GameObject selectRoot;
        [SerializeField] private GameObject startOptionRoot;
        [SerializeField] private GameObject confirmRoot;
        [SerializeField] private GameObject hideAllRoot;

        private State state = State.None;

        private void Awake()
        {
            ChangeState(State.None, doNothingIfSame: false);
        }

        private void ChangeState(State newState, bool doNothingIfSame = true)
        {
            if (doNothingIfSame && this.state == newState)
                return;
            this.state = newState;

            this.selectRoot.SetActive(this.state == State.Select);
            this.startOptionRoot.SetActive(this.state == State.StartOption);
            this.confirmRoot.SetActive(this.state == State.Confirm);
            this.hideAllRoot.SetActive(this.state == State.HideAll);
        }
    }
}
