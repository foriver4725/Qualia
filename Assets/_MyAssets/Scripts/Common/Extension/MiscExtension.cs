using UnityEngine.EventSystems;

namespace MyScripts.Common.Extension;

internal static class MiscExtension
{
    /// <summary>
    /// EventTriggerにイベントを登録する
    /// </summary>
    internal static void AddListener(this EventTrigger eventTrigger, EventTriggerType type,
                                     Action<PointerEventData> action)
    {
        EventTrigger.Entry entry = new() { eventID = type };
        entry.callback.AddListener(data =>
        {
            if (data is PointerEventData pointerData)
                action?.Invoke(pointerData);
        });
        eventTrigger.triggers.Add(entry);
    }
}
