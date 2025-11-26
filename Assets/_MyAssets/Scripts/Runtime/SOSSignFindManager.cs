namespace MyScripts.Runtime
{
    internal sealed class SOSSignFindManager : MonoBehaviour
    {
        [SerializeField] private Transform root; // SOSサインの親オブジェクト (生成した後ここに格納する)
        [SerializeField] private Transform pointRoot; // 配置箇所の親オブジェクト
        [SerializeField] private SOSSign prefab;
        [Space(10)]
        [SerializeField] private Collider playerCapsuleCollider;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        // Awake で初期化
        private ParticleSystem[] sosSigns;
        private SSOSSignLogText sosSignLogText;

        private async UniTaskVoid Awake()
        {
            sosSignLogText = InGameSOHolder.Instance.SOSSignLogText;

            RandomlyInstantiateAndPlace(out sosSigns, out Collider[] outColliders);
            // 外部からのデリゲート登録を確実に待つ
            await UniTask.NextFrame(destroyCancellationToken);
            Setup(outColliders);
        }

        // プレハブから生成して、ランダムに配置する
        // インスタンスを格納する用の配列を GC.Alloc して、それを返す
        private void RandomlyInstantiateAndPlace(out ParticleSystem[] outParticleSystems, out Collider[] outColliders)
        {
            // 配置箇所に一括でSOSサインを配置
            SOSSignPoint[] candidatePoints = new SOSSignPoint[pointRoot.childCount];
            for (int i = 0; i < pointRoot.childCount; i++)
            {
                candidatePoints[i] = pointRoot.GetChild(i).GetComponent<SOSSignPoint>();
            }
            int count = candidatePoints.Length; // 配置できた総数

            // 難易度による制限も踏まえて、結局配置できる数
            outParticleSystems = new ParticleSystem[count];
            outColliders = new Collider[count];

            for (int i = 0; i < count; i++)
            {
                SOSSign instance = Instantiate(prefab, candidatePoints[i].transform.position, candidatePoints[i].transform.rotation, root);

                outParticleSystems[i] = instance.ParticleSystem;
                outColliders[i] = instance.Collider;
            }
        }

        private void Setup(ReadOnlySpan<Collider> sosSignColliders)
        {
            foreach (Collider sosSignCollider in sosSignColliders)
            {
                Collider col = sosSignCollider;

                col.OnTriggerEnterAsObservable()
                    .Where(c => ReferenceEquals(c, playerCapsuleCollider))
                    .SubscribeAwait(col, async (c, col, ct) =>
                {
                    if (true)
                    {
                        "一旦、必ず取り除けるようにする".Print(LogSettings.Warning);
                        LogManager.Instance.ShowManually("左クリックで取り除く");

                        if (await WaitForClickOrExitAsync(col, ct) == true)
                        {
                            // 取り除く
                            col.gameObject.SetActive(false);

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
                            if (await WaitForClickOrExitAsync(col, ct) == true)
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
    }
}
