namespace RyanMillerGameCore.Character.SMB
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Field)]
    public class ExposeFromCharacterReferencesAttribute : PropertyAttribute
    {
        public string ReferenceFieldName;

        public ExposeFromCharacterReferencesAttribute(string referenceFieldName)
        {
            ReferenceFieldName = referenceFieldName;
        }
    }
}