namespace MyScripts.Runtime
{
    using Group = SSOSAnimaArrangement.Group;

    internal sealed class SOSAnimaArranger : ASingletonMonoBehaviour<SOSAnimaArranger>
    {
        [SerializeField] private SSOSAnimaArrangement param;
        [SerializeField] private Transform sosRoot;
        [SerializeField] private Transform animaRoot;

        private readonly struct TerrainTreeInfo
        {
            internal readonly Vector3 Position { get; init; }
            internal readonly float Height { get; init; }
        }

        internal void ArrangeRandomly(int randomSeed)
        {
            // 乱数シードを設定
            using RandomSeedScope randomSeedScope = new RandomSeedScope(randomSeed);

            // テラインの全ツリーの情報を取得
            TerrainTreeInfo[] treeInfosBuffer = new TerrainTreeInfo[0xffff];
            int treeInfoCount = GetTerrainTreeInfos(treeInfosBuffer);
            ReadOnlySpan<TerrainTreeInfo> treeInfos = treeInfosBuffer.AsSpan(0, treeInfoCount);

            // 他種類の配置で使うので、配置した位置をメモっておく
            Span<Vector3> sosLandPositions = default;
            Span<Vector3> sosSeaPositions = default;

            for (int i = 0; i < (int)Group.Count; i++)
            {
                Group group = (Group)i;

                GameObject prefab = param.GetPrefab(group);
                int count = param.GetCount(group);
                Transform parent = GetParent(group);

                Vector3[] positions = new Vector3[count];
                if (group == Group.SOS_Land) CreateInstancePositions_SOS_Land(positions);
                else if (group == Group.SOS_Sea) CreateInstancePositions_SOS_Sea(positions);
                else if (group == Group.SOS_Sky) CreateInstancePositions_SOS_Sky(positions, treeInfos);
                else if (group == Group.Anima_Land) CreateInstancePositions_Anima_Land(positions, sosLandPositions);
                else if (group == Group.Anima_Sea) CreateInstancePositions_Anima_Sea(positions, sosSeaPositions);
                else if (group == Group.Anima_Sky) CreateInstancePositions_Anima_Sky(positions);
                else throw new ArgumentOutOfRangeException(nameof(group), group, null);

                foreach (Vector3 position in positions)
                {
                    _ = Instantiate(prefab, position, Quaternion.identity, parent);
                }

                // 座標をメモっておいて、この後の配置で活用してもらう
                if (group == Group.SOS_Land) sosLandPositions = positions;
                else if (group == Group.SOS_Sea) sosSeaPositions = positions;
            }
        }

        private Transform GetParent(Group group) => group switch
        {
            Group.SOS_Land or Group.SOS_Sea or Group.SOS_Sky => sosRoot,
            Group.Anima_Land or Group.Anima_Sea or Group.Anima_Sky => animaRoot,
            _ => throw new ArgumentOutOfRangeException(nameof(group), group, null),
        };

