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

    internal static partial class InputManager
    {
        private static MyActions source;
        private static List<(InputAction InputAction, InputInfo InputInfo)> inputList;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            source = new();
            inputList = new(64);

            source?.Enable();
            Bind();
            InputSystem.onBeforeUpdate += ResetFlags;

            foreach ((InputAction ia, InputInfo ii) in inputList)
                ii.Link(ia, true);

            Application.quitting += Dispose;
        }

        private static void Dispose()
        {
            foreach ((InputAction ia, InputInfo ii) in inputList)
                ii.Link(ia, false);

            InputSystem.onBeforeUpdate -= ResetFlags;
            source?.Disable();
            source?.Dispose();
            source = null;
            inputList = null;
        }

        private static void ResetFlags()
        {
            foreach ((_, InputInfo ii) in inputList)
                ii.ResetFlags();
        }

        private static InputInfo Create(InputAction inputAction, InputType type)
        {
            InputInfo info = new(type);
            inputList.Add((inputAction, info));
            return info;
        }
    }
}
