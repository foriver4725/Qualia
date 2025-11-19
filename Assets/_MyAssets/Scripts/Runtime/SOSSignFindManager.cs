namespace MyScripts.Runtime
{
    internal sealed class SOSSignFindManager : MonoBehaviour
    {
        [SerializeField] private Transform root; // SOSサインの親オブジェクト (配下にはSOSサインしか置かない前提)
        [SerializeField] private Collider playerCapsuleCollider;
        [SerializeField] private TimeScoreManager timeScoreManager;
        [SerializeField] private SOSSoundPlayer soundPlayer;
        [Space(10)]
        [SerializeField] private WindstormManager windstormManager;
        [SerializeField] private BlizzardManager blizzardManager;

        // 災害の発生/終了は、この個別にカウントされる変数に基づいて行う
        private byte foundCountForDisaster = 0;

        // Awake で初期化
        private ParticleSystem[] sosSigns;
        private SSOSSignLogText sosSignLogText;
        private Dictionary<Disaster, ADisasterManager> disasterManagers;
        internal Func<bool> IsCharacterHuman { get; set; } = null; // 外部から設定してもらう

        private void Awake()
        {
            sosSigns = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            sosSignLogText = InGameSOHolder.Instance.SOSSignLogText;

            disasterManagers = new()
            {
                { Disaster.Windstorm, windstormManager },
                { Disaster.Blizzard, blizzardManager },
            };

            Setup(
                Array.AsReadOnly(root.GetComponentsInChildren<Collider>(includeInactive: true)),
                timeScoreManager.DecrementLeftAmount
            );

            ObserveDisasterOccurrenceAsync(destroyCancellationToken).Forget();
        }

        private void Setup(
            ReadOnlyCollection<Collider> sosSignColliders,
            // スコア更新など、見つけたとき共通の処理
            //! 災害の発生/終了は、このクラス内で行うので大丈夫
            Action onFind
        )
        {
            foreach (Collider sosSignCollider in sosSignColliders)
            {
                Collider col = sosSignCollider;

                col.OnTriggerEnterAsObservable()
                    .Where(c => ReferenceEquals(c, playerCapsuleCollider))
                    .SubscribeAwait((col, IsCharacterHuman, onFind), async (c, param, ct) =>
                {
                    if (param.IsCharacterHuman())
                    {
                        LogManager.Instance.ShowManually("左クリックで取り除く");

                        if (await WaitForClickOrExitAsync(param.col, ct) == true)
                        {
                            // 取り除く
                            param.col.gameObject.SetActive(false);
                            param.onFind?.Invoke();
                            foundCountForDisaster++;

                            LogManager.Instance.ShowManually(string.Empty);
                            LogManager.Instance.ShowAutomatically(
                                sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnHumanClick)
                            );

                            soundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);
                        }
                        else
                        {
                            LogManager.Instance.ShowManually(string.Empty);
                        }
                    }
                    else
                    {
                        {
                            using var sb = ZString.CreateStringBuilder();
                            sb.AppendFormat("{0}\n(人間でないと取り除けない)", sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnAnimalApproach));
                            LogManager.Instance.ShowManually(sb);
                        }

                        while (true)
                        {
                            if (await WaitForClickOrExitAsync(param.col, ct) == true)
                            {
                                soundPlayer.LetPlay(SSOSSound.Situation.CouldNotRemove);
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                        LogManager.Instance.ShowManually(string.Empty);
                    }
                })
                    .AddTo(col);
            }
        }

        internal void UpdateSOSSignsVisibility()
        {
            bool isVisible = !IsCharacterHuman();

            foreach (var sign in sosSigns)
            {
                if (sign != null)
                    sign.gameObject.SetActive(isVisible);
            }
        }

        // Click したなら true を、 Exit したなら false を返す
        // 同フレームなら Exit を優先する
        private async UniTask<bool> WaitForClickOrExitAsync(Collider collider, Ct ct) => await UniTask.WhenAny(
            // 同フレームなら Exit を優先するために、このタイミングで待つ
            UniTask.WaitUntil(() => InputManager.InGameSubmit.Bool, timing: PlayerLoopTiming.LastUpdate, cancellationToken: ct),
            collider.OnTriggerExitAsObservable()
                .Where(c => ReferenceEquals(c, playerCapsuleCollider))
                .FirstAsync(cancellationToken: ct)
                .AsUniTask()
        ) == 0;

        private async UniTaskVoid ObserveDisasterOccurrenceAsync(Ct ct)
        {
            var conditions = InGameSOHolder.Instance.GameRule.GetDisasterOccurrenceConditions();

            // インターフェースなので、foreach だとボックス化する
            for (int i = 0; i < conditions.Count; i++)
            {
                var condition = conditions[i];

                await WaitForDisasterCountAsync(condition.BeginCount, ct);

                SetDisasterEnabled(condition.Disaster, true);

                _ = await UniTask.WhenAny(
                    WaitForDisasterCountAsync(condition.EndCount, ct),
                    condition.EndDuration.SecAwait(ct: ct)
                );

                SetDisasterEnabled(condition.Disaster, false);
            }
        }

        // 災害用の発見数カウントが targetCount に達するまで待つ
        // カウントが減ることはないので、単純に >= で判定する
        private async UniTask WaitForDisasterCountAsync(byte targetCount, Ct ct)
            => await UniTask.WaitUntil(() => foundCountForDisaster >= targetCount, cancellationToken: ct);

        private void SetDisasterEnabled(Disaster disaster, bool enabled)
        {
            if (disasterManagers.TryGetValue(disaster, out var manager))
            {
                manager.Enabled = enabled;
            }
            else
            {
                "指定された災害のマネージャーが見つかりません".Print(LogSettings.Error);
            }
        }
    }
}
