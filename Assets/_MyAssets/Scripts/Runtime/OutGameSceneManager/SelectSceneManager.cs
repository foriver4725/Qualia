namespace MyScripts.Runtime
{
    internal sealed class SelectSceneManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI easyDescText;
        [SerializeField] private TextMeshProUGUI normalDescText;
        [SerializeField] private TextMeshProUGUI hardDescText;

        private void Awake()
        {
            SetSOSSignAmountToText();
        }

        private void SetSOSSignAmountToText()
        {
            Dictionary<Difficulty, TextMeshProUGUI> descTexts = new()
            {
                { Difficulty.Easy, easyDescText },
                { Difficulty.Normal, normalDescText },
                { Difficulty.Hard, hardDescText },
            };

            foreach (var (difficulty, text) in descTexts)
            {
                int sosSignAmount = GlobalSOHolder.Instance.GameRule.SOSSignMaxAmounts.Get(difficulty);
                text.SetTextFormat("{0} 個さがす", sosSignAmount);
            }
        }
    }
}
