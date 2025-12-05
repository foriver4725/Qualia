using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Select
{
    // UIが有効になるたびに実行するべき
    // 現在の数値を基に、見た目を再構成する
    internal sealed class ViewConstructor : MonoBehaviour
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

        internal void Construct()
        {
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

                    slotInfos[i].ThumbnailImage.sprite = null;
                    slotInfos[i].ProgressText.SetTextFormat("{0:F2}%", leftRatio);
                    slotInfos[i].DateText.text = "yyyy/MM/dd\nHH:mm";
                }
                else
                {
                    slotInfos[i].ThumbnailImage.sprite = null; // デフォルトの画像にするなど
                    slotInfos[i].ProgressText.text = "空きスロット";
                    slotInfos[i].DateText.text = "-\n-";
                }
            }
        }
    }
}
