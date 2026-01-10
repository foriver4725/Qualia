using UnityEditor;
using UnityEngine;

namespace MyScripts.EditorExtension.Private
{
    internal static class SOSArranger
    {
        [MenuItem("Tools/SOS Arranger (Open Window)")]
        private static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<Window>();
            window.titleContent = new GUIContent("SOS Arranger");
        }

        private sealed class Window : EditorWindow
        {
            private GameObject landPrefab;
            private GameObject seaPrefab;
            private GameObject skyPrefab;

            private int landCount;
            private int seaCount;
            private int skyCount;

            private void OnGUI()
            {
                EditorGUILayout.LabelField("SOS Prefabs", EditorStyles.boldLabel);

                // SOS用のPrefabと配置個数を設定
                // シーンの GameObject は設定できないようにする
                using (new EditorGUILayout.HorizontalScope())
                {
                    landPrefab = (GameObject)EditorGUILayout.ObjectField("Land", landPrefab, typeof(GameObject), false);
                    landCount = EditorGUILayout.IntField("Count", landCount);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    seaPrefab = (GameObject)EditorGUILayout.ObjectField("Sea", seaPrefab, typeof(GameObject), false);
                    seaCount = EditorGUILayout.IntField("Count", seaCount);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    skyPrefab = (GameObject)EditorGUILayout.ObjectField("Sky", skyPrefab, typeof(GameObject), false);
                    skyCount = EditorGUILayout.IntField("Count", skyCount);
                }

                if (GUILayout.Button("配置実行"))
                {
                    // Prefab の null チェック
                    if (!landPrefab)
                    {
                        Debug.LogError("Land Prefab is not set.");
                        return;
                    }
                    if (!seaPrefab)
                    {
                        Debug.LogError("Sea Prefab is not set.");
                        return;
                    }
                    if (!skyPrefab)
                    {
                        Debug.LogError("Sky Prefab is not set.");
                        return;
                    }

                    // ルートを取得
                    GameObject root = GameObject.Find("SOSSignsRoot");
                    if (!root)
                    {
                        Debug.LogError("SOSSignsRoot GameObject not found in the scene.");
                        return;
                    }

                    // 配置
                    for (int i = 0; i < landCount; i++)
                    {
                        GameObject landInstance = (GameObject)PrefabUtility.InstantiatePrefab(landPrefab);
                        landInstance.transform.SetParent(root.transform);
                        landInstance.name = $"Land_{i}";
                    }
                    for (int i = 0; i < seaCount; i++)
                    {
                        GameObject seaInstance = (GameObject)PrefabUtility.InstantiatePrefab(seaPrefab);
                        seaInstance.transform.SetParent(root.transform);
                        seaInstance.name = $"Sea_{i}";
                    }
                    for (int i = 0; i < skyCount; i++)
                    {
                        GameObject skyInstance = (GameObject)PrefabUtility.InstantiatePrefab(skyPrefab);
                        skyInstance.transform.SetParent(root.transform);
                        skyInstance.name = $"Sky_{i}";
                    }
                }
            }
        }
    }
}
