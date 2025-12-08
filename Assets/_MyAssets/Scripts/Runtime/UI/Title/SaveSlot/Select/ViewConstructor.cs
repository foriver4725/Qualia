using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Select
{
    internal sealed class ViewConstructor : AViewConstructor
    {
        [Serializable]
        private sealed class SlotInfo
        {
            [SerializeField] private Image thumbnailImage;
            [SerializeField] private TextMeshProUGUI progressText;
            [SerializeField] private TextMeshProUGUI dateText;

            internal Image ThumbnailImage => thumbnailImage;
            internal TextMeshProUGUI ProgressText => progressText;
            internal TextMeshProUGUI DateText => dateText;
        }

        // セーブスロットの順番に
        [SerializeField] private SlotInfo[] slotInfos;
        // 最初の要素が、最初に選択される
        [SerializeField] private SlotManager[] slotManagers;

        private static readonly Texture2D[] cachedThumbnailTextures = new Texture2D[Constants.SlotCount];
        private static readonly Sprite[] cachedThumbnailSprites = new Sprite[Constants.SlotCount];

        private void Awake()
        {
            // テクスチャやスプライトのキャッシュを初期化
            throw new NotImplementedException();
        }

        private void OnDestroy()
        {
            // テクスチャやスプライトのキャッシュを解放
            throw new NotImplementedException();
        }

        internal sealed override void Construct()
        {
            slotManagers[0].SelectThisForciblyUnsafe();

            Span<SingleData> slotDatas = SaveLoadManager.Data.Slots;
            for (int i = 0; i < slotDatas.Length; i++)
            {
                var slotData = slotDatas[i];
                if (slotData.IsValid)
                {
                    // 穢れ度を計算する
                    int leftCount = 0;
                    foreach (bool hasFound in slotData.HasFoundSOSSigns.AsSpan())
                    {
                        if (!hasFound)
                            leftCount++;
                    }
                    float leftRatio = 100.0f * leftCount / Constants.SOSSignCount;

                    slotInfos[i].ThumbnailImage.sprite = null; // セーブデータからサムネイル画像を取得してセットするなど
                    slotInfos[i].ProgressText.SetTextFormat("{0:F2}%", leftRatio);
                    slotInfos[i].DateText.text = ZString.Format("{0:yyyy-MM-dd}\n{0:HH:mm}", slotData.GetLastSavedAt());
                }
                else
                {
                    slotInfos[i].ThumbnailImage.sprite = null; // デフォルトの画像にするなど
                    slotInfos[i].ProgressText.text = "-";
                    slotInfos[i].DateText.text = "-";
                }
            }
        }

        internal sealed override void Deconstruct()
        {
            foreach (var slotManager in slotManagers)
            {
                if (slotManager.IsSelected)
                    slotManager.DeselectThisForciblyUnsafe();
            }
        }

        //! 新しくテクスチャ、スプライトを作成する
        private static (Texture2D Texture, Sprite Sprite) LoadThumbnailImage(int slotIndex)
        {
            string filePath = SaveLoadManager.Data.Slots[slotIndex].LastScreenshotSavedPath;
            // セーブファイルのパスが無い
            if (string.IsNullOrEmpty(filePath))
                return (null, null);

            Texture2D texture = ScreenshotManager.Load(filePath);
            // ロード失敗
            if (!texture)
                return (null, null);

            // スプライト化する
            Sprite sprite = Sprite.Create(
                texture,
                new(0, 0, texture.width, texture.height),
                new(0.5f, 0.5f)
            );

            return (texture, sprite);
        }
    }
}
