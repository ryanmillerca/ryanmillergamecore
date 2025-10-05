namespace RyanMillerGameCore.Utilities
{
    using UnityEngine;

    public class FoldoutAttribute : PropertyAttribute
    {
        public string GroupName;

        public FoldoutAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}