using UnityEngine.InputSystem;

namespace MyScripts.Common
{
    internal enum InputType : byte
    {
        /// <summary>
        /// 【null】デフォルト値。何も意味しない
        /// </summary>
        Null,

        /// <summary>
        /// 【bool】そのフレームが、押された瞬間のフレームであるか
        /// </summary>
        Click,

        /// <summary>
        /// 【bool】そのフレームが、一定秒数押された瞬間のフレームであるか
        /// </summary>
        Hold,

        /// <summary>
        /// 【bool】そのフレームにおける、押されているかのフラグ
        /// </summary>
        Value0,

        /// <summary>
        /// 【float】そのフレームにおける、1軸の入力の値(単位線 以内)
        /// </summary>
        Value1,

        /// <summary>
        /// 【Vector2】そのフレームにおける、2軸の入力の値(単位円 以内)
        /// </summary>
        Value2,

        /// <summary>
        /// 【Vector3】そのフレームにおける、3軸の入力の値(単位球 以内)
        /// </summary>
        Value3
    }

    internal sealed class InputInfo : IDisposable
    {
        private InputAction _inputAction;
        private readonly InputType _type;
        private Action<InputAction.CallbackContext>[] _action;

        internal bool Bool { get; private set; } = false;
        internal float Float { get; private set; } = 0;
        internal Vector2 Vector2 { get; private set; } = Vector2.zero;
        internal Vector3 Vector3 { get; private set; } = Vector3.zero;

        internal InputInfo(InputAction inputAction, InputType type)
        {
            this._inputAction = inputAction;
            this._type = type;

            this._action = this._type switch
            {
                InputType.Null => null,

                InputType.Click => new Action<InputAction.CallbackContext>[]
                {
                    _ => { Bool = true; }
                },

                InputType.Hold => new Action<InputAction.CallbackContext>[]
                {
                    _ => { Bool = true; }
                },

                InputType.Value0 => new Action<InputAction.CallbackContext>[]
                {
                    _ => { Bool = true; },
                    _ => { Bool = false; }
                },

                InputType.Value1 => new Action<InputAction.CallbackContext>[]
                {
                    e => { Float = e.ReadValue<float>(); }
                },

                InputType.Value2 => new Action<InputAction.CallbackContext>[]
                {
                    e => { Vector2 = e.ReadValue<Vector2>(); }
                },

                InputType.Value3 => new Action<InputAction.CallbackContext>[]
                {
                    e => { Vector3 = e.ReadValue<Vector3>(); }
                },

                _ => null
            };
        }

        public void Dispose()
        {
            Array.Clear(_action, 0, _action.Length);
            _action = null;

            _inputAction = null;
        }

        internal void Link(bool isLink)
        {
            if (_inputAction == null) return;
            if (_action == null) return;

            if (isLink)
            {
                switch (_type)
                {
                    case InputType.Null:
                        break;

                    case InputType.Click:
                        _inputAction.performed += _action[0];
                        break;

                    case InputType.Hold:
                        _inputAction.performed += _action[0];
                        break;

                    case InputType.Value0:
                        _inputAction.performed += _action[0];
                        _inputAction.canceled += _action[1];
                        break;

                    case InputType.Value1:
                        _inputAction.started += _action[0];
                        _inputAction.performed += _action[0];
                        _inputAction.canceled += _action[0];
                        break;

                    case InputType.Value2:
                        _inputAction.started += _action[0];
                        _inputAction.performed += _action[0];
                        _inputAction.canceled += _action[0];
                        break;

                    case InputType.Value3:
                        _inputAction.started += _action[0];
                        _inputAction.performed += _action[0];
                        _inputAction.canceled += _action[0];
                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch (_type)
                {
                    case InputType.Null:
                        break;

                    case InputType.Click:
                        _inputAction.performed -= _action[0];
                        break;

                    case InputType.Hold:
                        _inputAction.performed -= _action[0];
                        break;

                    case InputType.Value0:
                        _inputAction.performed -= _action[0];
                        _inputAction.canceled -= _action[1];
                        break;

                    case InputType.Value1:
                        _inputAction.started -= _action[0];
                        _inputAction.performed -= _action[0];
                        _inputAction.canceled -= _action[0];
                        break;

                    case InputType.Value2:
                        _inputAction.started -= _action[0];
                        _inputAction.performed -= _action[0];
                        _inputAction.canceled -= _action[0];
                        break;

                    case InputType.Value3:
                        _inputAction.started -= _action[0];
                        _inputAction.performed -= _action[0];
                        _inputAction.canceled -= _action[0];
                        break;

                    default:
                        break;
                }
            }
        }

        internal void ResetFlags()
        {
            if (_type == InputType.Click && Bool) Bool = false;
            else if (_type == InputType.Hold && Bool) Bool = false;
        }
    }

    internal abstract class AInputManager<T> : MonoBehaviour where T : AInputManager<T>
    {
        //TODO: ASingletonMonoBehaviour クラスと同じ内容
        private static T _instance = null;
        internal static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    T[] instances = FindObjectsByType<T>(FindObjectsSortMode.None);
                    if (instances == null || instances.Length == 0)
                    {
                        $"No instance of {typeof(T).Name} found in the scene. Please ensure there is one instance present.".LogError();
                        return null;
                    }
                    else if (instances.Length == 1)
                    {
                        _instance = instances[0];
                    }
                    else
                    {
                        $"Multiple instances of {typeof(T).Name} found in the scene. Using the first instance and destroying others.".LogWarning();
                        _instance = instances[0];
                        for (int i = 1; i < instances.Length; ++i)
                        {
                            Destroy(instances[i]);
                        }
                    }
                }

                return _instance;
            }
        }



        private protected MyActions _ia { get; private set; } = null;
        private List<InputInfo> _inputInfoList;

        private void Awake()
        {
            if (Instance == null)
            {
                $"Failed to initialize singleton instance of {typeof(T).Name}. Ensure that there is only one instance in the scene.".LogError();
                return;
            }

            _ia = new();
            _inputInfoList = new(64);

            Init();

            foreach (InputInfo e in _inputInfoList) e?.Link(true);
        }
        private void OnDestroy()
        {
            foreach (InputInfo e in _inputInfoList) e?.Link(false);

            _ia?.Dispose();
            foreach (InputInfo e in _inputInfoList) e?.Dispose();

            _ia = null;
            _inputInfoList = null;
        }

        private void OnEnable()
        {
            _ia?.Enable();
            InputSystem.onBeforeUpdate += ResetFlags;
        }
        private void OnDisable()
        {
            _ia?.Disable();
            InputSystem.onBeforeUpdate -= ResetFlags;
        }

        private void ResetFlags()
        {
            foreach (InputInfo e in _inputInfoList) e?.ResetFlags();
        }

        private protected InputInfo Setup(InputAction inputAction, InputType type)
        {
            InputInfo info = new(inputAction, type);
            _inputInfoList.Add(info);
            return info;
        }

        private protected abstract void Init();
    }
}
