namespace MyScripts.Runtime
{
    internal sealed class CharacterNameFacer : MonoBehaviour
    {
        [SerializeField] private new Camera camera;
        [SerializeField] private TextMeshPro[] characterNames;

        private void LateUpdate()
        {
            foreach (var name in characterNames)
            {
                if (name != null)
                {
                    Vector3 directionToCamera = camera.transform.position - name.transform.position;
                    directionToCamera.y = 0;
                    if (directionToCamera != Vector3.zero)
                        name.transform.rotation = Quaternion.LookRotation(-directionToCamera);
                }
            }
        }
    }
}