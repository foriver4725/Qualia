namespace MyScripts.Runtime
{
    internal sealed class DebugDescriptionToggler : MonoBehaviour
    {
        [SerializeField] private Canvas ui;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const bool Enable = true;
#else
        private const bool Enable = false;
#endif

        private void Awake() => ui.gameObject.SetActive(Enable);
    }
}
