namespace MyScripts.Runtime
{
    internal sealed class SOSSignFindManager : MonoBehaviour
    {
        [SerializeField] private Transform root; // SOSサインの親オブジェクト (生成した後ここに格納する)
        [SerializeField] private Transform candidateRoot; // 配置候補箇所の親オブジェクト
        [SerializeField] private GameObject sosSignPrefab;
        [Space(10)]
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
        // 当たり判定の購読のみで使う
        // 外部から設定してもらう
        // TODO: 汚い実装！
        internal Func<bool> IsCharacterHuman { get; set; } = null;

        // 外部から自由に設定できる
        // SOSサインを配置する時、配置候補箇所の中からランダムにこの個数だけ選んで配置する
        internal static int PlaceAmount { get; set; } = -1;

        private async UniTaskVoid Awake()
        {
            sosSignLogText = InGameSOHolder.Instance.SOSSignLogText;

            disasterManagers = new()
            {
                { Disaster.Windstorm, windstormManager },
                { Disaster.Blizzard, blizzardManager },
            };

            RandomlyInstantiateAndPlace(out sosSigns, out Collider[] outColliders);
            // 外部からのデリゲート登録を確実に待つ
            await UniTask.NextFrame(destroyCancellationToken);
            Setup(outColliders, timeScoreManager.DecrementLeftAmount);

            ObserveDisasterOccurrenceAsync(destroyCancellationToken).Forget();
        }

        // プレハブから生成して、ランダムに配置する
        // インスタンスを格納する用の配列を GC.Alloc して、それを返す
        private void RandomlyInstantiateAndPlace(out ParticleSystem[] outParticleSystems, out Collider[] outColliders)
        {
            // 配置候補箇所をランダムにシャッフルする
            Transform[] candidates = new Transform[candidateRoot.childCount];
            for (int i = 0; i < candidateRoot.childCount; i++)
            {
                candidates[i] = candidateRoot.GetChild(i);
            }
            candidates.AsSpan().ShuffleSelf();

            int placeAmountReal = Mathf.Min(candidateRoot.childCount, PlaceAmount);

            ReadOnlySpan<Transform> candidatesSpan = candidates.AsSpan(0, placeAmountReal);
            outParticleSystems = new ParticleSystem[placeAmountReal];
            outColliders = new Collider[placeAmountReal];

            for (int i = 0; i < placeAmountReal; i++)
            {
                GameObject sosSignInstance = Instantiate(sosSignPrefab, candidatesSpan[i].position, candidatesSpan[i].rotation, root);

                outParticleSystems[i] = sosSignInstance.GetComponentInChildren<ParticleSystem>(includeInactive: true);
                outColliders[i] = sosSignInstance.GetComponentInChildren<Collider>(includeInactive: true);
            }
        }

        private void Setup(
            ReadOnlySpan<Collider> sosSignColliders,
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

        internal void UpdateVisibility(bool isCharacterHuman)
        {
            bool isVisible = !isCharacterHuman;

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
