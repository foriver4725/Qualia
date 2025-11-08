using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEditor;
using Hnx8.ReadJEnc;

namespace foriver4725.ScriptsValidator
{
    internal sealed class ScriptsValidatorConfirmWindow : EditorWindow
    {
        private void OnGUI()
        {
            EditorGUILayout.Separator(); // ==================================================

            EditorGUILayout.LabelField("Confirm conversion?", EditorStyles.boldLabel);

            EditorGUILayout.Separator(); // ==================================================

            if (GUILayout.Button("Yes, convert now"))
            {
                ScriptsValidator.Execute(Parse(ScriptsValidatorWindow.CurrentSelections));
            }

            EditorGUILayout.Separator(); // ==================================================
        }

        private static IValidateOptions Parse(ScriptsValidatorWindow.ICurrentSelections inCurrentSelections)
        {
            ValidateOptionsInternal outValidateOptions = new();

            {
                var @in = inCurrentSelections.SrcRoot;
                var @out = @in switch
                {
                    ESrcRoot.Assets => "Assets/",
                    _               => throw new InvalidEnumArgumentException(null, (int)@in, typeof(ESrcRoot)),
                };
                outValidateOptions._Root = @out;
            }

            {
                var @in  = inCurrentSelections.SrcExtensions;
                var @out = new List<string>(16);
                {
                    if (@in.HasFlag(ESrcExtensions.Cs)) @out.Add("*.cs");
                    if (@in.HasFlag(ESrcExtensions.Shader)) @out.Add("*.shader");
                    if (@in.HasFlag(ESrcExtensions.Hlsl)) @out.Add("*.hlsl");
                    if (@in.HasFlag(ESrcExtensions.Txt)) @out.Add("*.txt");
                    if (@in.HasFlag(ESrcExtensions.Md)) @out.Add("*.md");
                }
                outValidateOptions._Patterns = @out;
            }

            {
                var @in = inCurrentSelections.DstEncoding;
                var @out = @in switch
                {
                    EDstEncoding.DontSpecify => null,
                    EDstEncoding.UTF8WithBOM => CharCode.UTF8,
                    EDstEncoding.UTF8WithoutBOM => CharCode.UTF8N,
                    EDstEncoding.ShiftJIS => CharCode.SJIS,
                    EDstEncoding.ASCII => CharCode.ASCII,
                    _ => throw new InvalidEnumArgumentException(null, (int)@in, typeof(EDstEncoding)),
                };
                outValidateOptions._CharCode = @out;
            }

            {
                var @in = inCurrentSelections.DstEndline;
                var @out = @in switch
                {
                    EDstEndline.DontSpecify => (null, string.Empty),
                    EDstEndline.LF => (new[]
                    {
                        ("\r\n", "\n"),
                        ("\r", "\n"),
                    }, "LF"),
                    EDstEndline.CRLF => (new[]
                    {
                        ("\r\n", "\n"),
                        ("\r", "\n"),
                        ("\n", "\r\n"),
                    }, "CRLF"),
                    _ => throw new InvalidEnumArgumentException(null, (int)@in, typeof(EDstEndline)),
                };
                outValidateOptions._Endline = @out;
            }

            return outValidateOptions;
        }

        private struct ValidateOptionsInternal : IValidateOptions
        {
            internal string _Root;
            internal IEnumerable<string> _Patterns;

            internal CharCode _CharCode;
            internal (IEnumerable<(string, string)>, string) _Endline;

            // ==================================================
            // Expose read-only interface properties
            public readonly string Root => _Root;
            public readonly IEnumerable<string> Patterns => _Patterns;

            public readonly CharCode CharCode => _CharCode;
            public readonly (IEnumerable<(string, string)>, string) Endline => _Endline;
        }
    }
}
