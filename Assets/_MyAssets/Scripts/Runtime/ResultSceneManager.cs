namespace MyScripts.Runtime
{
    internal sealed class ResultSceneManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button oneMoreButton;

        private void Awake() => Impl(destroyCancellationToken).Forget();

        private async UniTaskVoid Impl(Ct ct)
        {
            resultText.SetTextFormat("{0} 個\n<size=120>取り除いた！</size>", ScoreHolder.FoundAmount);
            await oneMoreButton.OnClickAsync(ct);
            LoadManager.Instance.BeginLoad(Scene.Main);
        }
    }
}
