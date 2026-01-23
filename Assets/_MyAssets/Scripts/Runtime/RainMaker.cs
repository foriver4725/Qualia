namespace MyScripts.Runtime
{
    internal sealed class RainMaker : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private MeshRenderer rainRenderer;
        [SerializeField] private AudioLowPassFilter rainAudioLowPassFilter;
        [SerializeField, Range(0.0f, 1.0f)] private float sunIntensityMultiplierDuringRain = 0.5f;
        [SerializeField, MinMaxRange(0.0f, 3600.0f)] private Vector2 rainIntervalSeconds = new Vector2(90.0f, 300.0f);
        [SerializeField, MinMaxRange(0.0f, 3600.0f)] private Vector2 rainDurationSeconds = new Vector2(20.0f, 45.0f);

        // Awake で初期化
        private float initialSunIntensity;

        private void Awake()
        {
            initialSunIntensity = sun.intensity;
            rainRenderer.enabled = false;
            rainAudioLowPassFilter.enabled = false;

            TriggerRainCyclicallyAsync(destroyCancellationToken).Forget();
        }

        private async UniTaskVoid TriggerRainCyclicallyAsync(Ct ct)
        {
            ct.ThrowIfCancellationRequested();

            while (true)
            {
                float intervalSeconds = Random.Range(rainIntervalSeconds.x, rainIntervalSeconds.y);
                float durationSeconds = Random.Range(rainDurationSeconds.x, rainDurationSeconds.y);

                await intervalSeconds.SecAwait(ct: ct);

                sun.intensity = initialSunIntensity * sunIntensityMultiplierDuringRain;
                rainRenderer.enabled = true;
                rainAudioLowPassFilter.enabled = true;

                await durationSeconds.SecAwait(ct: ct);

                sun.intensity = initialSunIntensity;
                rainRenderer.enabled = false;
                rainAudioLowPassFilter.enabled = false;
            }
        }
    }
}
