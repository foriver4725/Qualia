namespace MyScripts.Runtime
{
    internal sealed class SOSSignFindManager : MonoBehaviour
    {
        [SerializeField] private Collider playerCapsuleCollider;

        internal void Setup(
            ReadOnlyCollection<Collider> sosSignColliders,
            Func<bool> isCharacterHuman,
            Action onFind // スコア更新など、見つけたとき共通の処理
        )
        {
            foreach (Collider sosSignCollider in sosSignColliders)
            {
                Collider col = sosSignCollider;

                // プレイヤーが自身にTriggerEnterして、
                // プレイヤーが人間のキャラクターの時...
                col.OnTriggerEnterAsObservable()
                    .Where(c =>
                        ReferenceEquals(c, playerCapsuleCollider)
                        && isCharacterHuman?.Invoke() == true
                    )
                    .SubscribeAwait(async (c, ct) =>
                    {
                        LogManager.Instance.ShowManually("左クリックで取り除く");

                        // 決定の入力 or TriggerExit まで待つ
                        int i = await UniTask.WhenAny(
                            UniTask.WaitUntil(() => InputManager.Instance.InGameSubmit.Bool, cancellationToken: ct),
                            col.OnTriggerExitAsObservable()
                                .Where(c => ReferenceEquals(c, playerCapsuleCollider))
                                .FirstAsync(cancellationToken: ct)
                                .AsUniTask()
                        );

                        if (i == 0) // 決定された
                        {
                            // 取り除く
                            col.gameObject.SetActive(false);
                            onFind?.Invoke();
                        }

                        LogManager.Instance.ShowManually(string.Empty);
                    })
                    .AddTo(col);
            }
        }
    }
}