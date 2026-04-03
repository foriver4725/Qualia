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

                // 座標を生成
                Vector3[] positions = new Vector3[count];
                for (int j = 0; j < count; i++)
                {
                    // 上限に達するまで、配置できる座標を探し続ける
                    int attemptIndex = 0;
                    while (attemptIndex < param.PositionCreateMaxAttempts)
                    {
                        ReadOnlySpan<Vector3> createdPositions = positions.AsSpan(0, j);

                        Vector3 newPosition;
                        bool success = group switch
                        {
                            Group.SOS_Land => CreateNewPosition_SOS_Land(createdPositions, out newPosition),
                            Group.SOS_Sea  => CreateNewPosition_SOS_Sea(createdPositions, out newPosition),
                            Group.SOS_Sky  => CreateNewPosition_SOS_Sky(out newPosition, treeInfos[j]),
                            Group.Anima_Land => CreateNewPosition_Anima_Land(createdPositions, out newPosition,
                                sosLandPositions),
                            Group.Anima_Sea => CreateNewPosition_Anima_Sea(createdPositions, out newPosition,
                                sosSeaPositions),
                            Group.Anima_Sky => CreateNewPosition_Anima_Sky(createdPositions, out newPosition),
                            _               => throw new ArgumentOutOfRangeException(nameof(group), group, null),
                        };

                        if (!success)
                        {
                            attemptIndex++;
                            continue;
                        }
                        else
                        {
                            positions[j] = newPosition;
                            break;
                        }
                    }
                }

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

        private static bool CreateNewPosition_SOS_Land(
            ReadOnlySpan<Vector3> createdPositions, out Vector3 outPosition)
        {
            const float CenterX = -500f;
            const float CenterZ = 350f;
            const float MaxRange = 600.0f;
            const float MinDistance = 20.0f;      // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
            const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

            outPosition = Vector3.zero;

            // ランダムな位置を計算 (X, Z)
            // 極座標系でランダムに選ぶ
            float r = Random.Range(0f, MaxRange);
            float theta = Random.Range(0f, Mathf.PI * 2f);
            float x = CenterX + r * Mathf.Cos(theta);
            float z = CenterZ + r * Mathf.Sin(theta);

            // 既に配置されたオブジェクトとの距離をチェック
            foreach (var pos in createdPositions)
            {
                float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                if (distanceSq < MinDistance * MinDistance)
                {
                    return false; // 近すぎる
                }
            }

            // 地表のY座標を算出
            // 無限長のレイを打つ
            if (!Physics.Raycast(new Vector3(x, 1000f, z), Vector3.down, out RaycastHit hitInfo))
                return false; // 地表が見つからなかった

            // 水上には配置できない
            string hitObjectName = hitInfo.collider.gameObject.name;
            if (hitObjectName == "WaterPlane")
                return false;

            // 配置成功
            outPosition = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
            return false;
        }

        private static bool CreateNewPosition_SOS_Sea(
            ReadOnlySpan<Vector3> createdPositions, out Vector3 outPosition)
        {
            const float CenterX = -500f;
            const float CenterZ = 350f;
            const float MaxRange = 600.0f;
            const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

            outPosition = Vector3.zero;

            // ランダムな位置を計算 (X, Z)
            // 極座標系でランダムに選ぶ
            float r = Random.Range(0f, MaxRange);
            float theta = Random.Range(0f, Mathf.PI * 2f);
            float x = CenterX + r * Mathf.Cos(theta);
            float z = CenterZ + r * Mathf.Sin(theta);

            // 地表のY座標を算出
            // 無限長のレイを打つ
            if (!Physics.Raycast(new Vector3(x, 1000f, z), Vector3.down, out RaycastHit hitInfo))
                return false; // 地表が見つからなかった

            // 水上にしか配置できない
            string hitObjectName = hitInfo.collider.gameObject.name;
            if (hitObjectName != "WaterPlane")
                return false;

            // 配置成功
            outPosition = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
            return true;
        }

        private static bool CreateNewPosition_SOS_Sky(
            out Vector3 outPosition,
            TerrainTreeInfo treeInfo)
        {
            // ランダムに、木の上の方に配置
            float height = Random.Range(treeInfo.Height * 0.5f, treeInfo.Height * 0.95f);
            outPosition = treeInfo.Position + Vector3.up * height;
            return true;
        }

        private static bool CreateNewPosition_Anima_Land(
            ReadOnlySpan<Vector3> createdPositions, out Vector3 outPosition,
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

            outPosition = Vector3.zero;

            // ランダムな位置を計算 (X, Z)
            // 極座標系でランダムに選ぶ
            float r = Random.Range(0f, MaxRange);
            float theta = Random.Range(0f, Mathf.PI * 2f);
            float x = CenterX + r * Mathf.Cos(theta);
            float z = CenterZ + r * Mathf.Sin(theta);

            // 既に配置されたオブジェクトとの距離をチェック
            foreach (var pos in createdPositions)
            {
                float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                if (distanceSq < MinDistance * MinDistance)
                {
                    return false; // 近すぎる
                }
            }

            // 対応するSOSオブジェクトとの距離をチェック
            foreach (var pos in sosLandPositions)
            {
                float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                if (distanceSq < MinDistanceToSOS * MinDistanceToSOS)
                {
                    return false; // 近すぎる
                }
            }

            {
                bool tooFarFromSOS = true;
                foreach (var pos in sosLandPositions)
                {
                    float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                    if (distanceSq <= MaxDistanceToSOS * MaxDistanceToSOS)
                    {
                        tooFarFromSOS = false;
                        break;
                    }
                }

                if (tooFarFromSOS)
                    return false; // 離れすぎている
            }

            // 地表のY座標を算出
            // 無限長のレイを打つ
            if (!Physics.Raycast(new Vector3(x, 1000f, z), Vector3.down, out RaycastHit hitInfo))
                return false; // 地表が見つからなかった

            // 水上には、滅多に配置できない
            string hitObjectName = hitInfo.collider.gameObject.name;
            if (hitObjectName == "WaterPlane")
            {
                if (Random.value > WaterPlacementProbability)
                    return false;
            }

            // 配置成功
            outPosition = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
            return true;
        }

        private static bool CreateNewPosition_Anima_Sea(
            ReadOnlySpan<Vector3> createdPositions, out Vector3 outPosition,
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

            outPosition = Vector3.zero;

            // ランダムな位置を計算 (X, Z)
            // 極座標系でランダムに選ぶ
            float r = Random.Range(0f, MaxRange);
            float theta = Random.Range(0f, Mathf.PI * 2f);
            float x = CenterX + r * Mathf.Cos(theta);
            float z = CenterZ + r * Mathf.Sin(theta);

            // 既に配置されたオブジェクトとの距離をチェック
            foreach (var pos in createdPositions)
            {
                float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                if (distanceSq < MinDistance * MinDistance)
                {
                    return false; // 近すぎる
                }
            }

            // 対応するSOSオブジェクトとの距離をチェック
            foreach (var pos in sosSeaPositions)
            {
                float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                if (distanceSq < MinDistanceToSOS * MinDistanceToSOS)
                {
                    return false; // 近すぎる
                }
            }

            {
                bool tooFarFromSOS = true;
                foreach (var pos in sosSeaPositions)
                {
                    float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                    if (distanceSq <= MaxDistanceToSOS * MaxDistanceToSOS)
                    {
                        tooFarFromSOS = false;
                        break;
                    }
                }

                if (tooFarFromSOS)
                    return false; // 離れすぎている
            }

            // 地表のY座標を算出
            // 無限長のレイを打つ
            if (!Physics.Raycast(new Vector3(x, 1000f, z), Vector3.down, out RaycastHit hitInfo))
                return false; // 地表が見つからなかった

            // 水上以外には、それほど配置できない
            string hitObjectName = hitInfo.collider.gameObject.name;
            if (hitObjectName != "WaterPlane")
            {
                if (Random.value > LandPlacementProbability)
                    return false;
            }

            // 配置成功
            outPosition = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
            return true;
        }

        private static bool CreateNewPosition_Anima_Sky(
            ReadOnlySpan<Vector3> createdPositions, out Vector3 outPosition)
        {
            // 場所を問わず、まばらに配置する

            const float CenterX = -500f;
            const float CenterZ = 350f;
            const float MaxRange = 600.0f;
            const float MinDistance = 40.0f;      // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
            const float HeightAboveGround = 0.1f; // 地表からどのくらい上に配置するか (m)

            outPosition = Vector3.zero;

            // ランダムな位置を計算 (X, Z)
            // 極座標系でランダムに選ぶ
            float r = Random.Range(0f, MaxRange);
            float theta = Random.Range(0f, Mathf.PI * 2f);
            float x = CenterX + r * Mathf.Cos(theta);
            float z = CenterZ + r * Mathf.Sin(theta);

            // 既に配置されたオブジェクトとの距離をチェック
            foreach (var pos in createdPositions)
            {
                float distanceSq = new Vector2(x - pos.x, z - pos.z).sqrMagnitude;
                if (distanceSq < MinDistance * MinDistance)
                {
                    return false; // 近すぎる
                }
            }

            // 地表のY座標を算出
            // 無限長のレイを打つ
            if (!Physics.Raycast(new Vector3(x, 1000f, z), Vector3.down, out RaycastHit hitInfo))
                return false; // 地表が見つからなかった

            // 配置成功
            outPosition = new Vector3(x, hitInfo.point.y + HeightAboveGround, z);
            return true;
        }
    }
}