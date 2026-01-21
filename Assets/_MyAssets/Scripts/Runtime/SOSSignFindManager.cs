using MyScripts.Common.SaveSystem;
using MyScripts.Runtime.Log;
using MyScripts.Runtime.UI.Main;

namespace MyScripts.Runtime
{
    internal sealed class SOSSignFindManager : MonoBehaviour, IDataHoldingObject
    {
        [Header("Self Components")]
        [SerializeField] private Transform root; // SOSサインの親オブジェクト
        [Space(10)]
        [Header("Outer Components")]
        [SerializeField] private Collider playerCapsuleCollider;
        [SerializeField] private AnimalLeaveInvoker animalLeaveInvoker;
        [SerializeField] private SOSSignRatioUIManager sosSignRatioUIManager;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        // Awake で初期化
        private SSOSSignLogText sosSignLogText;
        private int sosSignCountReal;
        private int removedSOSSignCount = -1;
        private Collider[] sosSigns; // コライダーはルートに付いている

        #region Interface Implementation

        public void GetDataAndUpdateMyProperties()
        {
            Span<bool> foundSOSSigns = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasFoundSOSSigns.AsSpan();

            int removeCount = 0;
            for (int i = 0; i < sosSignCountReal; i++)
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
            for (int i = 0; i < sosSignCountReal; i++)
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

            sosSignCountReal = root.childCount;
            Assert.IsTrue(sosSignCountReal <= Constants.SOSSignCount);
            if (sosSignCountReal < Constants.SOSSignCount)
            {
                "SOSサインの数が不足しています。".Print(LogSettings.Error);
            }

            sosSigns = new Collider[sosSignCountReal];
            FetchAllInstance(sosSignCountReal, sosSigns);

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

        // SOSサインを取得する
        private void FetchAllInstance(int count, Span<Collider> outColliders)
        {
            for (int i = 0; i < count; i++)
                outColliders[i] = root.GetChild(i).GetComponent<Collider>();
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
                            LogManager.Instance.ShowManually(
                                sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnTouchWithAnima));

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
                                    sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnRemoveWithAnima));

                                soundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);
                            }
                            else
                            {
                                LogManager.Instance.ShowManually(string.Empty);
                            }
                        }
                        else
                        {
                            LogManager.Instance.ShowManually(
                                sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnTouchWithoutAnima));

                            while (true)
                            {
                                if (await WaitForClickOrExitAsync(selfCollider, playerCapsuleCollider, ct) == true)
                                {
                                    LogManager.Instance.ShowManually(
                                        sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnRemoveWithoutAnima));

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
