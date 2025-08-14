namespace MyScripts.Common
{
    internal sealed class VersionDisplayer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private void Awake()
        {
            text.text = $"v{Application.version}";
        }
    }
}