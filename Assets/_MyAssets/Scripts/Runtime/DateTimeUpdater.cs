using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime
{
    // DateTime.Now を毎フレーム更新して、セーブデータにセットする
    internal sealed class DateTimeUpdater : MonoBehaviour
    {
        private void Update()
        {
            // ドンドン新しいものに更新していく
            var lastSavedAt = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].GetLastSavedAt();
            var current = DateTime.Now;
            if (current > lastSavedAt)
                SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].SetLastSavedAt(current);
        }
    }
}
