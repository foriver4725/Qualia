namespace MyScripts.Runtime
{
    internal interface IImageSequencePlayerPlayOptions
    {
        /// <summary>
        /// 自動で次に流れていくか
        /// </summary>
        bool IsAutoStepEnabled { get; }

        /// <summary>
        /// ユーザーの操作により、次の画像に切り替えることを許可するか
        /// </summary>
        bool IsManualSkipEnabled { get; }

        /// <summary>
        /// 自動で流れていく場合、その間隔
        /// </summary>
        TimeSpan AutoStepDuration { get; }

        /// <summary>
        /// ユーザー操作によりスキップできる場合、このメソッドが true を返したら、次の画像に切り替える
        /// </summary>
        Func<bool> ManualSkipInputChecker { get; }

        /// <summary>
        /// 最初と最後に、背景がフェードするアニメーション時間
        /// </summary>
        TimeSpan BgFadeDuration { get; }

        /// <summary>
        /// 画像がフェードして切り替わる時間
        /// </summary>
        TimeSpan StepFadeDuration { get; }
    }

    [Serializable]
    internal sealed class ImageSequencePlayerPlayOptions : IImageSequencePlayerPlayOptions
    {
        private enum ManualSkipInputCheckWay : byte
        {
            WhenAnyInputWasPressedThisFrame,
        }

        [SerializeField] private bool isAutoStepEnabled = true;
        [SerializeField] private bool isManualSkipEnabled = true;
        [SerializeField, Range(0.0f, 60.0f)] private float autoStepDurationSeconds = 5.0f;

        [SerializeField] private ManualSkipInputCheckWay manualSkipInputCheckWay
            = ManualSkipInputCheckWay.WhenAnyInputWasPressedThisFrame;

        [SerializeField, Range(0.0f, 5.0f)] private float bgFadeDurationSeconds = 0.5f;
        [SerializeField, Range(0.0f, 5.0f)] private float stepFadeDurationSeconds = 0.5f;

        public bool IsAutoStepEnabled => isAutoStepEnabled;
        public bool IsManualSkipEnabled => isManualSkipEnabled;
        public TimeSpan AutoStepDuration => TimeSpan.FromSeconds(autoStepDurationSeconds);

        public Func<bool> ManualSkipInputChecker => manualSkipInputCheckWay switch
        {
            ManualSkipInputCheckWay.WhenAnyInputWasPressedThisFrame
                => static () => InputManager.CheckForAnyRawInputWasPressedThisFrame(),
            _
                => throw new ArgumentOutOfRangeException(nameof(manualSkipInputCheckWay), manualSkipInputCheckWay, null)
        };

        public TimeSpan BgFadeDuration => TimeSpan.FromSeconds(bgFadeDurationSeconds);
        public TimeSpan StepFadeDuration => TimeSpan.FromSeconds(stepFadeDurationSeconds);
    }
}