namespace MyScripts.Runtime
{
    internal sealed class TimeScoreManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI leftText;
        [SerializeField, Range(0.0f, 180.0f)] private float timeLimit;

        // Awake で初期化
        private int leftAmount;
        private float remainingTime;

        private void Awake()
        {
            // スコアをリセット
            ScoreHolder.FoundAmount = 0;

            // 変数の値を初期化
            leftAmount = ScoreHolder.ShouldFoundAmount;
            remainingTime = timeLimit;

            // UIの更新
            UpdateUI(leftAmount, remainingTime);

            // タイマーの開始
            CountTimeAsync(destroyCancellationToken).Forget();
        }

        private async UniTaskVoid CountTimeAsync(Ct ct)
        {
            // 適当に、５個見つけたものとする
            leftAmount -= 5;

            while (!ct.IsCancellationRequested)
            {
                remainingTime -= Time.deltaTime;

                if (remainingTime <= 0.0f)
                {
                    remainingTime = 0.0f;

                    UpdateUI(leftAmount, remainingTime);
                    break;
                }

                UpdateUI(leftAmount, remainingTime);
                await UniTask.NextFrame(cancellationToken: ct);
            }

            // タイムアップ時の処理
            ScoreHolder.FoundAmount = (byte)(ScoreHolder.ShouldFoundAmount - leftAmount); // スコアを受け渡す
            LoadManager.Instance.BeginLoad(Scene.Result);
        }

        private void UpdateUI(int leftAmount, float remainingTime)
        {
            int remainMin = Mathf.FloorToInt(remainingTime / 60);
            int remainSec = Mathf.FloorToInt(remainingTime % 60);
            timeText.text = $"{remainMin:D2}:{remainSec:D2}";

            leftText.text = $"残り{leftAmount}個";
        }
    }
}
