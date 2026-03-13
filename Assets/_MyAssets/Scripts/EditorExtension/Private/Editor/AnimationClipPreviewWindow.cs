using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyScripts.EditorExtension.Private
{
    internal sealed class AnimationClipPreviewWindow : EditorWindow
    {
        private const float TopHeight = 72f;

        private PreviewRenderUtility _pru;

        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationClipPlayable _playable;

        private AnimationClip _clip;
        private GameObject _prefab;

        private GameObject _instance;
        private GameObject _from;
        private Animator _animator;

        private bool _playing;
        private double _lastTime;

        [OnOpenAsset(0)]
        internal static bool OnOpen(int entityId, int line)
        {
            if (EditorUtility.EntityIdToObject(entityId) is not AnimationClip clip) return false;
            var w = GetWindow<AnimationClipPreviewWindow>("AnimationClip Preview");
            w.minSize = new Vector2(640, 640);
            w._clip = clip;
            return true;
        }

        [MenuItem("Tools/ScreenPocket/AnimationClip Preview")]
        internal static void Open()
        {
            var w = GetWindow<AnimationClipPreviewWindow>("AnimationClip Preview");
            w.minSize = new Vector2(640, 640);
        }

        private void OnEnable()
        {
            EnsurePRU();

            _playing = false;
            _lastTime = EditorApplication.timeSinceStartup;

            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Tick;
            ResetAll();
            CleanupPRU();
        }

        private void Tick()
        {
            if (!_playing) return;
            if (!_graph.IsValid() || !_playable.IsValid()) { _playing = false; return; }

            var now = EditorApplication.timeSinceStartup;
            var dt = now - _lastTime;
            _lastTime = now;

            if (dt <= 0 || dt > 0.2) dt = 1.0 / 60.0;

            var dur = _playable.GetDuration();
            if (dur > 0)
            {
                var remain = dur - _playable.GetTime();
                if (remain > 0 && dt > remain) dt = remain;
            }

            _graph.Evaluate((float)dt);

            if (dur > 0 && _playable.GetTime() >= dur)
            {
                _playable.SetTime(dur);
                Stop();
                Eval0();
            }

            Repaint();
        }

        private void OnGUI()
        {
            EnsurePRU();
            if (_pru == null)
            {
                EditorGUILayout.HelpBox("PreviewRenderUtility の初期化に失敗しました。", MessageType.Error);
                return;
            }

            _clip = (AnimationClip)EditorGUILayout.ObjectField("AnimationClip", _clip, typeof(AnimationClip), false);

            _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab (Asset Only)", _prefab, typeof(GameObject), false);
            if (_prefab != null && !PrefabUtility.IsPartOfPrefabAsset(_prefab)) _prefab = null;

            if (_prefab == null)
                EditorGUILayout.HelpBox("Prefab(Asset) を指定してください（シーン上オブジェクトは不可）。", MessageType.Info);
            else if (_instance != null && _animator == null)
                EditorGUILayout.HelpBox("Animator が見つかりません。このPrefabに Animator を追加してください。", MessageType.Warning);
            else if (_clip == null)
                EditorGUILayout.HelpBox("AnimationClip を指定してください。", MessageType.Info);

            EnsureAll();

            bool requestedDraw = DrawTransport();

            var rect = new Rect(0, TopHeight, position.width, position.height - TopHeight);
            if (rect.width <= 1f || rect.height <= 1f) return;

            _pru.BeginPreview(rect, GUIStyle.none);
            try
            {
                UpdateCamera(_pru.camera);
                _pru.Render(); 
            }
            finally
            {
                _pru.EndAndDrawPreview(rect);
            }

            if (requestedDraw) Repaint();
        }

        

        private void EnsureAll()
        {
            // Prefabなし → 全落とし
            if (_prefab == null)
            {
                if (_instance != null) ResetAll();
                return;
            }

            // Instance差し替え（または初回）
            if (_instance == null || _from != _prefab)
            {
                ResetAll();

                _instance = _pru.InstantiatePrefabInScene(_prefab);
                if (_instance == null) return;

                HideDontSaveRecursive(_instance);
                ResetTransform(_instance.transform);

                _animator = _instance.GetComponentInChildren<Animator>(true);
                _from = _prefab;

                Stop();
            }

            // Animator無し → Graph不要
            if (_animator == null)
            {
                ResetGraphOnly();
                return;
            }

            // Graph生成
            if (!_graph.IsValid())
            {
                _graph = PlayableGraph.Create("Animation Preview Window Graph");
                _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                _output = AnimationPlayableOutput.Create(_graph, "Output", _animator);
                _graph.Play();

                Stop();
                _lastTime = EditorApplication.timeSinceStartup;
            }

            // Clip未指定 → Output切る
            if (_clip == null)
            {
                if (_output.IsOutputValid()) _output.SetSourcePlayable(Playable.Null);
                Stop();
                return;
            }

            // Playable生成/差し替え
            if (_playable.IsValid() && _playable.GetAnimationClip() == _clip) return;

            if (_output.IsOutputValid()) _output.SetSourcePlayable(Playable.Null);
            if (_playable.IsValid()) _playable.Destroy();

            _playable = AnimationClipPlayable.Create(_graph, _clip);
            _playable.SetDuration(_clip.length);
            _playable.SetTime(0);
            _playable.SetSpeed(0);

            _output.SetSourcePlayable(_playable);

            Stop();
            Eval0();
        }

        private bool DrawTransport()
        {
            if (!_playable.IsValid()) { _playing = false; return false; }

            bool changed = false;

            void Apply(System.Action act, bool play)
            {
                act?.Invoke();
                _playing = play;
                if (_playable.IsValid()) _playable.SetSpeed(play ? 1 : 0);
                if (play) _lastTime = EditorApplication.timeSinceStartup;
                if (_graph.IsValid()) _graph.Evaluate(0);
                changed = true;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            var t = EditorGUILayout.Slider("Time", (float)_playable.GetTime(), 0f, (float)_playable.GetDuration());
            if (EditorGUI.EndChangeCheck())
                Apply(() => _playable.SetTime(t), false);

            if (GUILayout.Button("|<<", GUILayout.Width(28))) Apply(() => _playable.SetTime(0), false);
            if (GUILayout.Button("||", GUILayout.Width(28))) Apply(null, false);
            if (GUILayout.Button(">", GUILayout.Width(28))) Apply(null, true);
            if (GUILayout.Button(">>|", GUILayout.Width(28))) Apply(() => _playable.SetTime(_playable.GetDuration()), false);

            EditorGUILayout.EndHorizontal();
            return changed;
        }

        private void Stop()
        {
            _playing = false;
            if (_playable.IsValid()) _playable.SetSpeed(0);
        }

        private void Eval0()
        {
            if (_graph.IsValid()) _graph.Evaluate(0);
        }

        private void ResetAll()
        {
            if (_instance != null) { DestroyImmediate(_instance); _instance = null; }
            _from = null;
            _animator = null;
            Stop();
            ResetGraphOnly();
        }

        private void ResetGraphOnly()
        {
            if (_output.IsOutputValid()) _output.SetSourcePlayable(Playable.Null);
            if (_playable.IsValid()) _playable.Destroy();
            if (_graph.IsValid()) _graph.Destroy();
        }

        private void EnsurePRU()
        {
            if (_pru != null) return;
            _pru = new PreviewRenderUtility();
            _pru.lights[0].transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        }

        private void CleanupPRU()
        {
            if (_pru == null) return;
            _pru.Cleanup();
            _pru = null;
        }

        private static void HideDontSaveRecursive(GameObject go)
        {
            go.hideFlags = HideFlags.HideAndDontSave;
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
                t.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        private static void ResetTransform(Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        private static void UpdateCamera(Camera camera)
        {
            camera.farClipPlane = 100;
            camera.transform.position = new Vector3(0, 1f, 5f);
            camera.transform.rotation = Quaternion.Euler(0, 180, 0);
            camera.clearFlags = CameraClearFlags.Skybox;
        }
    }
}
