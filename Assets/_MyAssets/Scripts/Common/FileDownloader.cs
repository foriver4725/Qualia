using System.IO;
using UnityEngine.Networking;

namespace MyScripts.Common;

internal static class FileDownloader
{
    internal enum Extension : byte
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

    /// <summary>
    /// クラウドにあるファイルを、URLからダウンロードしてローカル保存する
    /// </summary>
    /// <param name="url">ダウンロード元のURL</param>
    /// <param name="saveName">ローカルに保存する名前. 拡張子は含めない</param>
    /// <param name="extension">拡張子. URLの文字列からは厳密に判定できないので、明示的に指定する</param>
    /// <param name="ct">キャンセレーショントークン</param>
    /// <param name="timeoutSeconds">Webリクエストのタイムアウト時間 (秒)</param>
    /// <returns>ダウンロード成功ならば (true, ローカル保存パス)、失敗ならば (false, "") を返す</returns>
    internal static async UniTask<(bool Success, string Path)> DownloadAsync(
        string url, string saveName, Extension extension,
        Ct ct, int timeoutSeconds = 5
    )
    {
        ct.ThrowIfCancellationRequested();

        // ドット付きの拡張子文字列を取得
        // Extension.PNG -> ".png"
        // Extension.MP4 -> ".mp4"
        string extensionWithDot = extension switch
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

        // TODO: URL の拡張子を見て、extension と矛盾していないかチェックする？

        // ローカルに保存する絶対パスを決定
        string savePath = Path.Combine(Application.persistentDataPath, saveName + extensionWithDot);

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
            request.timeout = timeoutSeconds;
        }

        $"Starting download from URL: {url}".Print();

        ct.ThrowIfCancellationRequested();
        await request.SendWebRequest().ToUniTask(cancellationToken: ct);
        ct.ThrowIfCancellationRequested();

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
}
