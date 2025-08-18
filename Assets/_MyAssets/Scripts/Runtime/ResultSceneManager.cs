namespace MyScripts.Runtime
{
    internal sealed class ResultSceneManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button oneMoreButton;

        private void Awake() => Impl(destroyCancellationToken).Forget();

        private async UniTaskVoid Impl(Ct ct)
        {
            resultText.SetTextFormat("{0}/{1}", ScoreHolder.FoundAmount, ScoreHolder.ShouldFoundAmount);
            await oneMoreButton.OnClickAsync(ct);
            LoadManager.Instance.BeginLoad(Scene.Main);
        }
    }
}
