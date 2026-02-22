using System;
using UnityEditor;
using UnityEngine;

namespace MyScripts.EditorExtension.Private
{
    [CustomEditor(typeof(ParticleSystem))]
    [CanEditMultipleObjects]
    internal sealed class ParticleSystemInspector : Editor
    {
        private static readonly Type BASE_EDITOR_TYPE = typeof(Editor)
            .Assembly
            .GetType("UnityEditor.ParticleSystemInspector");

        private Editor _cachedBaseEditor;

        private void OnDisable()
        {
            if (_cachedBaseEditor != null)
            {
                DestroyImmediate(_cachedBaseEditor);
                _cachedBaseEditor = null;
            }
        }

        public override void OnInspectorGUI()
        {
            // Unityバージョン差分で内部型が取れずnullになり得る。
            // その場合は最低限のフォールバックとして通常のDrawDefaultInspectorに落とす。
            if (BASE_EDITOR_TYPE == null)
            {
                EditorGUILayout.HelpBox(
                    "UnityEditor.ParticleSystemInspector が見つからないため、簡易表示(DrawDefaultInspector)で描画します。\n" +
                    "（Unityバージョン差分が原因です）",
                    MessageType.Warning);

                DrawDefaultInspector();
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Play"))
                {
                    foreach (var obj in targets)
                        if (obj is ParticleSystem ps) ps.Play();
                }

                if (GUILayout.Button("Stop"))
                {
                    foreach (var obj in targets)
                        if (obj is ParticleSystem ps) ps.Stop();
                }

                if (GUILayout.Button("Pause"))
                {
                    foreach (var obj in targets)
                        if (obj is ParticleSystem ps) ps.Pause();
                }

                if (GUILayout.Button("Clear"))
                {
                    foreach (var obj in targets)
                        if (obj is ParticleSystem ps) ps.Clear();
                }
            }

            // 毎回CreateEditorするとInspector再描画のたびにEditorインスタンスが生成され、
            // 破棄されないとGC/メモリ増加やリソースリークの原因になる。
            // CreateCachedEditorでキャッシュし、OnDisableで破棄する。
            CreateCachedEditor(target, BASE_EDITOR_TYPE, ref _cachedBaseEditor);
            if (_cachedBaseEditor != null)
            {
                _cachedBaseEditor.OnInspectorGUI();
            }
        }
    }
}
