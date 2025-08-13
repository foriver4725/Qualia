namespace MyScripts.Common
{
    internal abstract class ASingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance = null;
        internal static T Instance
        {
            get
            {
                if (instance == null)
                {
                    T[] instances = FindObjectsByType<T>(FindObjectsSortMode.None);

                    if (instances == null || instances.Length <= 0)
                    {
                        $"{typeof(T).Name} not found".LogError();

                        instance = null;
                    }
                    else if (instances.Length > 1)
                    {
                        $"Multiple instances of {typeof(T).Name} found. Using the first instance and destroying others.".LogWarning();

                        instance = instances[0];
                        for (int i = 1; i < instances.Length; i++)
                        {
                            Destroy(instances[i].gameObject);
                        }
                    }
                    else
                    {
                        instance = instances[0];
                    }
                }

                return instance;
            }
        }
    }
}