namespace MyScripts.Runtime
{
    internal sealed class TitleSceneManager : MonoBehaviour, IOutGameSceneManagerSingleTransition
    {
        [SerializeField] private Button startButton;

        private void Awake() => Impl(destroyCancellationToken).Forget();

        private async UniTaskVoid Impl(Ct ct)
        {
            await startButton.OnClickAsync(ct);
            TransitToNextScene();
        }

        public void TransitToNextScene() => LoadManager.Instance.BeginLoad(Scene.Select);
    }
}
