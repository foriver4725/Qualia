using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Hnx8.ReadJEnc;

namespace foriver4725.ScriptsValidator
{
    internal static class ScriptsValidator
    {
        internal static void Execute(IValidateOptions options)
        {
            var paths = SearchScripts(options);
            Debug.Log($"<color=#00ff00>{paths.Length} scripts found. Starting processing in order.</color>");

            // Count the number of files actually converted
            int validatedCount = 0;
            foreach (string path in paths)
            {
                string result = ConvertScripts(path, options);

                if (result != null)
                {
                    if (result.StartsWith("#error "))
                    {
                        Debug.Log(
                            $"<color=#00ff00>Processed</color> | <color=#00ffff>{result.Replace("#error ", string.Empty)}</color> | <color=#ff00ff>{Path.GetFileName(path)} ({path})</color>");
                    }
                    else
                    {
                        validatedCount++;
                        Debug.Log(
                            $"<color=#00ff00>Processed</color> | <color=#00ffff>{result}</color> | <color=#ff00ff>{Path.GetFileName(path)} ({path})</color>");
                    }
                }
            }

            Debug.Log(
                $"<color=#00ff00>Out of {paths.Length} scripts, {validatedCount} were actually modified.</color>");
        }

        // Searches for target script files and returns their absolute paths
        private static ReadOnlySpan<string> SearchScripts(IValidateSearchOptions options)
        {
            List<string> paths = new(1024);

            string root = options.Root;
            foreach (string pattern in options.Patterns)
            {
                paths.AddRange(Directory.GetFiles(root, pattern, SearchOption.AllDirectories));
            }

            // At this point: relative paths like `<root>/<...>.<ext>`
            // Replace `<root>` with `Application.dataPath`
            string rootParentAbsolutePath = Directory.GetParent(Application.dataPath)!.FullName;
            for (int i = 0; i < paths.Count; i++)
            {
                paths[i] = Path.Combine(rootParentAbsolutePath, paths[i]);
            }

            paths.TrimExcess();
            return paths.ToArray();
        }

        // Takes the absolute path of a script file and performs processing.
        // Returns null if no modification was made.
        // Returns a descriptive string if conversion occurred.
        // If an error occurs, returns an error message prefixed with "#error ".
        private static string ConvertScripts(string path, IValidateConvertOptions options)
        {
            CharCode originalCharCode = DetectCharCode(path);
            if (originalCharCode == null)
            {
                return "#error <color=#ff0000>This script could not be read! Skipping immediately.</color>";
            }

            List<string> results = new(4);

            // Encoding conversion
            if (options.CharCode != null)
            {
                if (originalCharCode.Name != options.CharCode.Name)
                {
                    string text = File.ReadAllText(path, originalCharCode.GetEncoding());
                    File.WriteAllText(path, text, options.CharCode.GetEncoding());

                    results.Add($"Encoding : {originalCharCode.Name} -> {options.CharCode.Name}");

                    originalCharCode = options.CharCode;
                }
            }

            // Line ending conversion
            if (options.Endline.Replacements != null)
            {
                string originalText = File.ReadAllText(path, originalCharCode.GetEncoding());

                string replacedText = originalText;
                foreach ((string @old, string @new) in options.Endline.Replacements)
                {
                    replacedText = replacedText.Replace(@old, @new);
                }

                File.WriteAllText(path, replacedText, originalCharCode.GetEncoding());

                if (replacedText != originalText)
                {
                    results.Add($"Line endings : -> {options.Endline.Name}");
                }
            }

            if (results.Count <= 0)
            {
                return null;
            }
            else
            {
                return string.Join(" ; ", results);
            }
        }

        private static CharCode DetectCharCode(string path)
        {
            FileInfo         file   = new(path);
            using FileReader reader = new(file);
            return reader.Read(file);
        }
    }
}
