using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title
{
    internal sealed class SaveSlotSelectCtor : AViewConstructor
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

        [SerializeField] private Button.ASelectableButtonManager defaultSelectedButton;
        [SerializeField] private Button.ASelectableButtonManager[] allButtons;

        // セーブスロットの順番に
        [SerializeField] private SlotInfo[] slotInfos;
        [SerializeField] private SaveSlotManager[] slotManagers;
        [SerializeField] private Sprite defaultThumbnailSprite;
        [SerializeField] private Color progressTextColorWhen100Percent;

        private static readonly Texture2D[] cachedThumbnailTextures = new Texture2D[Constants.SlotCount];
        private static readonly Sprite[] cachedThumbnailSprites = new Sprite[Constants.SlotCount];

        private void Awake()
        {
            for (int i = 0; i < Constants.SlotCount; i++)
            {
                if (!cachedThumbnailSprites[i])
                    (cachedThumbnailTextures[i], cachedThumbnailSprites[i]) = LoadThumbnailImage(i);
            }
        }

        private void OnDestroy()
        {
            foreach (var texture in cachedThumbnailTextures)
            {
                if (texture)
                    Destroy(texture);
            }
            Array.Clear(cachedThumbnailTextures, 0, cachedThumbnailTextures.Length);

            foreach (var sprite in cachedThumbnailSprites)
            {
                if (sprite)
                    Destroy(sprite);
            }
            Array.Clear(cachedThumbnailSprites, 0, cachedThumbnailSprites.Length);
        }

        internal sealed override void Construct()
        {
            defaultSelectedButton.SelectThisForciblyUnsafe();

            Span<SingleData> slotDatas = SaveLoadManager.Data.Slots;
            for (int i = 0; i < slotDatas.Length; i++)
            {
                var slotData = slotDatas[i];
                if (slotData.IsValid)
                {
                    // 達成度を計算する
                    int removeCount = 0;
                    foreach (bool hasFound in slotData.HasFoundSOSSigns.AsSpan())
                    {
                        if (hasFound)
                            removeCount++;
                    }
                    float removeRatio = 100.0f * removeCount / Constants.SOSSignCount;

                    slotInfos[i].ThumbnailImage.sprite = (cachedThumbnailSprites[i] ?
                        cachedThumbnailSprites[i] : defaultThumbnailSprite);
                    slotInfos[i].ProgressText.SetTextFormat("{0:F2}%", removeRatio);
                    slotInfos[i].ProgressText.color = (removeCount >= Constants.SOSSignCount ?
                        progressTextColorWhen100Percent : Color.white);
                    slotInfos[i].DateText.text = ZString.Format("{0:yyyy-MM-dd}\n{0:HH:mm}", slotData.GetLastSavedAt());
                }
                else
                {
                    slotInfos[i].ThumbnailImage.sprite = defaultThumbnailSprite;
                    slotInfos[i].ProgressText.text = "-";
                    slotInfos[i].ProgressText.color = Color.white;
                    slotInfos[i].DateText.text = "-";
                }
            }
        }

        internal sealed override void Deconstruct()
        {
            foreach (var button in allButtons)
            {
                if (button.IsSelected)
                    button.DeselectThisForciblyUnsafe();
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
