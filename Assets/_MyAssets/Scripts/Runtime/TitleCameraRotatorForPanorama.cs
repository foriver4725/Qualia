namespace MyScripts.Runtime
{
    internal sealed class TitleCameraRotatorForPanorama : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField, Range(0.0f, 360.0f)] private float rotationSpeed = 5.0f; // [deg/sec]

        private void Update()
        {
            targetCamera.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
