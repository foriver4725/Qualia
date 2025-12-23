using MyScripts.Common.SaveSystem;
using MyScripts.Runtime.Log;
using MyScripts.Runtime.UI.Main;

namespace MyScripts.Runtime
{
    internal sealed class SOSSignFindManager : MonoBehaviour, IDataHoldingObject
    {
        [Header("Self Components")]
        [SerializeField] private Transform root; // SOSサインの親オブジェクト (生成した後ここに格納する)
        [SerializeField] private Transform pointRoot; // 配置箇所の親オブジェクト
        [SerializeField] private SOSSign prefab;
        [Space(10)]
        [Header("Outer Components")]
        [SerializeField] private Collider playerCapsuleCollider;
        [SerializeField] private AnimalLeaveInvoker animalLeaveInvoker;
        [SerializeField] private SOSSignRatioUIManager sosSignRatioUIManager;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        // Awake で初期化
        private SSOSSignLogText sosSignLogText;
        private int removedSOSSignCount = -1;
        private Collider[] sosSigns; // コライダーはルートに付いている

        #region Interface Implementation

        public void GetDataAndUpdateMyProperties()
        {
            Span<bool> foundSOSSigns = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasFoundSOSSigns.AsSpan();

            int removeCount = 0;
            for (int i = 0; i < Constants.SOSSignCount; i++)
            {
                if (!foundSOSSigns[i])
                {
                    sosSigns[i].gameObject.SetActive(true);
                }
                else
                {
                    sosSigns[i].gameObject.SetActive(false);
                    removeCount++;
                }
            }

            removedSOSSignCount = removeCount;
        }

        public void SetMyPropertiesToData()
        {
            Span<bool> foundSOSSigns = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasFoundSOSSigns.AsSpan();

            int removeCount = 0;
            for (int i = 0; i < Constants.SOSSignCount; i++)
            {
                if (sosSigns[i].gameObject.activeSelf)
                {
                    foundSOSSigns[i] |= false; // 既に見つけたことがある場合は上書きしない
                }
                else
                {
                    foundSOSSigns[i] |= true; // 既に見つけたことがある場合は上書きしない
                    removeCount++;
                }
            }

            Assert.IsTrue(removeCount == removedSOSSignCount);
        }

        #endregion

        private void Awake()
        {
            sosSignLogText = InGameSOHolder.Instance.SOSSignLogText;
            Assert.IsTrue(Constants.SOSSignCount == pointRoot.childCount);

            sosSigns = new Collider[Constants.SOSSignCount];
            RandomlyInstantiateAndPlace(Constants.SOSSignCount, sosSigns);

            GetDataAndUpdateMyProperties();

            Setup(sosSigns);
        }

        private void Start()
        {
            // メソッドのコメントに従って、
            // - Awake() より後に呼び出す
            // - changeFillSmoothly は false にする
            sosSignRatioUIManager.UpdateRatio(1.0f * removedSOSSignCount / Constants.SOSSignCount, changeFillSmoothly: false);
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
                                    removedSOSSignCount++;
                                    sosSignRatioUIManager.UpdateRatio(1.0f * removedSOSSignCount / Constants.SOSSignCount);

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
                                sb.AppendFormat("{0}\n(アニマを取得していないと取り除けない)", sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnAnimalApproach));
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
        {
            int i = await UniTask.WhenAny(
                // 同フレームなら Exit を優先するために、このタイミングで待つ
                UniTask.WaitUntil(() => InputManager.InGame.Submit, timing: PlayerLoopTiming.LastUpdate, cancellationToken: ct),
                selfCollider.OnTriggerExitAsObservable()
                    .Where(otherCollider => ReferenceEquals(otherCollider, playerCollider))
                    .FirstAsync(cancellationToken: ct)
                    .AsUniTask()
            );

            if (i == 0)
            {
                InputManager.InGame.MakeSubmitInputDisabledUntilNextFrame();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
