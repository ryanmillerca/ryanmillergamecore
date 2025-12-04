namespace RyanMillerGameCore.SaveSystem
{
    using UnityEngine;

    public class KeyWatcher : MonoBehaviour
    {
        [SerializeField] private string watchKey = "someKey";
        
        [Header("Single Object")]
        [SerializeField] private GameObject objectIfKeyPresent;
        [SerializeField] private GameObject objectIfKeyAbsent;
        
        [Header("Multi Object")]
        [SerializeField] private GameObject[] objectsIfKeyPresent;
        [SerializeField] private GameObject[] objectsIfKeyAbsent;

        
        private void Start()
        {
            if (DataManager.Instance)
            {
                DataManager.Instance.SaveDataChanged += OnSaveDataChanged;
                UpdateGameObjects(); // Initial check
            }
        }

        private void OnDestroy()
        {
            if (DataManager.Instance)
            {
                DataManager.Instance.SaveDataChanged -= OnSaveDataChanged;
            }
        }

        private void OnSaveDataChanged()
        {
            UpdateGameObjects();
        }

        private void UpdateGameObjects()
        {
            bool keyFound = DataManager.Instance.HasKey(watchKey);
            if (objectIfKeyPresent) {
                objectIfKeyPresent.SetActive(keyFound);
            }
            if (objectIfKeyAbsent) {
                objectIfKeyAbsent.SetActive(!keyFound);
            }
            foreach (var obj in objectsIfKeyAbsent) {
                obj.SetActive(!keyFound);
            }
            foreach (var obj in objectsIfKeyPresent) {
                obj.SetActive(keyFound);
            }
        }
    }
}