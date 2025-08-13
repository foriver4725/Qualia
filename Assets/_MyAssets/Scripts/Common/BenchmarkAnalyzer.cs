using UnityEngine.Profiling;

namespace MyScripts.Common
{
    internal sealed class BenchmarkAnalyzer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private readonly StringBuilder sb = new(256);

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private int cnt = 0;
        private float preT = 0f;
        private float fps = 0f;

        private void Update()
        {
            cnt++;
            float t = Time.realtimeSinceStartup - preT;
            if (t >= 0.5f)
            {
                fps = cnt / t;
                cnt = 0;
                preT = Time.realtimeSinceStartup;
            }

            UpdateText();
        }

        private void UpdateText()
        {
            if (text == null) return;

            float allocatedMemory = GetAllocatedMemory();
            float reservedMemory = GetReservedMemory();
            float unusedReservedMemory = GetUnusedReservedMemory();
            float memoryP = allocatedMemory / reservedMemory;

            Color fpsColor = fps switch
            {
                > 50 => Color.green,
                > 30 => Color.yellow,
                _ => Color.red
            };
            Color memColor = memoryP switch
            {
                < 0.5f => Color.green,
                < 0.8f => Color.yellow,
                _ => Color.red
            };

            sb.Clear();
            sb.AppendFormat("FPS:<color=#{5}>{0:F2}</color>, Memory(MB):<color=#{6}>{1:F2}/{2:F2}({3:P2},{4:F2} unused)</color>",
                fps, allocatedMemory, reservedMemory, memoryP, unusedReservedMemory,
                ColorUtility.ToHtmlStringRGB(fpsColor), ColorUtility.ToHtmlStringRGB(memColor)
            );
            text.text = sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetAllocatedMemory() => ByteToMegabyte(Profiler.GetTotalAllocatedMemoryLong());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetReservedMemory() => ByteToMegabyte(Profiler.GetTotalReservedMemoryLong());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetUnusedReservedMemory() => ByteToMegabyte(Profiler.GetTotalUnusedReservedMemoryLong());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ByteToMegabyte(long n) => (n >> 10) / 1024f;

#endif
    }
}