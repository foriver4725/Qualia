using UnityEngine.InputSystem;

namespace MyScripts.Common
{
    internal enum InputType : byte
    {
        /// <summary>
        /// デフォルト値. 何も意味しない<br/>
        /// 入力は取得する意味がない<br/>
        /// </summary>
        Null,

        /// <summary>
        /// 現在のフレームが押された瞬間のフレームであるか<br/>
        /// 入力は Bool で取得する<br/>
        /// </summary>
        Click,

        /// <summary>
        /// 現在のフレームが一定秒数押された瞬間のフレームであるか<br/>
        /// 入力は Bool で取得する<br/>
        /// </summary>
        Hold,

        /// <summary>
        /// 現在のフレームにおける入力値 (真偽値)<br/>
        /// 入力は Bool で取得する<br/>
        /// ※ Action Type は Pass Through ではなく Button である想定<br/>
        /// </summary>
        Value0,

        /// <summary>
        /// 現在のフレームにおける入力値 (正規化スカラー)<br/>
        /// 入力は Float で取得する<br/>
        /// </summary>
        Value1,

        /// <summary>
        /// 現在のフレームにおける入力値 (正規化2次元ベクトル)<br/>
        /// 入力は Vector2 で取得する<br/>
        /// </summary>
        Value2,

        /// <summary>
        /// 現在のフレームにおける入力値 (正規化3次元ベクトル)<br/>
        /// 入力は Vector3 で取得する<br/>
        /// </summary>
        Value3
    }

    internal sealed class InputInfo
    {
        private readonly InputType type;

        internal bool Bool { get; private set; } = false;
        internal float Float { get; private set; } = 0;
        internal Vector2 Vector2 { get; private set; } = Vector2.zero;
        internal Vector3 Vector3 { get; private set; } = Vector3.zero;

        internal InputInfo(InputType type) => this.type = type;

        private void SetBoolTrue(InputAction.CallbackContext _) => Bool = true;
        private void SetBoolFalse(InputAction.CallbackContext _) => Bool = false;
        private void ReadToFloat(InputAction.CallbackContext c) => Float = c.ReadValue<float>();
        private void ReadToVector2(InputAction.CallbackContext c) => Vector2 = c.ReadValue<Vector2>();
        private void ReadToVector3(InputAction.CallbackContext c) => Vector3 = c.ReadValue<Vector3>();

        internal void Link(InputAction ia, bool doLink)
        {
            if (ia == null)
            {
                "InputAction is null. Cannot link/unlink.".LogError();
                return;
            }

            if (doLink)
            {
                switch (type)
                {
                    case InputType.Null:
                        break;

                    case InputType.Click:
                        ia.performed += SetBoolTrue;
                        break;

                    case InputType.Hold:
                        ia.performed += SetBoolTrue;
                        break;

                    case InputType.Value0:
                        ia.performed += SetBoolTrue;
                        ia.canceled += SetBoolFalse;
                        break;

                    case InputType.Value1:
                        ia.started += ReadToFloat;
                        ia.performed += ReadToFloat;
                        ia.canceled += ReadToFloat;
                        break;

                    case InputType.Value2:
                        ia.started += ReadToVector2;
                        ia.performed += ReadToVector2;
                        ia.canceled += ReadToVector2;
                        break;

                    case InputType.Value3:
                        ia.started += ReadToVector3;
                        ia.performed += ReadToVector3;
                        ia.canceled += ReadToVector3;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case InputType.Null:
                        break;

                    case InputType.Click:
                        ia.performed -= SetBoolTrue;
                        break;

                    case InputType.Hold:
                        ia.performed -= SetBoolTrue;
                        break;

                    case InputType.Value0:
                        ia.performed -= SetBoolTrue;
                        ia.canceled -= SetBoolFalse;
                        break;

                    case InputType.Value1:
                        ia.started -= ReadToFloat;
                        ia.performed -= ReadToFloat;
                        ia.canceled -= ReadToFloat;
                        break;

                    case InputType.Value2:
                        ia.started -= ReadToVector2;
                        ia.performed -= ReadToVector2;
                        ia.canceled -= ReadToVector2;
                        break;

                    case InputType.Value3:
                        ia.started -= ReadToVector3;
                        ia.performed -= ReadToVector3;
                        ia.canceled -= ReadToVector3;
                        break;

                    default:
                        break;
                }
            }
        }

        internal void ResetFlags()
        {
            if (type == InputType.Click && Bool) Bool = false;
            else if (type == InputType.Hold && Bool) Bool = false;
        }
    }

    internal abstract class AInputManager<T> : MonoBehaviour where T : AInputManager<T>
    {
        //TODO: ASingletonMonoBehaviour クラスと同じ内容
        #region Singleton
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
        #endregion

        private protected MyActions Source { get; private set; } = null;
        private List<(InputAction InputAction, InputInfo InputInfo)> inputList;

        private void Awake()
        {
            if (Instance == null)
            {
                $"Failed to initialize singleton instance of {typeof(T).Name}. Ensure that there is only one instance in the scene.".LogError();
                return;
            }

            Source = new();
            inputList = new(64);

            this.Init();

            foreach ((InputAction ia, InputInfo ii) in inputList)
                ii.Link(ia, true);
        }

        private void OnDestroy()
        {
            foreach ((InputAction ia, InputInfo ii) in inputList)
                ii.Link(ia, false);

            Source?.Dispose();
            Source = null;

            inputList = null;
        }

        private void OnEnable()
        {
            Source?.Enable();
            InputSystem.onBeforeUpdate += ResetFlags;
        }

        private void OnDisable()
        {
            Source?.Disable();
            InputSystem.onBeforeUpdate -= ResetFlags;
        }

        private void ResetFlags()
        {
            foreach ((_, InputInfo ii) in inputList)
                ii.ResetFlags();
        }

        private protected InputInfo Setup(InputAction inputAction, InputType type)
        {
            InputInfo info = new(type);
            inputList.Add((inputAction, info));
            return info;
        }

        private protected abstract void Init();
    }
}
