using System.IO;
using UnityEngine.Networking;

namespace MyScripts.Common;

internal static class FileDownloader
{
    private enum Extension : byte
    {
        // 画像
        JPG, PNG,
        // 動画
        MP4, MOV,
        // サウンド
        MP3, WAV,
        // テキスト
        TXT, JSON,
    }

    private static readonly TimeSpan WholeTimeoutDefault = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan NoProgressTimeoutDefault = TimeSpan.FromSeconds(5);

    /// <summary>
    /// クラウドにあるファイルを、URLからダウンロードしてローカル保存する
    /// </summary>
    /// <param name="url">ダウンロード元のURL. 拡張子を含む. この名前でローカル保存する</param>
    /// <returns>ダウンロード成功ならば (true, ローカル保存パス)、失敗ならば (false, "") を返す</returns>
    internal static async UniTask<(bool Success, string Path)> DownloadFileAsync(this string url, Ct ct)
    {
        // ちょっと効率が悪い. ベース名と拡張子に分解して、委譲メソッドの内部で再結合される
        string saveName = Path.GetFileNameWithoutExtension(url);     // "A/B/C/file.png" -> "file"
        Extension extension = Path.GetExtension(url).GetExtension(); // "A/B/C/file.png" -> ".png" -> Extension.PNG

        return await DownloadFileAsync(
            url, saveName, extension, ct,
            WholeTimeoutDefault, NoProgressTimeoutDefault
        );
    }

    /// <summary>
    /// クラウドにあるファイルを、URLからダウンロードしてローカル保存する
    /// </summary>
    /// <param name="url">ダウンロード元のURL</param>
    /// <param name="saveName">ローカルに保存する名前. 拡張子は含めない</param>
    /// <param name="extension">拡張子. URLの文字列からは厳密に判定できないので、明示的に指定する</param>
    /// <param name="ct">キャンセレーショントークン</param>
    /// <param name="wholeTimeout">ダウンロード全体のタイムアウト時間. 通信が途切れても、この時間が過ぎるまではタイムアウトを通知しない</param>
    /// <param name="noProgressTimeout">通信なしタイムアウト時間. この時間ごとに通信の進捗をチェックし、進捗がなければタイムアウトと判定する</param>
    /// <returns>ダウンロード成功ならば (true, ローカル保存パス)、失敗ならば (false, "") を返す</returns>
    private static async UniTask<(bool Success, string Path)> DownloadFileAsync(
        string url, string saveName, Extension extension, Ct ct,
        TimeSpan wholeTimeout, TimeSpan noProgressTimeout
    )
    {
        ct.ThrowIfCancellationRequested();

        // TODO: URL の拡張子を見て、extension と矛盾していないかチェックする？

        // ローカルに保存する絶対パスを決定
        string savePath = Path.Combine(Application.persistentDataPath, saveName + extension.GetString());

        // 保存するディレクトリが存在しないなら、新規に作成する
        string saveDirectory = Path.GetDirectoryName(savePath) ?? "";
        if (!Directory.Exists(saveDirectory))
        {
            $"The directory to save the file does not exist: {saveDirectory}. Creating directory.".Print();
            Directory.CreateDirectory(saveDirectory);
        }

        // 既にローカルに存在する場合は、削除して、この後新規にダウンロードする
        if (File.Exists(savePath))
        {
            $"File already exists at: {savePath}. Deleting existing file.".Print();
            File.Delete(savePath);
        }

        // Webリクエストの準備
        using var request = UnityWebRequest.Get(url);
        {
            // ダウンロードに失敗した場合、途中までのファイルを削除する
            request.downloadHandler = new DownloadHandlerFile(savePath) { removeFileOnAbort = true };
            request.timeout = wholeTimeout.Seconds;
        }

        $"Starting download from URL: {url}".Print();

        try
        {
            await SendRequestWithNoProgressTimeout(request, ct, noProgressTimeout);
        }
        catch (UnityWebRequestException e)
        {
            // 200番台以外の失敗ステータスとかコネクションエラーなど
            $"UnityWebRequestException occurred: {e}".Print(LogSettings.Error);
            return (false, "");
        }
        catch (TimeoutException e)
        {
            // タイムアウト. ダイアログ表示など
            $"TimeoutException occurred: {e}".Print(LogSettings.Error);
            return (false, "");
        }

        // ダウンロード失敗
        if (request.result != UnityWebRequest.Result.Success)
        {
            $"Failed to download file. Error: {request.error}".Print(LogSettings.Error);
            return (false, "");
        }

        // ダウンロード成功
        $"File downloaded successfully to: {savePath}".Print();
        return (true, savePath);
    }

