using System.IO;
using UnityEditor;
using UnityEngine;

namespace MyScripts.EditorExtension.Private
{
    internal static class PanoramaCapturer
    {
        [MenuItem("Tools/Panorama Capturer (Open Window)")]
        private static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<Window>();
            window.titleContent = new GUIContent("Panorama Capturer");
        }

        /// <summary>
        /// <para>- カメラでCubemapをレンダリング</para>
        /// <para>- CubemapをEquirectangularに変換</para>
        /// <para>- PNGで保存</para>
        /// </summary>
        private sealed class Window : EditorWindow
        {
            private Camera renderCamera;

            private Vector2Int cubemapSize = new(1024, 1024);         // Cubemap サイズ (1:1)
            private Vector2Int equirectangularSize = new(2048, 1024); // Equirectangular サイズ (2:1)

            private string savePath = "Assets/_MyAssets/Textures/Panorama.png";

            private void OnGUI()
            {
                EditorGUILayout.LabelField("Capture Settings", EditorStyles.boldLabel);

                // シーンのカメラを設定
                renderCamera = EditorGUILayout.ObjectField("Render Camera", renderCamera, typeof(Camera), true) as Camera;

                // 画像のサイズ設定
                cubemapSize = EditorGUILayout.Vector2IntField("Cubemap Size (1:1)", cubemapSize);
                equirectangularSize = EditorGUILayout.Vector2IntField("Equirectangular Size (2:1)", equirectangularSize);

                // 保存パス設定
                savePath = EditorGUILayout.TextField("Save Path", savePath);

                EditorGUILayout.Space();

                // 値の不正チェック
                using (new EditorGUI.DisabledScope(!(
                    renderCamera != null &&
                    cubemapSize is { x: > 0, y: > 0 } and { x: var cx, y: var cy } && cx == cy &&
                    equirectangularSize is { x: > 0, y: > 0 } and { x: var ex, y: var ey } && ex == 2 * ey &&
                    !string.IsNullOrEmpty(savePath) && savePath.StartsWith("Assets/") && savePath.EndsWith(".png")
                )))
                {
                    if (GUILayout.Button("Capture Panorama"))
                    {
                        Capture(renderCamera, cubemapSize, equirectangularSize, savePath);
                    }
                }
            }

            // 値の不正チェックは済んでいる
            private static void Capture(Camera renderCamera, Vector2Int cubemapSize, Vector2Int equirectangularSize, string savePath)
            {
                // Cubemap RT
                var cubemapRT = new RenderTexture(
                    cubemapSize.x, cubemapSize.y, 24)
                {
                    dimension = UnityEngine.Rendering.TextureDimension.Cube
                };
                cubemapRT.Create();

                // Equirectangular RT
                var equirectangularRT = new RenderTexture(
                    equirectangularSize.x, equirectangularSize.y, 0, RenderTextureFormat.ARGB32);
                equirectangularRT.Create();

                // カメラ → Cubemap にレンダリング
                renderCamera.RenderToCubemap(cubemapRT);

                // Cubemap → Equirectangular に変換
                // Graphics.ConvertTexture は環境差により上手くいかなかったので、Blitするためのシェーダーを作って対応する.
                var rtBlitShader = Shader.Find("Hidden/PanoramaCapturer_CubemapToEquirectangular");
                var rtBlitMaterial = new Material(rtBlitShader);
                rtBlitMaterial.SetTexture("_Cube", cubemapRT);
                Graphics.Blit(null, equirectangularRT, rtBlitMaterial);

                // Equirectangular RT から Texture2D に読み込み
                var equirectangularTexture = new Texture2D(equirectangularSize.x, equirectangularSize.y, TextureFormat.RGBA32, false);
                var previousRT = RenderTexture.active;
                {
                    RenderTexture.active = equirectangularRT;
                    equirectangularTexture.ReadPixels(new Rect(0, 0, equirectangularSize.x, equirectangularSize.y), 0, 0);
                    equirectangularTexture.Apply();
                }
                RenderTexture.active = previousRT;

                // PNGで保存
                var bytes = equirectangularTexture.EncodeToPNG();
                File.WriteAllBytes(savePath, bytes);

                // リソースを破棄
                DestroyImmediate(rtBlitMaterial);
                DestroyImmediate(equirectangularTexture);
                cubemapRT.Release();
                equirectangularRT.Release();

                // アセットをリロード
                AssetDatabase.ImportAsset(savePath);
                AssetDatabase.Refresh();

                // 保存したPNGのインポート設定を調整して再インポートし、
                // そのままパノラマテクスチャとして使えるようにする
                var importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
                if (importer != null)
                {
                    importer.sRGBTexture = true;
                    importer.wrapMode = TextureWrapMode.Repeat;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }

                Debug.Log($"Panorama captured and saved to: {savePath}");
            }
        }
    }
}
