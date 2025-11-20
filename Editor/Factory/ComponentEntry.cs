using UnityEngine;
using System;

namespace RyanMillerGameCore.Factory
{
    [Serializable]
    public class ComponentEntry
    {
        [Tooltip("Component type name (e.g., Rigidbody, BoxCollider, MyNamespace.MyScript)")]
        public string typeName;

        [Tooltip("Optional property overrides. Format: 'propertyName=value'. Supports int, float, bool, string, Color (#RRGGBB), Vector3 (x,y,z), Enum, Component/GameObject names")]
        public string[] propertyOverrides;

        public Type GetTypeSafe()
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            Type t = Type.GetType(typeName); // try default
            if (t != null) return t;

            // Try UnityEngine namespace for common built-ins
            t = Type.GetType("UnityEngine." + typeName + ", UnityEngine");
            if (t != null) return t;

            // Try your main assembly
            t = Type.GetType(typeName + ", " + typeof(ComponentEntry).Assembly.GetName().Name);
            if (t != null) return t;

            Debug.LogWarning($"Type '{typeName}' not found.");
            return null;
        }

        public bool IsValid()
        {
            var t = GetTypeSafe();
            return t != null && typeof(Component).IsAssignableFrom(t);
        }

        /// <summary>
        /// Applies serialized property overrides to a component instance.
        /// </summary>
        public void ApplyProperties(Component component)
        {
            if (component == null || propertyOverrides == null) return;

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(component);

            foreach (var entry in propertyOverrides)
            {
                if (string.IsNullOrEmpty(entry)) continue;

                int splitIndex = entry.IndexOf('=');
                if (splitIndex <= 0 || splitIndex >= entry.Length - 1) continue;

                string propName = entry.Substring(0, splitIndex).Trim();
                string valueStr = entry.Substring(splitIndex + 1).Trim();

                UnityEditor.SerializedProperty prop = so.FindProperty(propName);
                if (prop == null)
                {
                    Debug.LogWarning($"Property '{propName}' not found on component {component.GetType().Name}");
                    continue;
                }

                try
                {
                    switch (prop.propertyType)
                    {
                        case UnityEditor.SerializedPropertyType.Integer:
                            prop.intValue = int.Parse(valueStr);
                            break;
                        case UnityEditor.SerializedPropertyType.Float:
                            prop.floatValue = float.Parse(valueStr);
                            break;
                        case UnityEditor.SerializedPropertyType.Boolean:
                            prop.boolValue = bool.Parse(valueStr);
                            break;
                        case UnityEditor.SerializedPropertyType.String:
                            prop.stringValue = valueStr;
                            break;
                        case UnityEditor.SerializedPropertyType.Color:
                            if (ColorUtility.TryParseHtmlString(valueStr, out Color c))
                                prop.colorValue = c;
                            break;
                        case UnityEditor.SerializedPropertyType.Vector3:
                            string[] parts = valueStr.Split(',');
                            if (parts.Length == 3)
                                prop.vector3Value = new Vector3(
                                    float.Parse(parts[0]),
                                    float.Parse(parts[1]),
                                    float.Parse(parts[2]));
                            break;
                        case UnityEditor.SerializedPropertyType.Enum:
                            int enumIndex = Array.IndexOf(Enum.GetNames(component.GetType()), valueStr);
                            if (enumIndex >= 0) prop.enumValueIndex = enumIndex;
                            break;
                        case UnityEditor.SerializedPropertyType.ObjectReference:
                            // Try to find GameObject by name for Component/Object references
                            UnityEngine.Object obj = null;
                            GameObject go = GameObject.Find(valueStr);
                            if (go != null && typeof(Component).IsAssignableFrom(prop.objectReferenceValue?.GetType() ?? typeof(Component)))
                                obj = go.GetComponent(prop.objectReferenceValue?.GetType() ?? typeof(Component));
                            prop.objectReferenceValue = obj;
                            break;
                        default:
                            Debug.LogWarning($"Unsupported property type {prop.propertyType} for {propName}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to set property '{propName}' on {component.GetType().Name}: {e.Message}");
                }
            }

            so.ApplyModifiedProperties();
        }
    }
}