namespace MyScripts.Runtime
{
    internal sealed class ResultSceneManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button oneMoreButton;

        private void Awake()
        {
            resultText.text = $"{ScoreHolder.FoundAmount}/{ScoreHolder.ShouldFoundAmount}";
            oneMoreButton.onClick.AddListener(static () => LoadManager.Instance.BeginLoad(Scene.Main));
        }
    }
}
