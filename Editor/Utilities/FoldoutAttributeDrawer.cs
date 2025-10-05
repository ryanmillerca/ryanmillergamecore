namespace RyanMillerGameCore.Utilities
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Reflection;

    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class FoldoutGroupDrawer : Editor
    {
        private static Dictionary<string, bool> foldoutStates = new();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Dictionary<string, List<SerializedProperty>> grouped = new();
            List<SerializedProperty> ungrouped = new();

            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (prop.name == "m_Script")
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(prop, true);
                    EditorGUI.EndDisabledGroup();
                    continue;
                }

                var attr = GetFoldoutAttribute(prop);
                if (attr != null)
                {
                    if (!grouped.ContainsKey(attr.GroupName))
                        grouped[attr.GroupName] = new();

                    grouped[attr.GroupName].Add(prop.Copy());
                }
                else
                {
                    ungrouped.Add(prop.Copy());
                }
            }

            // Draw ungrouped first
            foreach (var propItem in ungrouped)
            {
                EditorGUILayout.PropertyField(propItem, true);
            }

            // Then draw grouped foldouts
            foreach (var kvp in grouped)
            {
                string group = kvp.Key;
                List<SerializedProperty> props = kvp.Value;

                if (!foldoutStates.ContainsKey(group))
                    foldoutStates[group] = false; // Collapsed by default

                foldoutStates[group] = EditorGUILayout.Foldout(foldoutStates[group], group, true);

                if (foldoutStates[group])
                {
                    EditorGUI.indentLevel++;
                    foreach (var p in props)
                    {
                        EditorGUILayout.PropertyField(p, true);
                    }

                    EditorGUI.indentLevel--;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private FoldoutAttribute GetFoldoutAttribute(SerializedProperty property)
        {
            var targetObj = serializedObject.targetObject;
            var type = targetObj.GetType();

            FieldInfo field = null;
            while (type != null)
            {
                field = type.GetField(property.name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                    break;

                type = type.BaseType;
            }

            if (field == null)
                return null;

            return (FoldoutAttribute)System.Attribute.GetCustomAttribute(field, typeof(FoldoutAttribute));
        }
    }
}