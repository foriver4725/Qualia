using UnityEngine.Profiling;

namespace MyScripts.Common
{
    internal sealed class BenchmarkAnalyzer : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI text;

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private readonly StringBuilder sb = new(256);

        private int cnt = 0;
        private float preT = 0f;
        private float fps = 0f;

        // FPSと一緒のタイミングで処理
        private int gcCntInit = -1;
        private int gcCnt = 0;

        private void Update()
        {
            cnt++;
            float t = Time.realtimeSinceStartup - preT;
            if (t >= 0.5f)
            {
                fps = cnt / t;
                cnt = 0;
                preT = Time.realtimeSinceStartup;

                if (gcCntInit == -1)
                    gcCntInit = GC.CollectionCount(0);
                gcCnt = GC.CollectionCount(0) - gcCntInit;
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
                > 24 => Color.green,
                > 18 => Color.yellow,
                _ => Color.red
            };
            Color memColor = allocatedMemory switch
            {
                < 1200 => Color.green,
                < 2400 => Color.yellow,
                _ => Color.red
            };
            Color gcCntColor = gcCnt switch
            {
                0 => Color.green,
                < 4 => Color.yellow,
                _ => Color.red
            };

            sb.Clear();
            sb.AppendFormat("FPS : <color=#{0}>{1:F2}</color>,    ",
                ColorUtility.ToHtmlStringRGB(fpsColor), fps
            );
            sb.AppendFormat("Memory(MB) : <color=#{0}>{1:F2}/{2:F2} ({3:P2}, {4:F2} unused)</color>,    ",
                ColorUtility.ToHtmlStringRGB(memColor), allocatedMemory, reservedMemory, memoryP, unusedReservedMemory
            );
            sb.AppendFormat("GC.Collect : <color=#{0}>{1}</color>,    ",
                ColorUtility.ToHtmlStringRGB(gcCntColor), gcCnt
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

#else
        private void Awake()
        {
            root.SetActive(false);
            this.enabled = false;
        }
#endif
    }
}