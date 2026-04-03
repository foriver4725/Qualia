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
                CreatePositions(group, positions, param, treeInfos, sosLandPositions, sosSeaPositions);

                foreach (Vector3 position in positions)
                {
                    _ = Instantiate(prefab, position, Quaternion.identity, parent);
                }

                // 座標をメモっておいて、この後の配置で活用してもらう
                if (group == Group.SOS_Land) sosLandPositions = positions;
                else if (group == Group.SOS_Sea) sosSeaPositions = positions;
            }
        }

        private static void CreatePositions(
            Group group, Span<Vector3> outPositions,
            SSOSAnimaArrangement param,
            ReadOnlySpan<TerrainTreeInfo> treeInfos = default, // グループによって使われるなら、明示的に指定しておくこと
            Span<Vector3> sosLandPositions = default,          // グループによって使われるなら、明示的に指定しておくこと
            Span<Vector3> sosSeaPositions = default            // グループによって使われるなら、明示的に指定しておくこと
        )
        {
            outPositions.Fill(Vector3.zero);

            for (int i = 0; i < outPositions.Length; i++)
            {
                // 上限に達するまで、配置できる座標を探し続ける
                int attemptIndex = 0;
                while (attemptIndex < param.MaxAttempts)
                {
                    ReadOnlySpan<Vector3> createdPositions = outPositions[0..i];

                    Vector3 newPosition;
                    bool success = group switch
                    {
                        Group.SOS_Land => TryCreateNewPosition_SOS_Land(createdPositions, out newPosition, param),
                        Group.SOS_Sea  => TryCreateNewPosition_SOS_Sea(out newPosition, param),
                        Group.SOS_Sky  => TryCreateNewPosition_SOS_Sky(out newPosition, treeInfos[i]),
                        Group.Anima_Land => TryCreateNewPosition_Anima_Land(createdPositions, out newPosition,
                            param, sosLandPositions),
                        Group.Anima_Sea => TryCreateNewPosition_Anima_Sea(createdPositions, out newPosition,
                            param, sosSeaPositions),
                        Group.Anima_Sky => TryCreateNewPosition_Anima_Sky(createdPositions, out newPosition, param),
                        _               => throw new ArgumentOutOfRangeException(nameof(group), group, null),
                    };

                    if (!success)
                    {
                        attemptIndex++;
                        continue;
                    }
                    else
                    {
                        outPositions[i] = newPosition;
                        break;
                    }
                }
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
                    if (!IsTreePrefab(prefabName))
                        continue;

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

            static bool IsTreePrefab(string prefabName)
            {
                foreach (string treeName in SSOSAnimaArrangement.TreeNameHeightMap.Keys)
                {
                    if (prefabName == treeName)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static bool TryCreateNewPosition_SOS_Land(
            ReadOnlySpan<Vector3> createdPositions, out Vector3 outPosition,
            SSOSAnimaArrangement param)
        {
            const float MinDistance = 20.0f; // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)

            outPosition = Vector3.zero;

            Vector2 position = CreateCandidatePositionRandomly(param);

            if (IsCloseToAnyPositionXZ(position, createdPositions, MinDistance))
                return false;

            if (!DoesGroundExistBelow(position, out RaycastHit hitInfo))
                return false;

            // 水上には配置できない
            if (IsWaterPlaneObject(hitInfo.collider.gameObject))
                return false;

            outPosition = position.ToVector3(y: hitInfo.point.y + param.HeightAboveGround);
            return false;
        }

        private static bool TryCreateNewPosition_SOS_Sea(
            out Vector3 outPosition,
            SSOSAnimaArrangement param)
        {
            outPosition = Vector3.zero;

            Vector2 position = CreateCandidatePositionRandomly(param);

            if (!DoesGroundExistBelow(position, out RaycastHit hitInfo))
                return false;

            // 水上にしか配置できない
            if (!IsWaterPlaneObject(hitInfo.collider.gameObject))
                return false;

            outPosition = position.ToVector3(y: hitInfo.point.y + param.HeightAboveGround);
            return true;
        }

        private static bool TryCreateNewPosition_SOS_Sky(
            out Vector3 outPosition,
            TerrainTreeInfo treeInfo)
        {
            // ランダムに、木の上の方に配置
            float height = Random.Range(treeInfo.Height * 0.5f, treeInfo.Height * 0.95f);
            outPosition = treeInfo.Position + Vector3.up * height;
            return true;
        }

        private static bool TryCreateNewPosition_Anima_Land(
            ReadOnlySpan<Vector3> createdPositions, out Vector3 outPosition,
            SSOSAnimaArrangement param, Span<Vector3> sosLandPositions)
        {
            const float MinDistance = 30.0f;              // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
            const float MinDistanceToSOS = 5.0f;          // SOSオブジェクトとは、最低どれくらい離すか (m. XZ平面距離)
            const float MaxDistanceToSOS = 50.0f;         // SOSオブジェクトとは、最大どれくらい離すか (m. XZ平面距離)
            const float WaterPlacementProbability = 0.1f; // 水上に配置する確率

            outPosition = Vector3.zero;

            Vector2 position = CreateCandidatePositionRandomly(param);

            if (IsCloseToAnyPositionXZ(position, createdPositions, MinDistance))
                return false;

            if (IsCloseToAnyPositionXZ(position, sosLandPositions, MinDistanceToSOS))
                return false;

            if (!IsCloseToAnyPositionXZ(position, sosLandPositions, MaxDistanceToSOS))
                return false;

            if (!DoesGroundExistBelow(position, out RaycastHit hitInfo))
                return false;

            // 水上には、滅多に配置できない
            if (IsWaterPlaneObject(hitInfo.collider.gameObject) && Random.value > WaterPlacementProbability)
                return false;

            outPosition = position.ToVector3(y: hitInfo.point.y + param.HeightAboveGround);
            return true;
        }

        private static bool TryCreateNewPosition_Anima_Sea(
            ReadOnlySpan<Vector3> createdPositions, out Vector3 outPosition,
            SSOSAnimaArrangement param, Span<Vector3> sosSeaPositions)
        {
            const float MinDistance = 20.0f;             // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)
            const float MinDistanceToSOS = 5.0f;         // SOSオブジェクトとは、最低どれくらい離すか (m. XZ平面距離)
            const float MaxDistanceToSOS = 150.0f;       // SOSオブジェクトとは、最大どれくらい離すか (m. XZ平面距離)
            const float LandPlacementProbability = 0.5f; // 水上以外の場所に配置する確率

            outPosition = Vector3.zero;

            Vector2 position = CreateCandidatePositionRandomly(param);

            if (IsCloseToAnyPositionXZ(position, createdPositions, MinDistance))
                return false;

            if (IsCloseToAnyPositionXZ(position, sosSeaPositions, MinDistanceToSOS))
                return false;

            if (!IsCloseToAnyPositionXZ(position, sosSeaPositions, MaxDistanceToSOS))
                return false;

            if (!DoesGroundExistBelow(position, out RaycastHit hitInfo))
                return false;

            // 水上以外には、滅多に配置できない
            if (!IsWaterPlaneObject(hitInfo.collider.gameObject) && Random.value > LandPlacementProbability)
                return false;

            outPosition = position.ToVector3(y: hitInfo.point.y + param.HeightAboveGround);
            return true;
        }

        private static bool TryCreateNewPosition_Anima_Sky(
            ReadOnlySpan<Vector3> createdPositions, out Vector3 outPosition,
            SSOSAnimaArrangement param)
        {
            // 場所を問わず、まばらに配置する

            const float MinDistance = 40.0f; // 他のオブジェクトと、最低どれ以上話すか (m. XZ平面距離)

            outPosition = Vector3.zero;

            Vector2 position = CreateCandidatePositionRandomly(param);

            if (IsCloseToAnyPositionXZ(position, createdPositions, MinDistance))
                return false;

            if (!DoesGroundExistBelow(position, out RaycastHit hitInfo))
                return false;

            outPosition = position.ToVector3(y: hitInfo.point.y + param.HeightAboveGround);
            return true;
        }

        // 水面のオブジェクトかどうか判定する
        private static bool IsWaterPlaneObject(GameObject go) => go.name == "WaterPlane";

        // 候補座標をランダムに作成する
        private static Vector2 CreateCandidatePositionRandomly(SSOSAnimaArrangement param)
        {
            float range = Random.Range(0f, param.MaxRange);
            return range * Random.onUnitCircle + param.Center;
        }

        /// <summary>
        /// 判定する座標について、与えられた座標群の中でXZ平面の距離が境界値以下のものがあるか調べる<br/>
        /// ( = 近すぎるものがあるかどうか)<br/>
        /// </summary>
        private static bool IsCloseToAnyPositionXZ(
            Vector2 targetPosition, ReadOnlySpan<Vector3> positions,
            float thresholdDistance)
        {
            foreach (var position in positions)
            {
                Vector2 diffXZ = new Vector2(position.x, position.z) - targetPosition;
                float distanceSq = diffXZ.sqrMagnitude;

                if (distanceSq < thresholdDistance * thresholdDistance)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 地表が存在する座標かどうか、レイキャストを使って判定する
        /// </summary>
        private static bool DoesGroundExistBelow(Vector2 positionXZ, out RaycastHit rayCastHitInfo)
        {
            Vector3 origin = positionXZ.ToVector3(y: 1000.0f);                // 十分高い位置から
            return Physics.Raycast(origin, Vector3.down, out rayCastHitInfo); // 真下に無限長
        }
    }

    file static class Utils
    {
        /// <summary>
        /// XZ平面の座標 -> XYZ空間の座標 に変換する
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector3 ToVector3(this Vector2 vectorXZ, float y)
            => new(vectorXZ.x, y, vectorXZ.y);
    }
}