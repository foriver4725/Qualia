# Qualia
クオリアを用いた、「自然とのコミュニケーション」

### 環境構築の手順
1. プロジェクトをクローンする
2. 「TextMeshPro」 パッケージをインポートする
3. 「Unity Terrain - HDRP Demo Scene」 パッケージをインポートする
4. 「Free Quick Effects Vol. 1」 パッケージをインポートする
5. その他必要ならば、 「使用ライブラリ」 の項を参考にインポートする
6. 「Assets/TerrainDemoScene_HDRP/Prefabs/Rocks/Rock_Overgrown_D.prefab」 プレハブを開き、ヒエラルキーから 「Rock_Overgrown_D/Rock_Overgrown_I_LOD00」 ゲームオブジェクトを選択する。 「Box Collider」 コンポーネントをリムーブし、 「Mesh Collider」 コンポーネントをアタッチする。

### 使用ライブラリ
- UniTask
- R3
- ZString
- CsprojModifier
- NuGetForUnity
- DOTween
- Unity Profiling Core API