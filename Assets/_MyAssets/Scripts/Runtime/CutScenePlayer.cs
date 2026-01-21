using System.IO;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace MyScripts.Runtime
{
    internal sealed class CutScenePlayer : ASingletonMonoBehaviour<CutScenePlayer>
    {
        [SerializeField, Range(0.0f, 5.0f)] private float bgFadeDuration = 0.5f;
        [SerializeField] private Image bg;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private SGameConfig gameConfig;

        private bool isPlaying = false;
        private SCutScene.CutSceneType currentPlayingType;

        // Awake で初期化
        private float bgAlphaMax;

        internal bool IsPlaying => isPlaying;

        private void Awake()
        {
            rawImage.enabled = false;
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = "";

            bgAlphaMax = bg.color.a;
            SetBgAlpha(0.0f);
            bg.enabled = false;

            videoPlayer.prepareCompleted += OnPrepareCompleted;
            videoPlayer.started += OnStarted;
            videoPlayer.loopPointReached += OnLoopPointReached;
        }

        private void OnDestroy()
        {
            videoPlayer.prepareCompleted -= OnPrepareCompleted;
            videoPlayer.started -= OnStarted;
            videoPlayer.loopPointReached -= OnLoopPointReached;
        }

        public async UniTask PlayAsync(SCutScene.CutSceneType type, Ct ct)
        {
            if (isPlaying)
            {
                $"既に{currentPlayingType}のカットシーンが再生中です。".Print(LogSettings.Warning);
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!gameConfig.DoesPlayIntroCutScene && type == SCutScene.CutSceneType.Intro)
            {
                "イントロカットシーンの再生は設定で無効化されています。".Print(LogSettings.Warning);
                return;
            }
#endif

            string url = InGameSOHolder.Instance.CutScene.Get(type);
            string saveName = ZString.Format("CutScene_{0}.mp4", type);
            string localPath = await DownloadVideoAsync(url, saveName, ct);
            if (string.IsNullOrEmpty(localPath))
            {
                "カットシーンのダウンロードに失敗したため、再生を中止します。".Print(LogSettings.Error);
                return;
            }

            // 状態を更新
            isPlaying = true;
            currentPlayingType = type;

            OnBeginPlay();

            // 再生する
            videoPlayer.url = ZString.Format("file://{0}", localPath);
            videoPlayer.Prepare();

            await UniTask.WaitUntil(() => !isPlaying, cancellationToken: ct);
        }

        #region 内部デリゲート

        private void OnPrepareCompleted(VideoPlayer _) => OnPrepareCompletedInternal();
        private void OnStarted(VideoPlayer _) => OnStartedInternal();
        private void OnLoopPointReached(VideoPlayer _) => OnLoopPointReachedInternal();

        private void OnPrepareCompletedInternal()
        {
            rawImage.texture = videoPlayer.texture;

            videoPlayer.Play();
        }

        private void OnStartedInternal()
        {
            rawImage.enabled = true;
        }

        private void OnLoopPointReachedInternal()
        {
            OnEndPlay();

            rawImage.enabled = false;

            // currentPlayingType は何もしない
            videoPlayer.url = "";
            isPlaying = false;
        }

        #endregion

        private void OnBeginPlay()
        {
            InputManager.DisableAllInputs();
            FadeInBgAsync(destroyCancellationToken).Forget();
        }

        private void OnEndPlay()
        {
            InputManager.EnableAllInputs();
            FadeOutBgAsync(destroyCancellationToken).Forget();
        }

        // ブラウザ上の動画URLをダウンロードしてローカル保存し、その絶対パスを返す
        // 保存する名前も指定する. 拡張子も含めて指定すること!
        // 失敗したら空文字列を返す
        private static async UniTask<string> DownloadVideoAsync(string url, string saveName, Ct ct)
        {
            $"Downloading video from URL: {url}".Print();

            string savePath = Path.Combine(Application.persistentDataPath, saveName);

            // 既にダウンロード済みでローカルに存在するなら、それを使う
            if (File.Exists(savePath))
            {
                $"Video already downloaded at: {savePath}. Using existing file.".Print();
                return savePath;
            }

            using var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerFile(savePath)
            {
                // ダウンロードに失敗した場合、途中までのファイルを削除する
                removeFileOnAbort = true
            };

            await request.SendWebRequest().ToUniTask(cancellationToken: ct);

            // ダウンロード失敗
            if (request.result != UnityWebRequest.Result.Success)
            {
                $"Failed to download video. Error: {request.error}".Print(LogSettings.Error);
                return "";
            }

            // ダウンロード成功
            $"Video downloaded successfully to: {savePath}".Print();
            return savePath;
        }

        // 重複実行はバグると思う
        private async UniTaskVoid FadeInBgAsync(Ct ct)
        {
            SetBgAlpha(0.0f);
            bg.enabled = true;

            await LMotion.Create(0.0f, bgAlphaMax, bgFadeDuration)
                        .WithEase(Ease.OutQuad)
                        .Bind(SetBgAlpha)
                        .ToUniTask(cancellationToken: ct);
        }

        // 重複実行はバグると思う
        private async UniTaskVoid FadeOutBgAsync(Ct ct)
        {
            SetBgAlpha(bgAlphaMax);

            await LMotion.Create(bgAlphaMax, 0.0f, bgFadeDuration)
                        .WithEase(Ease.InQuad)
                        .Bind(SetBgAlpha)
                        .ToUniTask(cancellationToken: ct);

            bg.enabled = false;
        }

        private void SetBgAlpha(float alpha)
        {
            Color color = bg.color;
            color.a = alpha;
            bg.color = color;
        }
    }
}
