namespace MyScripts.Runtime
{
    internal sealed class TitleSceneManager : MonoBehaviour
    {
        [SerializeField] private Button startButton;

        private void Awake() => Impl(destroyCancellationToken).Forget();

        private async UniTaskVoid Impl(Ct ct)
        {
            await startButton.OnClickAsync(ct);
            LoadManager.Instance.BeginLoad(Scene.Main);
        }
    }
}
