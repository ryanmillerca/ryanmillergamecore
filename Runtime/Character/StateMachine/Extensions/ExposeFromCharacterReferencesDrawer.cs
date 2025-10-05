#if UNITY_EDITOR
namespace RyanMillerGameCore.Character.SMB
{
    using UnityEditor;
    using UnityEngine;
    using System.Reflection;

    [CustomPropertyDrawer(typeof(ExposeFromCharacterReferencesAttribute))]
    public class ExposeFromCharacterReferencesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (ExposeFromCharacterReferencesAttribute)attribute;
            var smb = property.serializedObject.targetObject as StateMachineBehaviour;

            if (smb == null)
            {
                EditorGUI.LabelField(position, label.text, "Not an SMB");
                return;
            }

            var animator = AnimatorExtensions.FindAnimatorUsingSerializedObject(property.serializedObject);
            if (animator == null)
            {
                EditorGUI.LabelField(position, label.text, "No Animator found");
                return;
            }

            var characterReferences = animator.GetComponent<CharacterReferences>();
            if (characterReferences == null)
            {
                EditorGUI.LabelField(position, label.text, "No CharacterReferences found");
                return;
            }

            var field = typeof(CharacterReferences).GetField(attr.ReferenceFieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                EditorGUI.LabelField(position, label.text, $"Field '{attr.ReferenceFieldName}' not found");
                return;
            }

            var value = field.GetValue(characterReferences);

            EditorGUI.BeginChangeCheck();

            if (value is float floatVal)
            {
                float newVal = EditorGUI.FloatField(position, label, floatVal);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(characterReferences, "Modify CharacterReferences field");
                    field.SetValue(characterReferences, newVal);
                    EditorUtility.SetDirty(characterReferences);
                }
            }
            else if (value is int intVal)
            {
                int newVal = EditorGUI.IntField(position, label, intVal);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(characterReferences, "Modify CharacterReferences field");
                    field.SetValue(characterReferences, newVal);
                    EditorUtility.SetDirty(characterReferences);
                }
            }
            else if (value is bool boolVal)
            {
                bool newVal = EditorGUI.Toggle(position, label, boolVal);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(characterReferences, "Modify CharacterReferences field");
                    field.SetValue(characterReferences, newVal);
                    EditorUtility.SetDirty(characterReferences);
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, $"Unsupported type: {value?.GetType().Name ?? "null"}");
            }
        }
    }
}
#endif