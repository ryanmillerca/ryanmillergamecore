namespace RyanMillerGameCore
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewID", menuName = "RyanMillerGameCore/ID/ID")]
    public class ID : ScriptableObject
    {
        public string entityName;
        public string prettyName;
    }

    public interface IHasID
    {
        public ID GetID();
    }
}