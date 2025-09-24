namespace MyScripts.Runtime
{
    internal sealed class TimeScoreManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI leftText;

        // Awake で初期化
        private float maxElapsed; // 未使用
        private byte maxFind;
        private float shouldElapse;
        private byte shouldFind;

        // クリア判定で使用
        private float elapsedAmount = 0.0f;
        private byte foundAmount = 0;

        private void Awake()
        {
            // スコアをリセット
            ScoreHolder.FoundAmount = 0;

            // クリア条件の取得
            {
                var clearCondition = InGameSOHolder.Instance.GameRule.GetClearCondition();
                maxElapsed = clearCondition.MaxElapse;
                maxFind = clearCondition.MaxFind;
                shouldElapse = clearCondition.ShouldElapse;
                shouldFind = clearCondition.ShouldFind;
            }

            // UIの更新
            UpdateUI(elapsedAmount, foundAmount);
        }

        private void Start()
        {
            // タイマーの開始
            CountTimeAsync(destroyCancellationToken).Forget();
        }

        private async UniTaskVoid CountTimeAsync(Ct ct)
        {
            while (!ct.IsCancellationRequested)
            {
                elapsedAmount += Time.deltaTime;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (InputManager.DebugFastenTimeLimit.Bool)
                {
                    // デバッグ用 : 時間を早める
                    elapsedAmount += 30.0f;
                }
#endif

                if (elapsedAmount >= shouldElapse)
                {
                    elapsedAmount = shouldElapse;
                    break;
                }

                UpdateUI(elapsedAmount, foundAmount); // 毎フレーム更新されるので、ここ以外で実行する必要はなさそう
                await UniTask.NextFrame(cancellationToken: ct);
            }

            // タイムアップ
            OnClear();
        }

        private void UpdateUI(float elapsedAmount, byte foundAmount)
        {
            // 残りの数値を計算
            float remainingTime = Mathf.Max(0.0f, shouldElapse - elapsedAmount);
            byte remainingFind = (byte)Mathf.Max(0, shouldFind - foundAmount);

            int min = Mathf.FloorToInt(remainingTime / 60);
            int sec = Mathf.FloorToInt(remainingTime % 60);
            timeText.SetTextFormat("{0:D2}:{1:D2}", min, sec);

            leftText.SetTextFormat("残り{0}個 (全{1}個)", remainingFind, maxFind);
        }

        internal void DecrementLeftAmount()
        {
            if (++foundAmount >= shouldFind)
                OnClear();
        }

        private void OnClear()
        {
            // UIを更新しておく
            UpdateUI(elapsedAmount, foundAmount);

            // スコアを受け渡す
            ScoreHolder.FoundAmount = foundAmount;

            // シーン遷移
            LoadManager.Instance.BeginLoad(Scene.Result);
        }
    }
}
