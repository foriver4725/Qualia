using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyScripts.EditorExtension.Private
{
    internal static class SOSAnimaArranger
    {
        [MenuItem("Tools/SOS and Anima Arranger (Open Window)")]
        private static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<Window>();
            window.titleContent = new GUIContent("SOS & Anima Arranger");
        }

        private sealed class Window : EditorWindow
        {
            private enum Group : byte
            {
                Land,
                Sea,
                Sky,
            }

            private static readonly Dictionary<string, float> TreeNameHeightMap = new Dictionary<string, float>()
            {
                { "Conifer", 29.0f },
                { "Cypress", 10.8f },
                { "Pine_A", 29.0f },
                { "Pine_B", 28.0f },
                { "Pine_C", 20.3f },
                { "Pine_D", 12.7f },
            };

            private GameObject sosLandPrefab = null;
            private GameObject sosSeaPrefab = null;
            private GameObject sosSkyPrefab = null;

            private int sosLandCount = 1;
            private int sosSeaCount = 1;
            private int sosSkyCount = 1;

            private GameObject animaLandPrefab = null;
            private GameObject animaSeaPrefab = null;
            private GameObject animaSkyPrefab = null;

            private int animaLandCount = 1;
            private int animaSeaCount = 1;
            private int animaSkyCount = 1;

            private GameObject sosRoot = null;
            private GameObject animaRoot = null;
            private bool deleteRootChildren = false;

            private bool randomSeedOverride = false;
            private int randomSeed = 0;

            private void OnGUI()
            {
                {
                    EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);

                    EditorGUILayout.Space();

                    // SOS用のPrefabと配置個数を設定
                    CreatePrefabCountField(ref sosLandPrefab, ref sosLandCount, "SOS - Land");
                    CreatePrefabCountField(ref sosSeaPrefab, ref sosSeaCount, "SOS - Sea");
                    CreatePrefabCountField(ref sosSkyPrefab, ref sosSkyCount, "SOS - Sky");

                    EditorGUILayout.Space();

                    // アニマ用のPrefabと配置個数を設定
                    CreatePrefabCountField(ref animaLandPrefab, ref animaLandCount, "Anima - Land");
                    CreatePrefabCountField(ref animaSeaPrefab, ref animaSeaCount, "Anima - Sea");
                    CreatePrefabCountField(ref animaSkyPrefab, ref animaSkyCount, "Anima - Sky");

                    EditorGUILayout.Space();
                }

                {
                    EditorGUILayout.LabelField("Arrangement Settings", EditorStyles.boldLabel);

                    EditorGUILayout.Space();

                    // ルートオブジェクト・削除オプションの設定
                    sosRoot = (GameObject)EditorGUILayout.ObjectField("SOS - Root Object", sosRoot, typeof(GameObject), true);
                    animaRoot = (GameObject)EditorGUILayout.ObjectField("Anima - Root Object", animaRoot, typeof(GameObject), true);
                    deleteRootChildren = EditorGUILayout.Toggle("Delete Existing Children", deleteRootChildren);

                    EditorGUILayout.Space();
                }

                {
                    EditorGUILayout.LabelField("Randomization Settings", EditorStyles.boldLabel);

                    EditorGUILayout.Space();

                    // 乱数シードの設定
                    randomSeedOverride = EditorGUILayout.Toggle("Override Random Seed", randomSeedOverride);
                    using (new EditorGUI.DisabledScope(!randomSeedOverride))
                    {
                        randomSeed = EditorGUILayout.IntField("Random Seed", randomSeed);
                    }

                    EditorGUILayout.Space();
                }

                // 木の座標を全取得 (中心座標, 高さ)
                List<(Vector3 Position, float Height)> treeTransforms = new List<(Vector3, float)>(4096);
                {
                    Terrain[] terrains = Terrain.activeTerrains;
                    foreach (var terrain in terrains)
                    {
                        TerrainData terrainData = terrain.terrainData;
                        int treeInstanceCount = terrainData.treeInstanceCount;
                        for (int i = 0; i < treeInstanceCount; i++)
                        {
                            TreeInstance treeInstance = terrainData.GetTreeInstance(i);
                            string prefabName = terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.name;

                            // プレハブの名前を見て、木でないならスキップ
                            {
                                bool isTree = false;
                                foreach (string name in TreeNameHeightMap.Keys)
                                {
                                    if (prefabName == name)
                                    {
                                        isTree = true;
                                        break;
                                    }
                                }
                                if (!isTree)
                                    continue;
                            }

                            Vector3 localXZ = new Vector3(
                                treeInstance.position.x * terrainData.size.x,
                                0f,
                                treeInstance.position.z * terrainData.size.z
                            );
                            Vector3 worldXZ = terrain.transform.TransformPoint(localXZ);
                            // 地表Yを取り直す
                            float groundY = terrain.SampleHeight(worldXZ) + terrain.transform.position.y;
                            Vector3 treeRootPos = new Vector3(worldXZ.x, groundY, worldXZ.z);

                            treeTransforms.Add((treeRootPos, TreeNameHeightMap[prefabName]));
                        }
                    }
                }
                (Vector3, float)[] treeTransformsArray = treeTransforms.ToArray();
                // シャッフルする (フィッシャー・イェーツのアルゴリズム)
                {
                    int n = treeTransformsArray.Length;
                    for (int i = 0; i < n - 1; i++)
                    {
                        int j = UnityEngine.Random.Range(i, n);
                        (treeTransformsArray[i], treeTransformsArray[j]) = (treeTransformsArray[j], treeTransformsArray[i]);
                    }
                }

                if (GUILayout.Button("Execute SOS Arrangement"))
                {
                    // null チェック
                    if (!sosLandPrefab) { Debug.LogError("Land Prefab is not set."); return; }
                    if (!sosSeaPrefab) { Debug.LogError("Sea Prefab is not set."); return; }
                    if (!sosSkyPrefab) { Debug.LogError("Sky Prefab is not set."); return; }
                    if (!sosRoot) { Debug.LogError("Root Object is not set."); return; }

                    // 乱数シードを設定
                    if (randomSeedOverride)
                    {
                        UnityEngine.Random.InitState(randomSeed);
                    }

                    // 指定されたなら、既存の子オブジェクトを削除
                    if (deleteRootChildren)
                    {
                        int childCount = sosRoot.transform.childCount;
                        for (int i = childCount - 1; i >= 0; i--)
                        {
                            Transform child = sosRoot.transform.GetChild(i);
                            DestroyImmediate(child.gameObject);
                        }
                    }

                    // 近すぎる場所には配置しないように、配置した座標を保存しておく
                    // デフォルト値は Vector3.zero なので、未使用の要素はそれで判定する
                    // 種類ごとに作成
                    Vector3[] placedPositionsLand = new Vector3[sosLandCount];
                    Vector3[] placedPositionsSea = new Vector3[sosSeaCount];
                    Vector3[] placedPositionsSky = new Vector3[sosSkyCount];

                    // 配置
                    for (int i = 0; i < sosLandCount; i++)
                    {
                        GameObject landInstance = (GameObject)PrefabUtility.InstantiatePrefab(sosLandPrefab);
                        landInstance.transform.SetParent(sosRoot.transform);
                        landInstance.name = $"Land_{i}";
                        RandomlyArrange(landInstance, i, placedPositionsLand, treeTransformsArray, Group.Land);
                    }
                    for (int i = 0; i < sosSeaCount; i++)
                    {
                        GameObject seaInstance = (GameObject)PrefabUtility.InstantiatePrefab(sosSeaPrefab);
                        seaInstance.transform.SetParent(sosRoot.transform);
                        seaInstance.name = $"Sea_{i}";
                        RandomlyArrange(seaInstance, i, placedPositionsSea, treeTransformsArray, Group.Sea);
                    }
                    for (int i = 0; i < sosSkyCount; i++)
                    {
                        GameObject skyInstance = (GameObject)PrefabUtility.InstantiatePrefab(sosSkyPrefab);
                        skyInstance.transform.SetParent(sosRoot.transform);
                        skyInstance.name = $"Sky_{i}";
                        RandomlyArrange(skyInstance, i, placedPositionsSky, treeTransformsArray, Group.Sky);
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

                    EditorGUIUtility.labelWidth = 120f;
                    EditorGUIUtility.fieldWidth = 60f;
                    // シーンの GameObject は設定できないようにする
                    prefab = (GameObject)EditorGUILayout.ObjectField(label, prefab, typeof(GameObject), false);

                    GUILayout.Space(10f);

                    EditorGUIUtility.labelWidth = 50f;
                    EditorGUIUtility.fieldWidth = 80f;
                    count = EditorGUILayout.IntSlider("Count", count, 0, 1000);

                    EditorGUIUtility.labelWidth = labelWidth;
                    EditorGUIUtility.fieldWidth = fieldWidth;
                }
            }

            private static void RandomlyArrange(
                GameObject instance, int instanceId, Span<Vector3> placedPositions, ReadOnlySpan<(Vector3, float)> treeTransforms, Group type)
            {
                const float MaxAttempts = 100; // 配置場所を探す最大試行回数

                for (int attempt = 0; attempt < MaxAttempts; attempt++)
                {
                    if (type == Group.Land)
                    {
                        const float CenterX = -500f;
                        const float CenterZ = 350f;
                        const float MaxRange = 600.0f;
                        const float MinDistance = 20.0f; // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
                        const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

                        // ランダムな位置を計算 (X, Z)
                        // 極座標系でランダムに選ぶ
                        float r = UnityEngine.Random.Range(0f, MaxRange);
                        float theta = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                        float x = CenterX + r * Mathf.Cos(theta);
                        float z = CenterZ + r * Mathf.Sin(theta);

                        // 既に配置されたオブジェクトとの距離をチェック
                        bool tooClose = false;
                        foreach (var pos in placedPositions)
                        {
                            if (pos == Vector3.zero)
                                break; // 未使用の要素に到達したら終了

                            float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                            if (distanceSq < MinDistance * MinDistance)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                        if (tooClose)
                            continue; // 近すぎるなら再試行

                        // 地表のY座標を算出
                        // レイを打つ
                        Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);
                        bool raycastHit = Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity);
                        if (!raycastHit)
                            continue; // 地表が見つからなかった場合は再試行

                        // 水上には配置できない
                        string hitObjectName = hitInfo.collider.gameObject.name;
                        if (hitObjectName == "WaterPlane")
                            continue; // 再試行

                        // 配置成功
                        Vector3 position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                        instance.transform.position = position;
                        placedPositions[instanceId] = position;
                    }
                    else if (type == Group.Sea)
                    {
                        const float CenterX = -500f;
                        const float CenterZ = 350f;
                        const float MaxRange = 600.0f;
                        const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

                        // ランダムな位置を計算 (X, Z)
                        // 極座標系でランダムに選ぶ
                        float r = UnityEngine.Random.Range(0f, MaxRange);
                        float theta = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                        float x = CenterX + r * Mathf.Cos(theta);
                        float z = CenterZ + r * Mathf.Sin(theta);

                        // 地表のY座標を算出
                        // レイを打つ
                        Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);
                        bool raycastHit = Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity);
                        if (!raycastHit)
                            continue; // 地表が見つからなかった場合は再試行

                        // 水上にしか配置できない
                        string hitObjectName = hitInfo.collider.gameObject.name;
                        if (hitObjectName != "WaterPlane")
                            continue; // 再試行

                        // 配置成功
                        Vector3 position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                        instance.transform.position = position;
                        placedPositions[instanceId] = position;
                    }
                    else // type == SOSType.Sky
                    {
                        // 木のトランスフォームを取得
                        if (instanceId >= treeTransforms.Length) // インスタンスIDが、木の配列のサイズを超えた
                            continue; // 再試行 (本当はすぐ return するべきだが、処理を共通化するためにこうしている)
                        (Vector3 treePos, float treeHeight) = treeTransforms[instanceId];

                        // ランダムに、木の上の方に配置
                        Vector3 position = treePos + Vector3.up * UnityEngine.Random.Range(treeHeight * 0.5f, treeHeight * 0.95f);

                        // 配置成功
                        instance.transform.position = position;
                        placedPositions[instanceId] = position;
                    }

                    return;
                }

                Debug.LogWarning($"Failed to arrange {instance.name} after {MaxAttempts} attempts.");
                return;
            }
        }
    }
}
