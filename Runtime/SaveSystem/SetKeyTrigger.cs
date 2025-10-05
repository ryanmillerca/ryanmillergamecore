namespace RyanMillerGameCore.SaveSystem
{
    using UnityEngine;

    public class SetKeyTrigger : MonoBehaviour
    {
        [SerializeField] private string key;
        [SerializeField] private bool allowDuplicateEntries = false;
        
        public void SetKey()
        {
            DataManager.Instance.AddKey(key,allowDuplicateEntries);
        }
    }
}