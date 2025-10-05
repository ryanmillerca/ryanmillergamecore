using UnityEngine;

/// <summary>
/// Shows the field in the inspector only if the specified field equals the given value.
/// Works with bools and enums (as ints). Safe to use in runtime scripts.
/// </summary>
public class ShowIfAttribute : PropertyAttribute
{
    public string ConditionFieldName { get; private set; }
    public int CompareIntValue { get; private set; }

    public ShowIfAttribute(string conditionFieldName, int compareValue)
    {
        ConditionFieldName = conditionFieldName;
        CompareIntValue = compareValue;
    }

    public ShowIfAttribute(string conditionFieldName, bool compareValue)
    {
        ConditionFieldName = conditionFieldName;
        CompareIntValue = compareValue ? 1 : 0;
    }
}
