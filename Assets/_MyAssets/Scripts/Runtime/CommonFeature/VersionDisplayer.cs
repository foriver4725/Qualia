namespace MyScripts.Runtime
{
    internal sealed class VersionDisplayer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private void Awake() => text.SetTextFormat("v{0}", Application.version);
    }
}
