using System;
using UnityEngine;
using UnityEditor;

namespace foriver4725.ScriptsValidator
{
    internal sealed class ScriptsValidatorWindow : EditorWindow
    {
        #region Public Interface

        internal interface ICurrentSelections
        {
            ESrcRoot SrcRoot { get; }
            ESrcExtensions SrcExtensions { get; }
            EDstEncoding DstEncoding { get; }
            EDstEndline DstEndline { get; }
        }

        private struct CurrentSelectionsInternal : ICurrentSelections
        {
            internal ESrcRoot _SrcRoot;
            internal ESrcExtensions _SrcExtensions;
            internal EDstEncoding _DstEncoding;
            internal EDstEndline _DstEndline;

            public readonly ESrcRoot SrcRoot => _SrcRoot;
            public readonly ESrcExtensions SrcExtensions => _SrcExtensions;
            public readonly EDstEncoding DstEncoding => _DstEncoding;
            public readonly EDstEndline DstEndline => _DstEndline;
        }

        private static CurrentSelectionsInternal _currentSelections = new();
        internal static ICurrentSelections CurrentSelections => _currentSelections;

        #endregion

        private Rect _dropDownButtonRect;

        [MenuItem("Tools/Scripts Validator (Open Window)")]
        private static void CreateAndShow()
        {
            var window = CreateInstance<ScriptsValidatorWindow>();
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.Separator(); // ==================================================

            EditorGUILayout.LabelField("Scripts Validator", EditorStyles.boldLabel);

            EditorGUILayout.Separator(); // ==================================================

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            {
                EnumPopup("Root Directory", ref _currentSelections._SrcRoot);
                EnumFlagsField("Target Script Types", ref _currentSelections._SrcExtensions);
                EnumPopup("Destination Encoding", ref _currentSelections._DstEncoding);
                EnumPopup("Destination Line Endings", ref _currentSelections._DstEndline);
            }

            EditorGUILayout.Separator(); // ==================================================

            if (GUILayout.Button("Run Conversion"))
            {
                var screenPoint = GUIUtility.GUIToScreenPoint(_dropDownButtonRect.center);
                var window      = CreateInstance<ScriptsValidatorConfirmWindow>();
                window.ShowAsDropDown(new(screenPoint, Vector2.zero), new(200, 100));
            }

            if (Event.current.type == EventType.Repaint)
            {
                _dropDownButtonRect = GUILayoutUtility.GetLastRect();
            }

            EditorGUILayout.Separator(); // ==================================================
        }

        private static void EnumPopup<TEnum>(string label, ref TEnum value)
            where TEnum : Enum
            => value = (TEnum)EditorGUILayout.EnumPopup(label, value);

        private static void EnumFlagsField<TEnum>(string label, ref TEnum value)
            where TEnum : Enum
            => value = (TEnum)EditorGUILayout.EnumFlagsField(label, value);
    }
}
