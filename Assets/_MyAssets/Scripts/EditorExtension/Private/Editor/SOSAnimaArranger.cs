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
                SOS_Land,
                SOS_Sea,
                SOS_Sky,
                Anima_Land,
                Anima_Sea,
                Anima_Sky,
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

            private readonly Dictionary<Group, Vector3[]> placedPositionsMap = new Dictionary<Group, Vector3[]>()
            {
                { Group.SOS_Land, null },
                { Group.SOS_Sea, null },
                { Group.SOS_Sky, null },
                { Group.Anima_Land, null },
                { Group.Anima_Sea, null },
                { Group.Anima_Sky, null },
            };

            private (Vector3 Position, float Height)[] treeTransforms = null;

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

                if (GUILayout.Button("Execute SOS & Anima Arrangement"))
                {
                    // null チェック
                    if (!sosLandPrefab) { Debug.LogError("Land Prefab is not set."); return; }
                    if (!sosSeaPrefab) { Debug.LogError("Sea Prefab is not set."); return; }
                    if (!sosSkyPrefab) { Debug.LogError("Sky Prefab is not set."); return; }
                    if (!animaLandPrefab) { Debug.LogError("Land Prefab is not set."); return; }
                    if (!animaSeaPrefab) { Debug.LogError("Sea Prefab is not set."); return; }
                    if (!animaSkyPrefab) { Debug.LogError("Sky Prefab is not set."); return; }
                    if (!sosRoot) { Debug.LogError("Root Object is not set."); return; }
                    if (!animaRoot) { Debug.LogError("Root Object is not set."); return; }

                    // Undo 登録開始 (Ctrl+Z でここまで戻せるようにする)
                    Undo.IncrementCurrentGroup();
                    int undoGroup = Undo.GetCurrentGroup();
                    Undo.SetCurrentGroupName("SOS & Anima Arrangement");

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
                    placedPositionsMap[Group.SOS_Land] = new Vector3[sosLandCount];
                    placedPositionsMap[Group.SOS_Sea] = new Vector3[sosSeaCount];
                    placedPositionsMap[Group.SOS_Sky] = new Vector3[sosSkyCount];
                    placedPositionsMap[Group.Anima_Land] = new Vector3[animaLandCount];
                    placedPositionsMap[Group.Anima_Sea] = new Vector3[animaSeaCount];
                    placedPositionsMap[Group.Anima_Sky] = new Vector3[animaSkyCount];

                    // 木の座標を全取得 (中心座標, 高さ)
                    List<(Vector3 Position, float Height)> treeTransformsList = new(4096);
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

                                treeTransformsList.Add((treeRootPos, TreeNameHeightMap[prefabName]));
                            }
                        }
                    }
                    treeTransforms = treeTransformsList.ToArray();
                    // シャッフルする (フィッシャー・イェーツのアルゴリズム)
                    {
                        int n = treeTransforms.Length;
                        for (int i = 0; i < n - 1; i++)
                        {
                            int j = UnityEngine.Random.Range(i, n);
                            (treeTransforms[i], treeTransforms[j]) = (treeTransforms[j], treeTransforms[i]);
                        }
                    }

                    // 配置
                    Arrange(Group.SOS_Land);
                    Arrange(Group.SOS_Sea);
                    Arrange(Group.SOS_Sky);
                    Arrange(Group.Anima_Land);
                    Arrange(Group.Anima_Sea);
                    Arrange(Group.Anima_Sky);

                    // Undo 登録終了 (ここまでの変更を1操作としてまとめる)
                    Undo.CollapseUndoOperations(undoGroup);

                    // シーンへの変更を差分として出すようにする
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                        UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                    );

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

            private void Arrange(Group group)
            {
                GameObject prefab = group switch
                {
                    Group.SOS_Land => sosLandPrefab,
                    Group.SOS_Sea => sosSeaPrefab,
                    Group.SOS_Sky => sosSkyPrefab,
                    Group.Anima_Land => animaLandPrefab,
                    Group.Anima_Sea => animaSeaPrefab,
                    Group.Anima_Sky => animaSkyPrefab,
                    _ => throw new ArgumentOutOfRangeException(nameof(group), group, null),
                };

                Transform parent = group switch
                {
                    Group.SOS_Land or Group.SOS_Sea or Group.SOS_Sky => sosRoot.transform,
                    Group.Anima_Land or Group.Anima_Sea or Group.Anima_Sky => animaRoot.transform,
                    _ => throw new ArgumentOutOfRangeException(nameof(group), group, null),
                };

                int count = group switch
                {
                    Group.SOS_Land => sosLandCount,
                    Group.SOS_Sea => sosSeaCount,
                    Group.SOS_Sky => sosSkyCount,
                    Group.Anima_Land => animaLandCount,
                    Group.Anima_Sea => animaSeaCount,
                    Group.Anima_Sky => animaSkyCount,
                    _ => throw new ArgumentOutOfRangeException(nameof(group), group, null),
                };

                for (int i = 0; i < count; i++)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    instance.transform.SetParent(parent);
                    instance.name = $"{group}_{i}";
                    RandomlyArrange(instance, i, placedPositionsMap, treeTransforms, group);
                }
            }

            private static void RandomlyArrange(
                GameObject instance, int instanceId,
                IReadOnlyDictionary<Group, Vector3[]> placedPositionsMap, // Vector3[] は中で書き換える
                ReadOnlySpan<(Vector3, float)> treeTransforms,
                Group group
            )
            {
                const float MaxAttempts = 100; // 配置場所を探す最大試行回数

                for (int attempt = 0; attempt < MaxAttempts; attempt++)
                {
                    if (group == Group.SOS_Land)
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
                        foreach (var pos in placedPositionsMap[group])
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
                        placedPositionsMap[group][instanceId] = position;
                    }
                    else if (group == Group.SOS_Sea)
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
                        placedPositionsMap[group][instanceId] = position;
                    }
                    else if (group == Group.SOS_Sky)
                    {
                        // 木のトランスフォームを取得
                        if (instanceId >= treeTransforms.Length) // インスタンスIDが、木の配列のサイズを超えた
                            continue; // 再試行 (本当はすぐ return するべきだが、処理を共通化するためにこうしている)
                        (Vector3 treePos, float treeHeight) = treeTransforms[instanceId];

                        // ランダムに、木の上の方に配置
                        float height = UnityEngine.Random.Range(treeHeight * 0.5f, treeHeight * 0.95f);
                        Vector3 position = treePos + Vector3.up * height;

                        // 配置成功
                        instance.transform.position = position;
                        placedPositionsMap[group][instanceId] = position;
                    }
                    else if (group == Group.Anima_Land)
                    {
                        const float CenterX = -500f;
                        const float CenterZ = 350f;
                        const float MaxRange = 600.0f;
                        const float MinDistance = 30.0f; // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
                        const float MinDistanceToSOS = 5.0f; // SOSオブジェクトとは、最低どれくらい離すか (m. XZ平面距離)
                        const float MaxDistanceToSOS = 50.0f; // SOSオブジェクトとは、最大どれくらい離すか (m. XZ平面距離)
                        const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)
                        const float WaterPlacementProbability = 0.1f; // 水上に配置する確率

                        // ランダムな位置を計算 (X, Z)
                        // 極座標系でランダムに選ぶ
                        float r = UnityEngine.Random.Range(0f, MaxRange);
                        float theta = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                        float x = CenterX + r * Mathf.Cos(theta);
                        float z = CenterZ + r * Mathf.Sin(theta);

                        // 既に配置されたオブジェクトとの距離をチェック
                        bool tooClose = false;
                        foreach (var pos in placedPositionsMap[group])
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

                        // 対応するSOSオブジェクトとの距離をチェック
                        bool tooCloseToSOS = false;
                        foreach (var pos in placedPositionsMap[Group.SOS_Land])
                        {
                            if (pos == Vector3.zero)
                                break; // 未使用の要素に到達したら終了

                            float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                            if (distanceSq < MinDistanceToSOS * MinDistanceToSOS)
                            {
                                tooCloseToSOS = true;
                                break;
                            }
                        }
                        if (tooCloseToSOS)
                            continue; // 近すぎるなら再試行
                        bool tooFarFromSOS = true;
                        foreach (var pos in placedPositionsMap[Group.SOS_Land])
                        {
                            if (pos == Vector3.zero)
                                break; // 未使用の要素に到達したら終了

                            float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                            if (distanceSq <= MaxDistanceToSOS * MaxDistanceToSOS)
                            {
                                tooFarFromSOS = false;
                                break;
                            }
                        }
                        if (tooFarFromSOS)
                            continue; // 離れすぎているなら再試行

                        // 地表のY座標を算出
                        // レイを打つ
                        Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);
                        bool raycastHit = Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity);
                        if (!raycastHit)
                            continue; // 地表が見つからなかった場合は再試行

                        // 水上には、滅多に配置できない
                        string hitObjectName = hitInfo.collider.gameObject.name;
                        if (hitObjectName == "WaterPlane")
                        {
                            if (UnityEngine.Random.value > WaterPlacementProbability)
                                continue; // 再試行
                        }

                        // 配置成功
                        Vector3 position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                        instance.transform.position = position;
                        placedPositionsMap[group][instanceId] = position;
                    }
                    else if (group == Group.Anima_Sea)
                    {
                        const float CenterX = -500f;
                        const float CenterZ = 350f;
                        const float MaxRange = 600.0f;
                        const float MinDistance = 20.0f; // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
                        const float MinDistanceToSOS = 5.0f; // SOSオブジェクトとは、最低どれくらい離すか (m. XZ平面距離)
                        const float MaxDistanceToSOS = 150.0f; // SOSオブジェクトとは、最大どれくらい離すか (m. XZ平面距離)
                        const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)
                        const float LandPlacementProbability = 0.5f; // 水上以外の場所に配置する確率

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

                        // 既に配置されたオブジェクトとの距離をチェック
                        bool tooClose = false;
                        foreach (var pos in placedPositionsMap[group])
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

                        // 対応するSOSオブジェクトとの距離をチェック
                        bool tooCloseToSOS = false;
                        foreach (var pos in placedPositionsMap[Group.SOS_Sea])
                        {
                            if (pos == Vector3.zero)
                                break; // 未使用の要素に到達したら終了

                            float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                            if (distanceSq < MinDistanceToSOS * MinDistanceToSOS)
                            {
                                tooCloseToSOS = true;
                                break;
                            }
                        }
                        if (tooCloseToSOS)
                            continue; // 近すぎるなら再試行
                        bool tooFarFromSOS = true;
                        foreach (var pos in placedPositionsMap[Group.SOS_Sea])
                        {
                            if (pos == Vector3.zero)
                                break; // 未使用の要素に到達したら終了

                            float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                            if (distanceSq <= MaxDistanceToSOS * MaxDistanceToSOS)
                            {
                                tooFarFromSOS = false;
                                break;
                            }
                        }
                        if (tooFarFromSOS)
                            continue; // 離れすぎているなら再試行

                        // 水上以外には、それほど配置できない
                        string hitObjectName = hitInfo.collider.gameObject.name;
                        if (hitObjectName != "WaterPlane")
                        {
                            if (UnityEngine.Random.value > LandPlacementProbability)
                                continue; // 再試行
                        }

                        // 配置成功
                        Vector3 position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                        instance.transform.position = position;
                        placedPositionsMap[group][instanceId] = position;
                    }
                    else if (group == Group.Anima_Sky)
                    {
                        // 場所を問わず、まばらに配置する

                        const float CenterX = -500f;
                        const float CenterZ = 350f;
                        const float MaxRange = 600.0f;
                        const float MinDistance = 40.0f; // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
                        const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

                        // ランダムな位置を計算 (X, Z)
                        // 極座標系でランダムに選ぶ
                        float r = UnityEngine.Random.Range(0f, MaxRange);
                        float theta = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                        float x = CenterX + r * Mathf.Cos(theta);
                        float z = CenterZ + r * Mathf.Sin(theta);

                        // 既に配置されたオブジェクトとの距離をチェック
                        bool tooClose = false;
                        foreach (var pos in placedPositionsMap[group])
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

                        // 配置成功
                        Vector3 position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                        instance.transform.position = position;
                        placedPositionsMap[group][instanceId] = position;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(group), group, null);
                    }

                    return;
                }

                Debug.LogWarning($"Failed to arrange {instance.name} after {MaxAttempts} attempts.");
                return;
            }
        }
    }
}
