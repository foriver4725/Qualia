using System;
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
            private GameObject landPrefab = null;
            private GameObject seaPrefab = null;
            private GameObject skyPrefab = null;

            private int landCount = 1;
            private int seaCount = 1;
            private int skyCount = 1;

            private GameObject root = null;
            private bool deleteRootChildren = false;

            private bool randomSeedOverride = false;
            private int randomSeed = 0;

            private void OnGUI()
            {
                // SOS用のPrefabと配置個数を設定
                CreatePrefabCountField(ref landPrefab, ref landCount, "Land");
                CreatePrefabCountField(ref seaPrefab, ref seaCount, "Sea");
                CreatePrefabCountField(ref skyPrefab, ref skyCount, "Sky");

                EditorGUILayout.Space();

                // ルートオブジェクト・削除オプションの設定
                root = (GameObject)EditorGUILayout.ObjectField("Root Object", root, typeof(GameObject), true);
                deleteRootChildren = EditorGUILayout.Toggle("Delete Existing Children", deleteRootChildren);

                EditorGUILayout.Space();

                // 乱数シードの設定
                randomSeedOverride = EditorGUILayout.Toggle("Override Random Seed", randomSeedOverride);
                using (new EditorGUI.DisabledScope(!randomSeedOverride))
                {
                    randomSeed = EditorGUILayout.IntField("Random Seed", randomSeed);
                }

                if (GUILayout.Button("配置実行"))
                {
                    // null チェック
                    if (!landPrefab) { Debug.LogError("Land Prefab is not set."); return; }
                    if (!seaPrefab) { Debug.LogError("Sea Prefab is not set."); return; }
                    if (!skyPrefab) { Debug.LogError("Sky Prefab is not set."); return; }
                    if (!root) { Debug.LogError("Root Object is not set."); return; }

                    // 乱数シードを設定
                    if (randomSeedOverride)
                    {
                        UnityEngine.Random.InitState(randomSeed);
                    }

                    // 指定されたなら、既存の子オブジェクトを削除
                    if (deleteRootChildren)
                    {
                        int childCount = root.transform.childCount;
                        for (int i = childCount - 1; i >= 0; i--)
                        {
                            Transform child = root.transform.GetChild(i);
                            DestroyImmediate(child.gameObject);
                        }
                    }

                    // 近すぎる場所には配置しないように、配置した座標を保存しておく
                    // デフォルト値は Vector3.zero なので、未使用の要素はそれで判定する
                    Vector3[] placedPositions = new Vector3[landCount + seaCount + skyCount];

                    // 配置
                    for (int i = 0; i < landCount; i++)
                    {
                        GameObject landInstance = (GameObject)PrefabUtility.InstantiatePrefab(landPrefab);
                        landInstance.transform.SetParent(root.transform);
                        landInstance.name = $"Land_{i}";
                        RandomlyArrange(landInstance, placedPositions);
                    }
                    for (int i = 0; i < seaCount; i++)
                    {
                        GameObject seaInstance = (GameObject)PrefabUtility.InstantiatePrefab(seaPrefab);
                        seaInstance.transform.SetParent(root.transform);
                        seaInstance.name = $"Sea_{i}";
                        RandomlyArrange(seaInstance, placedPositions);
                    }
                    for (int i = 0; i < skyCount; i++)
                    {
                        GameObject skyInstance = (GameObject)PrefabUtility.InstantiatePrefab(skyPrefab);
                        skyInstance.transform.SetParent(root.transform);
                        skyInstance.name = $"Sky_{i}";
                        RandomlyArrange(skyInstance, placedPositions);
                    }
                }
            }

            private static void CreatePrefabCountField(ref GameObject prefab, ref int count, string label)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    float labelWidth = EditorGUIUtility.labelWidth;
                    float fieldWidth = EditorGUIUtility.fieldWidth;

                    EditorGUIUtility.labelWidth = 60f;
                    EditorGUIUtility.fieldWidth = 120f;
                    // シーンの GameObject は設定できないようにする
                    prefab = (GameObject)EditorGUILayout.ObjectField(label, prefab, typeof(GameObject), false);

                    GUILayout.Space(10f);

                    EditorGUIUtility.labelWidth = 50f;
                    EditorGUIUtility.fieldWidth = 50f;
                    count = EditorGUILayout.IntSlider("Count", count, 0, 100);

                    EditorGUIUtility.labelWidth = labelWidth;
                    EditorGUIUtility.fieldWidth = fieldWidth;
                }
            }

            private static void RandomlyArrange(GameObject instance, Span<Vector3> placedPositions)
            {
                const float CenterX = -500f;
                const float CenterZ = 350f;
                const float MaxRange = 600.0f;

                const float MinDistance = 20.0f; // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
                const float MaxAttempts = 100; // 配置場所を探す最大試行回数
                const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

                // 円形範囲内のランダムな位置を計算 (X, Z)
                bool positionFound = false;
                float x = 0f;
                float z = 0f;
                for (int attempt = 0; attempt < MaxAttempts; attempt++)
                {
                    float r = UnityEngine.Random.Range(0f, MaxRange);
                    float theta = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                    x = CenterX + r * Mathf.Cos(theta);
                    z = CenterZ + r * Mathf.Sin(theta);

                    // 既に配置されたオブジェクトとの距離をチェック
                    bool tooClose = false;
                    foreach (var pos in placedPositions)
                    {
                        if (pos == Vector3.zero) break; // 未使用の要素に到達したら終了

                        float distanceSq = (new Vector2(x - pos.x, z - pos.z)).sqrMagnitude;
                        if (distanceSq < MinDistance * MinDistance)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    positionFound = !tooClose;
                }

                // 位置が見つからなかったので、Y=0に配置して終了
                if (!positionFound)
                {
                    Debug.LogWarning("Could not find a suitable position for arrangement.");
                    instance.transform.position = new Vector3(x, 0f, z);
                    return;
                }

                // 地表のY座標を算出
                // レイを打つ
                Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                {
                    // 地表が見つかった場合、そのY座標に配置
                    instance.transform.position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                }
                else
                {
                    // 地表が見つからなかった場合はY=0に配置
                    instance.transform.position = new Vector3(x, 0f, z);
                }
            }
        }
    }
}