    /// <summary>
    /// 通信なしタイムアウトを設定してリクエストを送信する
    /// </summary>
    /// <param name="request">リクエスト</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <param name="noProgressTimeout">通信なしタイムアウト時間.
    /// この時間ごとに通信の進捗をチェックする.
    /// 短すぎるとタイムアウトが頻繁に起きてしまうので注意すること.
    /// </param>
    /// <exception cref="TimeoutException">通信なしタイムアウトが発生した場合</exception>
    /// <returns></returns>
    // このサイトを参考にした : https://zenn.dev/mingos/articles/c5435ecc974a1f
    private static async UniTask SendRequestWithNoProgressTimeout(UnityWebRequest request, Ct ct, TimeSpan noProgressTimeout)
    {
        using var compositeDisposable = new CompositeDisposable();
        bool isNoProgressTimeout = false;

        try
        {
            // 一定時間データが受信できない場合、通信なしタイムアウトと判定してリクエストを中断する
            Observable.Interval(noProgressTimeout)
                .Select(_ => new
                {
                    DownloadedBytes = request.downloadedBytes,
                    UploadedBytes = request.uploadedBytes
                })
                .Pairwise() // 前回と今回の進捗を取得
                .Where(pair =>
                {
                    // ダウンロード進捗は常にチェック
                    bool isDownloadedProgress = pair.Previous.DownloadedBytes != pair.Current.DownloadedBytes;

                    // アップロード進捗はPOST/PUTの場合のみチェック。それ以外は常にtrue
                    bool isUploadedProgress = true;
                    if (request.method == UnityWebRequest.kHttpVerbPOST || request.method == UnityWebRequest.kHttpVerbPUT)
                    {
                        isUploadedProgress = pair.Previous.UploadedBytes != pair.Current.UploadedBytes;
                    }

                    // どちらも進捗がなければタイムアウトと判定する
                    return !isDownloadedProgress && !isUploadedProgress;
                })
                .Subscribe(pair =>
                {
                    isNoProgressTimeout = true;
                    request.Abort();
                })
                .AddTo(compositeDisposable);

            await request.SendWebRequest().WithCancellation(ct);
        }
        catch (Exception e)
        {
            // 通信なしタイムアウトが発生した場合、ログを出力して例外をスローする
            if (isNoProgressTimeout)
            {
                $"通信なしタイムアウトが発生しました: method={request.method}, uri={request.uri}, error={e}".Print(LogSettings.Error);
                throw new TimeoutException("通信がタイムアウトしました");
            }
            else
            {
                // それ以外の例外はそのままスローする
                throw;
            }
        }
    }

    /// <summary>
    /// 列挙型 -> ドット付きの拡張子文字列<br/>
    /// Extension.PNG -> ".png"<br/>
    /// Extension.MP4 -> ".mp4"
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetString(this Extension extension) => extension switch
    {
        Extension.JPG => ".jpg",
        Extension.PNG => ".png",
        Extension.MP4 => ".mp4",
        Extension.MOV => ".mov",
        Extension.MP3 => ".mp3",
        Extension.WAV => ".wav",
        Extension.TXT => ".txt",
        Extension.JSON => ".json",
        _ => throw new ArgumentOutOfRangeException(nameof(extension), extension, null)
    };

    /// <summary>
    /// ドット付きの拡張子文字列 -> 列挙型<br/>
    /// ".png" -> Extension.PNG<br/>
    /// ".mp4" -> Extension.MP4
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Extension GetExtension(this string extensionStr) => extensionStr.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => Extension.JPG,
        ".png" => Extension.PNG,
        ".mp4" => Extension.MP4,
        ".mov" => Extension.MOV,
        ".mp3" => Extension.MP3,
        ".wav" => Extension.WAV,
        ".txt" => Extension.TXT,
        ".json" => Extension.JSON,
        _ => throw new ArgumentOutOfRangeException(nameof(extensionStr), extensionStr, null)
    };
}
