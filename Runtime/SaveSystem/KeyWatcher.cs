namespace RyanMillerGameCore.SaveSystem
{
    using UnityEngine;

    public class KeyWatcher : MonoBehaviour
    {
        [SerializeField] private string watchKey = "someKey";
        [SerializeField] private GameObject objectIfKeyPresent;
        [SerializeField] private GameObject objectIfKeyAbsent;

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
            objectIfKeyPresent.SetActive(keyFound);
            objectIfKeyAbsent.SetActive(!keyFound);
        }
    }
}