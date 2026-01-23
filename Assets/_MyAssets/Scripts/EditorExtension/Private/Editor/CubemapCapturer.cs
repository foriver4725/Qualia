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

                // 既存アセットを探し、存在するならばそれをロードする
                // 既存アセットが無いならば、新規作成する
                var existingCubemap = AssetDatabase.LoadAssetAtPath<Cubemap>(savePath);
                Cubemap cubemap;
                if (existingCubemap != null)
                {
                    cubemap = existingCubemap;
                    Undo.RegisterCompleteObjectUndo(cubemap, "Update Cubemap Asset");
                }
                else
                {
                    cubemap = new Cubemap(cubemapSize, TextureFormat.RGBA32, true);
                    AssetDatabase.CreateAsset(cubemap, savePath);
                }

                // Cubemap RT -> Cubemap アセット にコピー
                // 各面それぞれコピーする
                // Graphics.CopyTexture だと上手くいかなかった
                RenderTexture previousRT = RenderTexture.active;
                for (int face = 0; face < 6; face++)
                {
                    // Cubemap の指定 face を 2D RT にコピー
                    var tempRT = RenderTexture.GetTemporary(
                        cubemapSize,
                        cubemapSize,
                        0,
                        RenderTextureFormat.ARGB32,
                        RenderTextureReadWrite.Linear
                    );

                    Graphics.CopyTexture(
                        cubemapRT, face, 0,
                        tempRT, 0, 0
                    );

                    RenderTexture.active = tempRT;

                    var tex = new Texture2D(
                        cubemapSize,
                        cubemapSize,
                        TextureFormat.RGBA32,
                        false,
                        true // linear
                    );

                    tex.ReadPixels(new Rect(0, 0, cubemapSize, cubemapSize), 0, 0);
                    tex.Apply();

                    cubemap.SetPixels(tex.GetPixels(), (CubemapFace)face);

                    DestroyImmediate(tex);
                    RenderTexture.ReleaseTemporary(tempRT);
                }
                RenderTexture.active = previousRT;

                // 変更を確定する
                cubemap.Apply(updateMipmaps: true, makeNoLongerReadable: false);

                // Dirty フラグを立てて保存
                EditorUtility.SetDirty(cubemap);
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
