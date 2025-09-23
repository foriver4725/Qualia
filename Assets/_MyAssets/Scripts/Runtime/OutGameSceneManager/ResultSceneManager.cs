namespace MyScripts.Runtime
{
    internal sealed class ResultSceneManager : MonoBehaviour, IOutGameSceneManagerMultiTransition
    {
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button backButton;

        private void Awake() => Impl(destroyCancellationToken).Forget();

        private async UniTaskVoid Impl(Ct ct)
        {
            resultText.SetTextFormat("{0} 個\n<size=120>取り除いた！</size>", ScoreHolder.FoundAmount);

            int completedTaskIndex = await UniTask.WhenAny(
                retryButton.OnClickAsync(ct),
                backButton.OnClickAsync(ct)
            );

            Scene nextScene = (completedTaskIndex == 0) ? Scene.Main : Scene.Title;
            TransitToScene(nextScene);
        }

        public void TransitToScene(Scene scene) => LoadManager.Instance.BeginLoad(scene);
    }
}
