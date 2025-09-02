namespace MyScripts.Runtime
{
    internal abstract class ASoundPlayer : MonoBehaviour
    {
        [SerializeField] private ASSound param;
        [SerializeField, Tooltip("3D音源の場合、この場所からサウンドが再生される")] private Transform root;

        private protected ASSound Param => param;
        private protected Transform Root => root;

        private protected abstract void Init();

        private void Awake()
        {
            Init();
        }
    }
}
