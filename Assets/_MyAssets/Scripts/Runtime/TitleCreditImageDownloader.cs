using System.IO;

namespace MyScripts.Runtime
{
    /// <summary>
    /// <para>タイトルシーン開始時に、毎回実行される</para>
    /// <para>クレジット画像をクラウドから非同期ダウンロードし、RawImage にセットしておく</para>
    /// <para>成功したらコンポーネントをアクティブに、失敗したら非アクティブにする</para>
    /// </summary>
    internal sealed class TitleCreditImageDownloader : MonoBehaviour
    {
        [SerializeField] private SCloudFileUrl cloudFileUrl;
        [SerializeField] private RawImage targetRawImage;

        private void Awake() => Impl(destroyCancellationToken).Forget();

        private async UniTaskVoid Impl(Ct ct)
        {
            ct.ThrowIfCancellationRequested();

            string url = cloudFileUrl.Get(SCloudFileUrl.FileType.Image_Credit);
            string saveName = "TitleCreditTexture";

            "タイトルクレジット画像をダウンロード中...".Print();

            (bool success, string savePath) = await FileDownloader.DownloadAsync(
                url, saveName, FileDownloader.Extension.PNG, ct);
            ct.ThrowIfCancellationRequested();

            if (!success)
            {
                "タイトルクレジット画像のダウンロードに失敗しました。".Print(LogSettings.Error);
                targetRawImage.enabled = false; // 非アクティブにする
                return;
            }

            "タイトルクレジット画像のダウンロードに成功しました。テクスチャとして読み込み中...".Print();

            // ローカルに保存された画像ファイルをテクスチャとして読み込む
            byte[] imageData = await File.ReadAllBytesAsync(savePath, ct);
            ct.ThrowIfCancellationRequested();
            Texture2D texture = new(2, 2); // 空のテクスチャを作成 (サイズは後で自動調整されるので適当で良い)
            if (!texture.LoadImage(imageData))
            {
                "タイトルクレジット画像のテクスチャ読み込みに失敗しました。".Print(LogSettings.Error);
                targetRawImage.enabled = false;
                return;
            }

            // RawImage にセットしてアクティブにする
            targetRawImage.texture = texture;
            targetRawImage.enabled = true;

            "タイトルクレジット画像のダウンロードとセットに成功しました。".Print();
        }
    }
}
