using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime
{
    internal sealed class SOSSignFindManager : MonoBehaviour, IDataHoldingObject
    {
        [SerializeField] private Transform root; // SOSサインの親オブジェクト (生成した後ここに格納する)
        [SerializeField] private Transform pointRoot; // 配置箇所の親オブジェクト
        [SerializeField] private SOSSign prefab;
        [Space(10)]
        [SerializeField] private Collider playerCapsuleCollider;
        [SerializeField] private TextMeshProUGUI sosSignLeftRatioText;
        [Space(10)]
        [SerializeField] private AnimalLeaveInvoker animalLeaveInvoker;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        // Awake で初期化
        private SSOSSignLogText sosSignLogText;
        private int totalSOSSignCount;
        private int leftSOSSignCount = -1;
        private Collider[] sosSigns; // コライダーはルートに付いている

        #region Interface Implementation

        public void GetDataAndUpdateMyProperties()
        {
            Span<bool> foundSOSSigns = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasFoundSOSSigns.AsSpan();

            int activeCount = 0;
            for (int i = 0; i < totalSOSSignCount; i++)
            {
                if (!foundSOSSigns[i])
                {
                    sosSigns[i].gameObject.SetActive(true);
                    activeCount++;
                }
                else
                {
                    sosSigns[i].gameObject.SetActive(false);
                }
            }

            leftSOSSignCount = activeCount;
        }

        public void SetMyPropertiesToData()
        {
            Span<bool> foundSOSSigns = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasFoundSOSSigns.AsSpan();

            int activeCount = 0;
            for (int i = 0; i < totalSOSSignCount; i++)
            {
                if (sosSigns[i].gameObject.activeSelf)
                {
                    foundSOSSigns[i] |= false; // 既に見つけたことがある場合は上書きしない
                    activeCount++;
                }
                else
                {
                    foundSOSSigns[i] |= true; // 既に見つけたことがある場合は上書きしない
                }
            }

            Assert.IsTrue(activeCount == leftSOSSignCount);
        }

        #endregion

        private void Awake()
        {
            sosSignLogText = InGameSOHolder.Instance.SOSSignLogText;
            // totalSOSSignCount = InGameSOHolder.Instance.GameRule.SOSSignCount;
            totalSOSSignCount = Mathf.Min(InGameSOHolder.Instance.GameRule.SOSSignCount, pointRoot.childCount); // 暫定処理
#if UNITY_EDITOR
            // Assert.IsTrue(pointRoot.childCount == totalSOSSignCount);
            """
            配置箇所の数が規定値と一致しているか、ここでAssertするべきです。
            しかし、現在は機能が実装途中のため、このAssertは一旦無効化しています。
            """
            .Print(LogSettings.Warning);
#else
#error "ここの処理が未完成です。このままリリースするべきではありません。"
#endif

            sosSigns = new Collider[totalSOSSignCount];
            RandomlyInstantiateAndPlace(totalSOSSignCount, sosSigns);

            GetDataAndUpdateMyProperties();

            UpdateSOSSignLeftRatioText(sosSignLeftRatioText, leftSOSSignCount, totalSOSSignCount);
            Setup(sosSigns);
        }

        // プレハブから生成して、ランダムに配置する
        private void RandomlyInstantiateAndPlace(int count, Span<Collider> outColliders)
        {
            // 配置箇所に一括でSOSサインを配置
            SOSSignPoint[] candidatePoints = new SOSSignPoint[count];
            for (int i = 0; i < count; i++)
            {
                candidatePoints[i] = pointRoot.GetChild(i).GetComponent<SOSSignPoint>();
            }

            for (int i = 0; i < count; i++)
            {
                SOSSign instance = Instantiate(prefab, candidatePoints[i].transform.position, candidatePoints[i].transform.rotation, root);
                outColliders[i] = instance.Collider;
            }
        }

        private void Setup(ReadOnlySpan<Collider> sosSignColliders)
        {
            foreach (Collider sosSignCollider in sosSignColliders)
            {
                Collider collider = sosSignCollider;

                // TODO: ラムダ式のGC.Allocを無くしたい
                collider.OnTriggerEnterAsObservable()
                    .Where(otherCollider => ReferenceEquals(otherCollider, playerCapsuleCollider))
                    .SubscribeAwait(collider, async (otherCollider, selfCollider, ct) =>
                    {
                        if (animalLeaveInvoker.IsPossessing)
                        {
                            LogManager.Instance.ShowManually("左クリックで取り除く");

                            if (await WaitForClickOrExitAsync(selfCollider, playerCapsuleCollider, ct) == true)
                            {
                                // 取り除く
                                // "取り除く" = "ルートのゲームオブジェクトが非アクティブ"
                                selfCollider.gameObject.SetActive(false);
                                {
                                    leftSOSSignCount--;
                                    UpdateSOSSignLeftRatioText(sosSignLeftRatioText, leftSOSSignCount, totalSOSSignCount);

                                    // このタイミングで、セーブデータに反映しておく
                                    SetMyPropertiesToData();
                                }

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
                                sb.AppendFormat("{0}\n(動物でないと取り除けない)", sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnAnimalApproach));
                                LogManager.Instance.ShowManually(sb);
                            }

                            while (true)
                            {
                                if (await WaitForClickOrExitAsync(selfCollider, playerCapsuleCollider, ct) == true)
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
                    .AddTo(collider);
            }
        }

        // Click したなら true を、 Exit したなら false を返す
        // 同フレームなら Exit を優先する
        private static async UniTask<bool> WaitForClickOrExitAsync(Collider selfCollider, Collider playerCollider, Ct ct)
            => await UniTask.WhenAny(
                // 同フレームなら Exit を優先するために、このタイミングで待つ
                UniTask.WaitUntil(() => InputManager.InGame.Submit, timing: PlayerLoopTiming.LastUpdate, cancellationToken: ct),
                selfCollider.OnTriggerExitAsObservable()
                    .Where(otherCollider => ReferenceEquals(otherCollider, playerCollider))
                    .FirstAsync(cancellationToken: ct)
                    .AsUniTask()
            ) == 0;

        private static void UpdateSOSSignLeftRatioText(TextMeshProUGUI text, int leftCount, int totalCount)
        {
            text.SetTextFormat("穢れ度 : {0:F2}%", 100.0f * leftCount / totalCount);
        }
    }
}
