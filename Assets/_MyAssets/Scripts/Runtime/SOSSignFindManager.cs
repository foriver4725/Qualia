using MyScripts.Common.SaveSystem;
using MyScripts.Runtime.Log;
using MyScripts.Runtime.UI.Main;
using UnityEngine.Video;

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
        [SerializeField] private TextMeshProUGUI sosAnimaArrangementSeedLabel;
        [SerializeField] private SStoryMovie storyMovie;

        // Awake で初期化
        private SSOSSignLogText sosSignLogText;
        private int sosSignCountReal;
        private int removedSOSSignCount = -1;
        private Collider[] sosSigns; // コライダーはルートに付いている

        // セーブデータから復元した初期達成率をもとに Start() で初期化する
        // ロード再開時に既に超えたマイルストーンを再生しないためのフラグ
        private bool hasPlayed33Movie;
        private bool hasPlayed66Movie;
        private bool hasPlayed100Movie;

        // マイルストーン判定に使う達成率の閾値
        private const float Ratio33 = 1.0f / 3.0f;
        private const float Ratio66 = 2.0f / 3.0f;

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
            // ここで一括生成する
            int seed = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].SOSAnimaArrangementSeed;
            sosAnimaArrangementSeedLabel.text = seed.ToString();
            SOSAnimaArranger.Instance.ArrangeRandomly(seed);

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

            // removedSOSSignCount が確定した直後にフラグを初期化する
            // ロード再開時に既に超えていたマイルストーンは再生しない
            {
                float initialRatio = 1.0f * removedSOSSignCount / Constants.SOSSignCount;
                hasPlayed33Movie  = initialRatio >= Ratio33;
                hasPlayed66Movie  = initialRatio >= Ratio66;
                hasPlayed100Movie = initialRatio >= 1.0f;
            }

            Setup(sosSigns);
        }

        private void Start()
        {
            // UpdateRatio() のコメントに従って、
            // - Awake() より後に呼び出す
            // - changeFillSmoothly は false にする
            sosSignRatioUIManager.UpdateRatio(
                1.0f * removedSOSSignCount / Constants.SOSSignCount,
                changeFillSmoothly: false);
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
                                    sosSignRatioUIManager.UpdateRatio(1.0f * removedSOSSignCount /
                                                                      Constants.SOSSignCount);

                                    // このタイミングで、セーブデータに反映しておく
                                    SetMyPropertiesToData();
                                }

                                LogManager.Instance.ShowManually(string.Empty);
                                LogManager.Instance.ShowAutomatically(
                                    sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnRemoveWithAnima));

                                soundPlayer.LetPlayWwise(SSOSSound.Situation.CouldRemove);
                                TryPlayMilestoneMovie();
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

                                    soundPlayer.LetPlayWwise(SSOSSound.Situation.CouldNotRemove);
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

        // 現在の達成率に応じて未再生のマイルストーン動画を再生する
        // Start() でセーブデータから初期フラグを設定しているため、ロード再開時の誤再生は起きない
        private void TryPlayMilestoneMovie()
        {
            if (storyMovie == null)
            {
                "SStoryMovie が設定されていません。".Print(LogSettings.Error);
                return;
            }

            float ratio = 1.0f * removedSOSSignCount / Constants.SOSSignCount;

            // 今回到達したマイルストーンを特定する
            SStoryMovie.GameProgress? progress = null;
            if (!hasPlayed33Movie && ratio >= Ratio33)
                progress = SStoryMovie.GameProgress.P33;
            else if (!hasPlayed66Movie && ratio >= Ratio66)
                progress = SStoryMovie.GameProgress.P66;
            else if (!hasPlayed100Movie && ratio >= 1.0f)
                progress = SStoryMovie.GameProgress.P100;

            if (!progress.HasValue) return;

            // クリップを取得してから null チェックし、有効な場合のみフラグを立てて再生する
            // フラグを先に立てると、クリップ未設定のマイルストーンが以後永久に再生不能になるため
            VideoClip clip = storyMovie.Get(progress.Value);
            if (clip == null)
            {
                $"マイルストーン {progress.Value} の VideoClip が設定されていません。".Print(LogSettings.Error);
                return;
            }

            switch (progress.Value)
            {
                case SStoryMovie.GameProgress.P33:  hasPlayed33Movie  = true; break;
                case SStoryMovie.GameProgress.P66:  hasPlayed66Movie  = true; break;
                case SStoryMovie.GameProgress.P100: hasPlayed100Movie = true; break;
            }

            CutScenePlayer.Instance.PlayAsync(clip, destroyCancellationToken).Forget();
        }

        // Click したなら true を、 Exit したなら false を返す
        // 同フレームなら Exit を優先する
        private static async UniTask<bool> WaitForClickOrExitAsync(Collider selfCollider, Collider playerCollider,
            Ct ct)
        {
            int i = await UniTask.WhenAny(
                // 同フレームなら Exit を優先するために、このタイミングで待つ
                UniTask.WaitUntil(() => InputManager.InGame.Submit, timing: PlayerLoopTiming.LastUpdate,
                    cancellationToken: ct),
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