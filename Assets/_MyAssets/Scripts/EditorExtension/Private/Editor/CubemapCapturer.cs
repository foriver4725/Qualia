using System.IO;
using UnityEditor;
using UnityEngine;

namespace MyScripts.EditorExtension.Private
{
    internal static class CubemapCapturer
    {
        [MenuItem("Tools/Cubemap Capturer (Open Window)")]
        private static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<Window>();
            window.titleContent = new GUIContent("Cubemap Capturer");
        }

        /// <summary>
        /// <para>カメラでCubemapレンダリングし、アセットとして保存する</para>
        /// </summary>
        private sealed class Window : EditorWindow
        {
            private Camera renderCamera;
            private int cubemapSize = 1024; // 正方形
            private string savePath = "Assets/_MyAssets/Scenes/Main/Cubemap.asset";

            private void OnGUI()
            {
                EditorGUILayout.LabelField("Capture Settings", EditorStyles.boldLabel);

                renderCamera = EditorGUILayout.ObjectField("Render Camera", renderCamera, typeof(Camera), true) as Camera;
                cubemapSize = EditorGUILayout.IntField("Cubemap Size", cubemapSize);
                savePath = EditorGUILayout.TextField("Save Path", savePath);

                EditorGUILayout.Space();

                // 値の不正チェック
                using (new EditorGUI.DisabledScope(!(
                    renderCamera != null &&
                    cubemapSize > 0 &&
                    !string.IsNullOrEmpty(savePath) && savePath.StartsWith("Assets/") && savePath.EndsWith(".asset")
                )))
                {
                    if (GUILayout.Button("Capture Cubemap"))
                    {
                        Capture(renderCamera, cubemapSize, savePath);
                    }
                }
            }

            // 値の不正チェックは済んでいる
            private static void Capture(Camera renderCamera, int cubemapSize, string savePath)
            {
                // Cubemap RT
                var cubemapRT = new RenderTexture(cubemapSize, cubemapSize, 24)
                {
                    dimension = UnityEngine.Rendering.TextureDimension.Cube
                };
                cubemapRT.Create();

                // カメラ → Cubemap RT にレンダリング
                renderCamera.RenderToCubemap(cubemapRT);

                // Cubemap アセットを作成
                var cubemap = new Cubemap(cubemapSize, TextureFormat.RGBA32, true);
                cubemap.Apply(updateMipmaps: true, makeNoLongerReadable: false);

                // Cubemap RT -> Cubemap アセット にコピー
                // 各面それぞれコピーする
                for (int face = 0; face < 6; face++)
                {
                    Graphics.CopyTexture(
                        cubemapRT, face, 0,
                        cubemap, face, 0
                    );
                }

                // 保存先のディレクトリが無いなら、新規作成
                string saveDirectory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(saveDirectory))
                    Directory.CreateDirectory(saveDirectory);

                // Cubemap アセットを保存して、リロードする
                AssetDatabase.CreateAsset(cubemap, savePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // リソースを破棄
                cubemapRT.Release();
                DestroyImmediate(cubemapRT);

                Debug.Log($"Cubemap captured and saved to '{savePath}'");
            }
        }
    }
}
