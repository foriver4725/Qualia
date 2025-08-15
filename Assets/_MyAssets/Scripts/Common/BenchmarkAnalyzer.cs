using Unity.Profiling;
using UnityEngine.Profiling;

namespace MyScripts.Common
{
    internal sealed class BenchmarkAnalyzer : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI text;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        // 一定時間ごとに計測する
        private float time = 0f;
        private float baseTime = 0f;

        private int fpsCount = 0;
        private float fps = -1f;

        float allocatedMemory = -1f;
        float reservedMemory = -1f;
        float unusedReservedMemory = -1f;
        float memoryUsingRate = -1f;

        private int gcCountInit = -1;
        private int gcCount = -1;

        private ProfilerRecorder setPassCallsRecorder;
        private ProfilerRecorder drawCallsRecorder;
        private long setPassCalls = -1;
        private long drawCalls = -1;

        private void Awake()
        {
            setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        }

        private void OnDestroy()
        {
            setPassCallsRecorder.Dispose();
            drawCallsRecorder.Dispose();
        }

        private void Update()
        {
            fpsCount++;
            time = Time.realtimeSinceStartup - baseTime;
            if (time >= 0.5f)
            {
                UpdateStats();
                UpdateUI();
            }
        }

        private void UpdateStats()
        {
            fps = fpsCount / time;
            fpsCount = 0;
            baseTime = Time.realtimeSinceStartup;

            allocatedMemory = ByteToMegabyte(Profiler.GetTotalAllocatedMemoryLong());
            reservedMemory = ByteToMegabyte(Profiler.GetTotalReservedMemoryLong());
            unusedReservedMemory = ByteToMegabyte(Profiler.GetTotalUnusedReservedMemoryLong());
            memoryUsingRate = allocatedMemory / reservedMemory;

            if (gcCountInit == -1)
                gcCountInit = GC.CollectionCount(0);
            gcCount = GC.CollectionCount(0) - gcCountInit;

            if (setPassCallsRecorder.Valid)
            {
                long lastValue = setPassCallsRecorder.LastValue;
                if (lastValue > 0)
                    setPassCalls = lastValue;
            }
            if (drawCallsRecorder.Valid)
            {
                long lastValue = drawCallsRecorder.LastValue;
                if (lastValue > 0)
                    drawCalls = lastValue;
            }
        }

        private void UpdateUI()
        {
            if (text == null) return;

            string fpsColorText = (fps switch
            {
                > 24 => Color.green,
                > 18 => Color.yellow,
                _ => Color.red
            }).ToHtmlStringRGB();
            string memoryUsingColorText = (allocatedMemory switch
            {
                < 1200 => Color.green,
                < 2400 => Color.yellow,
                _ => Color.red
            }).ToHtmlStringRGB();
            string gcCountColorText = (gcCount switch
            {
                0 => Color.green,
                < 4 => Color.yellow,
                _ => Color.red
            }).ToHtmlStringRGB();
            string setPassCallsColorText = (setPassCalls switch
            {
                < 200 => Color.green,
                < 400 => Color.yellow,
                _ => Color.red
            }).ToHtmlStringRGB();
            string drawCallsColorText = (drawCalls switch
            {
                < 600 => Color.green,
                < 1200 => Color.yellow,
                _ => Color.red
            }).ToHtmlStringRGB();

            using var sb = ZString.CreateStringBuilder();
            sb.AppendFormat("FPS : <color=#{0}>{1:F2}</color>,    ",
                fpsColorText, fps);
            sb.AppendFormat("Memory(MB) : <color=#{0}>{1:F2}/{2:F2} ({3:P2}, {4:F2} unused)</color>,    ",
                memoryUsingColorText, allocatedMemory, reservedMemory, memoryUsingRate, unusedReservedMemory);
            sb.AppendFormat("GC.Collect : <color=#{0}>{1}</color>,    ",
                gcCountColorText, gcCount);
            sb.AppendFormat("Calls : SetPass=<color=#{0}>{2}</color>,Draw=<color=#{1}>{3}</color>,    ",
                setPassCallsColorText, drawCallsColorText, setPassCalls, drawCalls);
            text.SetText(sb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ByteToMegabyte(long n) => (n >> 10) / 1024f;

#else
        private void Awake()
        {
            root.SetActive(false);
            this.enabled = false;
        }
#endif
    }
}