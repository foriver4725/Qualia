namespace MyScripts.Runtime
{
    internal sealed class ResultSceneManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI resultText;

        private void Start()
        {
            resultText.SetTextFormat("{0} 個\n<size=120>取り除いた！</size>", ScoreHolder.FoundAmount);
        }
    }
}
