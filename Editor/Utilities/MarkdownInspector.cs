namespace RyanMillerGameCore.Editor
{
    using UnityEditor;
    using UnityEngine;
    using System.IO;
    using System.Text.RegularExpressions;

    [CustomEditor(typeof(TextAsset))]
    public class MarkdownInspector : Editor
    {
        private string markdownContent;
        private Vector2 scrollPosition;

        public override void OnInspectorGUI()
        {
            string path = AssetDatabase.GetAssetPath(target);
            if (Path.GetExtension(path).ToLower() != ".md")
            {
                base.OnInspectorGUI();
                return;
            }

            if (markdownContent == null)
            {
                markdownContent = File.ReadAllText(path);
                markdownContent = ParseMarkdown(markdownContent);
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUIStyle style = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                richText = true,
                normal =
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                } // set to white or black depending on theme
            };

            GUILayout.Label(markdownContent, style);
            EditorGUILayout.EndScrollView();
        }


        private string ParseMarkdown(string input)
        {
            // Headers
            input = Regex.Replace(input, @"^###### (.*)$", "<size=10><b>$1</b></size>", RegexOptions.Multiline);
            input = Regex.Replace(input, @"^##### (.*)$", "<size=11><b>$1</b></size>", RegexOptions.Multiline);
            input = Regex.Replace(input, @"^#### (.*)$", "<size=12><b>$1</b></size>", RegexOptions.Multiline);
            input = Regex.Replace(input, @"^### (.*)$", "<size=13><b>$1</b></size>", RegexOptions.Multiline);
            input = Regex.Replace(input, @"^## (.*)$", "<size=14><b>$1</b></size>", RegexOptions.Multiline);
            input = Regex.Replace(input, @"^# (.*)$", "<size=15><b>$1</b></size>", RegexOptions.Multiline);

            // Bold
            input = Regex.Replace(input, @"\*\*(.*?)\*\*", "<b>$1</b>");
            // Italic
            input = Regex.Replace(input, @"\*(.*?)\*", "<i>$1</i>");

            // Inline code
            input = Regex.Replace(input, @"(?<!`)`([^`\n]+)`(?!`)", "<color=#AAAAAA><b>$1</b></color>");

            // Line breaks
            input = input.Replace("\n", "\n\n");

            return input;
        }
    }
}