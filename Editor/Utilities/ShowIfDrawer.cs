using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var showIf = (ShowIfAttribute)attribute;
        var conditionProperty = property.serializedObject.FindProperty(showIf.ConditionFieldName);

        if (ShouldShow(conditionProperty, showIf))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var showIf = (ShowIfAttribute)attribute;
        var conditionProperty = property.serializedObject.FindProperty(showIf.ConditionFieldName);

        return ShouldShow(conditionProperty, showIf)
            ? EditorGUI.GetPropertyHeight(property, label, true)
            : 0;
    }

    private bool ShouldShow(SerializedProperty conditionProperty, ShowIfAttribute showIf)
    {
        if (conditionProperty == null) return false;

        switch (conditionProperty.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return (conditionProperty.boolValue ? 1 : 0) == showIf.CompareIntValue;
            case SerializedPropertyType.Enum:
                return conditionProperty.enumValueIndex == showIf.CompareIntValue;
            default:
                return false;
        }
    }
}
