namespace MyScripts.Runtime
{
    internal sealed class StartManager : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite[] sprites; // 順に表示していく
        [SerializeField, Range(0.0f, 10.0f)] private float fadeDuration = 0.5f;
        [SerializeField, Range(0.0f, 10.0f)] private float enabledDuration = 2.0f;
        [SerializeField, Range(0.0f, 10.0f)] private float disabledDuration = 0.5f;

        private void Start() => Impl(destroyCancellationToken).Forget();

        private async UniTaskVoid Impl(Ct ct)
        {
            ct.ThrowIfCancellationRequested();

            image.sprite = null;
            image.enabled = false;

            SetImageAlpha(0.0f);

            foreach (Sprite sprite in sprites)
            {
                await disabledDuration.SecAwait(ct: ct);

                image.sprite = sprite;
                image.enabled = true;

                await LMotion.Create(0.0f, 1.0f, fadeDuration)
                   .WithEase(Ease.OutQuad)
                   .Bind(SetImageAlpha)
                   .ToUniTask(cancellationToken: ct);

                await enabledDuration.SecAwait(ct: ct);

                await LMotion.Create(1.0f, 0.0f, fadeDuration)
                   .WithEase(Ease.InQuad)
                   .Bind(SetImageAlpha)
                   .ToUniTask(cancellationToken: ct);

                image.sprite = null;
                image.enabled = false;
            }

            ct.ThrowIfCancellationRequested();
            Scene.Title.LoadAsync().Forget();
        }

        private void SetImageAlpha(float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
