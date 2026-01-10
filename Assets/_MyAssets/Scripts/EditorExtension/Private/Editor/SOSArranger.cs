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
            private enum SOSType : byte
            {
                Land,
                Sea,
                Sky,
            }

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
                        RandomlyArrange(landInstance, placedPositions, SOSType.Land);
                    }
                    for (int i = 0; i < seaCount; i++)
                    {
                        GameObject seaInstance = (GameObject)PrefabUtility.InstantiatePrefab(seaPrefab);
                        seaInstance.transform.SetParent(root.transform);
                        seaInstance.name = $"Sea_{i}";
                        RandomlyArrange(seaInstance, placedPositions, SOSType.Sea);
                    }
                    for (int i = 0; i < skyCount; i++)
                    {
                        GameObject skyInstance = (GameObject)PrefabUtility.InstantiatePrefab(skyPrefab);
                        skyInstance.transform.SetParent(root.transform);
                        skyInstance.name = $"Sky_{i}";
                        RandomlyArrange(skyInstance, placedPositions, SOSType.Sky);
                    }

                    Debug.Log("SOS Arrangement Completed.");
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

            private static void RandomlyArrange(GameObject instance, Span<Vector3> placedPositions, SOSType type)
            {
                const float CenterX = -500f;
                const float CenterZ = 350f;
                const float MaxRange = 600.0f;

                const float MinDistanceIfLand = 20.0f; // Land のみ : 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
                const float MaxAttempts = 100; // 配置場所を探す最大試行回数
                const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

                for (int attempt = 0; attempt < MaxAttempts; attempt++)
                {
                    // ランダムな位置を計算 (X, Z)
                    // 極座標系でランダムに選ぶ
                    float r = UnityEngine.Random.Range(0f, MaxRange);
                    float theta = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                    float x = CenterX + r * Mathf.Cos(theta);
                    float z = CenterZ + r * Mathf.Sin(theta);

                    // Land のみ : 既に配置されたオブジェクトとの距離をチェック
                    if (type == SOSType.Land)
                    {
                        bool tooClose = false;
                        foreach (var pos in placedPositions)
                        {
                            if (pos == Vector3.zero) break; // 未使用の要素に到達したら終了

                            float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                            if (distanceSq < MinDistanceIfLand * MinDistanceIfLand)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                        if (tooClose) continue; // 近すぎるなら再試行
                    }

                    // 地表のY座標を算出
                    // レイを打つ
                    Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);
                    bool raycastHit = Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity);
                    if (!raycastHit)
                    {
                        // 地表が見つからなかった場合は再試行
                        continue;
                    }

                    // 相手の名前を取得
                    string hitObjectName = hitInfo.collider.gameObject.name;

                    // Land は、水上・木の上には配置できない
                    // TODO: 木の上の判定は未実装
                    if (type == SOSType.Land)
                    {
                        if (hitObjectName == "WaterPlane")
                        {
                            // 再試行
                            continue;
                        }
                    }

                    // Sea は、水上にしか配置できない
                    if (type == SOSType.Sea)
                    {
                        if (hitObjectName != "WaterPlane")
                        {
                            // 再試行
                            continue;
                        }
                    }

                    // Sky は、木の上にしか配置できない
                    // TODO: 木の上の判定は未実装
                    if (type == SOSType.Sky)
                    {
                    }

                    // 配置成功
                    instance.transform.position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);

                    // 配置した位置を記録
                    for (int i = 0; i < placedPositions.Length; i++)
                    {
                        if (placedPositions[i] == Vector3.zero)
                        {
                            placedPositions[i] = instance.transform.position;
                            break;
                        }
                    }

                    return;
                }

                Debug.LogWarning($"Failed to arrange {instance.name} after {MaxAttempts} attempts.");
                return;
            }
        }
    }
}