        /// <summary>
        /// 現在のテラインにおける、全ツリーの情報を取得する<br/>
        /// 引数に渡した戻り値用の配列は、十分なサイズであること<br/>
        /// 実際にいくつ見つかったかを返す<br/>
        /// </summary>
        private static int GetTerrainTreeInfos(Span<TerrainTreeInfo> outInfos)
        {
            int outInfoIndex = 0;

            foreach (var terrain in Terrain.activeTerrains)
            {
                TerrainData terrainData = terrain.terrainData;
                for (int i = 0; i < terrainData.treeInstanceCount; i++)
                {
                    TreeInstance treeInstance = terrainData.GetTreeInstance(i);
                    string prefabName = terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.name;

                    // プレハブの名前を見て、木でないならスキップ
                    {
                        bool isNotTree = true;
                        foreach (string treeName in SSOSAnimaArrangement.TreeNameHeightMap.Keys)
                        {
                            if (prefabName == treeName)
                            {
                                isNotTree = false;
                                break;
                            }
                        }

                        if (isNotTree)
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

                    outInfos[outInfoIndex++] = new TerrainTreeInfo
                    {
                        Position = treeRootPos,
                        Height = SSOSAnimaArrangement.TreeNameHeightMap[prefabName],
                    };
                }
            }

            return outInfoIndex;
        }

        private static void CreateInstancePositions_SOS_Land(Span<Vector3> outPositions)
        {
            const float CenterX = -500f;
            const float CenterZ = 350f;
            const float MaxRange = 600.0f;
            const float MinDistance = 20.0f;      // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
            const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

            // 失敗した場合は Vector3.zero にするため、あらかじめ埋めておく
            outPositions.Fill(Vector3.zero);

            for (int i = 0; i < outPositions.Length; i++)
            {
                // ランダムな位置を計算 (X, Z)
                // 極座標系でランダムに選ぶ
                float r = Random.Range(0f, MaxRange);
                float theta = Random.Range(0f, Mathf.PI * 2f);
                float x = CenterX + r * Mathf.Cos(theta);
                float z = CenterZ + r * Mathf.Sin(theta);

                // 既に配置されたオブジェクトとの距離をチェック
                {
                    bool tooClose = false;
                    foreach (var pos in outPositions)
                    {
                        // 未使用の要素に到達したら終了
                        if (pos == Vector3.zero)
                            break;

                        float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                        if (distanceSq < MinDistance * MinDistance)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose)
                        continue; // 近すぎるなら再試行
                }

                // 地表のY座標を算出
                // レイを打つ
                Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);
                if (!Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                    continue; // 地表が見つからなかった場合は再試行

                // 水上には配置できない
                string hitObjectName = hitInfo.collider.gameObject.name;
                if (hitObjectName == "WaterPlane")
                    continue; // 再試行

                // 配置成功
                Vector3 position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                outPositions[i] = position;
            }
        }

        private static void CreateInstancePositions_SOS_Sea(Span<Vector3> outPositions)
        {
            const float CenterX = -500f;
            const float CenterZ = 350f;
            const float MaxRange = 600.0f;
            const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

            for (int i = 0; i < outPositions.Length; i++)
            {
                // ランダムな位置を計算 (X, Z)
                // 極座標系でランダムに選ぶ
                float r = Random.Range(0f, MaxRange);
                float theta = Random.Range(0f, Mathf.PI * 2f);
                float x = CenterX + r * Mathf.Cos(theta);
                float z = CenterZ + r * Mathf.Sin(theta);

                // 地表のY座標を算出
                // レイを打つ
                Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);
                if (!Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                    continue; // 地表が見つからなかった場合は再試行

                // 水上にしか配置できない
                string hitObjectName = hitInfo.collider.gameObject.name;
                if (hitObjectName != "WaterPlane")
                    continue; // 再試行

                // 配置成功
                Vector3 position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                outPositions[i] = position;
            }
        }

        private static void CreateInstancePositions_SOS_Sky(Span<Vector3> outPositions,
            ReadOnlySpan<TerrainTreeInfo> treeInfos)
        {
            for (int i = 0; i < outPositions.Length; i++)
            {
                // テラインに存在するツリーの個数を超えてしまう
                if (i >= treeInfos.Length)
                    return; // 多すぎるので、ここで終わり

                // ランダムに、木の上の方に配置
                TerrainTreeInfo treeInfo = treeInfos[i];
                float height = Random.Range(treeInfo.Height * 0.5f, treeInfo.Height * 0.95f);
                Vector3 position = treeInfo.Position + Vector3.up * height;

                // 配置成功
                outPositions[i] = position;
            }
        }

        private static void CreateInstancePositions_Anima_Land(Span<Vector3> outPositions,
            Span<Vector3> sosLandPositions)
        {
            const float CenterX = -500f;
            const float CenterZ = 350f;
            const float MaxRange = 600.0f;
            const float MinDistance = 30.0f;              // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
            const float MinDistanceToSOS = 5.0f;          // SOSオブジェクトとは、最低どれくらい離すか (m. XZ平面距離)
            const float MaxDistanceToSOS = 50.0f;         // SOSオブジェクトとは、最大どれくらい離すか (m. XZ平面距離)
            const float HeightAboveGround = 0.1f;         // 地表からどのくらい上に配置するか (m)
            const float WaterPlacementProbability = 0.1f; // 水上に配置する確率

            for (int i = 0; i < outPositions.Length; i++)
            {
                // ランダムな位置を計算 (X, Z)
                // 極座標系でランダムに選ぶ
                float r = Random.Range(0f, MaxRange);
                float theta = Random.Range(0f, Mathf.PI * 2f);
                float x = CenterX + r * Mathf.Cos(theta);
                float z = CenterZ + r * Mathf.Sin(theta);

                // 既に配置されたオブジェクトとの距離をチェック
                {
                    bool tooClose = false;
                    foreach (var pos in outPositions)
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
                }

                // 対応するSOSオブジェクトとの距離をチェック
                {
                    bool tooCloseToSOS = false;
                    foreach (var pos in sosLandPositions)
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
                }
                {
                    bool tooFarFromSOS = true;
                    foreach (var pos in sosLandPositions)
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
                }

                // 地表のY座標を算出
                // レイを打つ
                Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);
                if (!Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                    continue; // 地表が見つからなかった場合は再試行

                // 水上には、滅多に配置できない
                string hitObjectName = hitInfo.collider.gameObject.name;
                if (hitObjectName == "WaterPlane")
                {
                    if (Random.value > WaterPlacementProbability)
                        continue; // 再試行
                }

                // 配置成功
                Vector3 position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                outPositions[i] = position;
            }
        }

        private static void CreateInstancePositions_Anima_Sea(Span<Vector3> outPositions,
            Span<Vector3> sosSeaPositions)
        {
            const float CenterX = -500f;
            const float CenterZ = 350f;
            const float MaxRange = 600.0f;
            const float MinDistance = 20.0f;             // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
            const float MinDistanceToSOS = 5.0f;         // SOSオブジェクトとは、最低どれくらい離すか (m. XZ平面距離)
            const float MaxDistanceToSOS = 150.0f;       // SOSオブジェクトとは、最大どれくらい離すか (m. XZ平面距離)
            const float HeightAboveGround = 0.1f;        // 地表からどのくらい上に配置するか (m)
            const float LandPlacementProbability = 0.5f; // 水上以外の場所に配置する確率

            for (int i = 0; i < outPositions.Length; i++)
            {
                // ランダムな位置を計算 (X, Z)
                // 極座標系でランダムに選ぶ
                float r = Random.Range(0f, MaxRange);
                float theta = Random.Range(0f, Mathf.PI * 2f);
                float x = CenterX + r * Mathf.Cos(theta);
                float z = CenterZ + r * Mathf.Sin(theta);

                // 地表のY座標を算出
                // レイを打つ
                Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);
                if (!Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                    continue; // 地表が見つからなかった場合は再試行

                // 既に配置されたオブジェクトとの距離をチェック
                {
                    bool tooClose = false;
                    foreach (var pos in outPositions)
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
                }

                // 対応するSOSオブジェクトとの距離をチェック
                {
                    bool tooCloseToSOS = false;
                    foreach (var pos in sosSeaPositions)
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
                }
                {
                    bool tooFarFromSOS = true;
                    foreach (var pos in sosSeaPositions)
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
                }

                // 水上以外には、それほど配置できない
                string hitObjectName = hitInfo.collider.gameObject.name;
                if (hitObjectName != "WaterPlane")
                {
                    if (Random.value > LandPlacementProbability)
                        continue; // 再試行
                }

                // 配置成功
                Vector3 position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                outPositions[i] = position;
            }
        }

        private static void CreateInstancePositions_Anima_Sky(Span<Vector3> outPositions)
        {
            // 場所を問わず、まばらに配置する

            const float CenterX = -500f;
            const float CenterZ = 350f;
            const float MaxRange = 600.0f;
            const float MinDistance = 40.0f;      // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
            const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

            for (int i = 0; i < outPositions.Length; i++)
            {
                // ランダムな位置を計算 (X, Z)
                // 極座標系でランダムに選ぶ
                float r = Random.Range(0f, MaxRange);
                float theta = Random.Range(0f, Mathf.PI * 2f);
                float x = CenterX + r * Mathf.Cos(theta);
                float z = CenterZ + r * Mathf.Sin(theta);

                // 既に配置されたオブジェクトとの距離をチェック
                {
                    bool tooClose = false;
                    foreach (var pos in outPositions)
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
                }

                // 地表のY座標を算出
                // レイを打つ
                Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);
                if (!Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                    continue; // 地表が見つからなかった場合は再試行

                // 配置成功
                Vector3 position = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
                outPositions[i] = position;
            }
        }
    }
}