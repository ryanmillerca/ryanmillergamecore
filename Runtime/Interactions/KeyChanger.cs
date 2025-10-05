namespace RyanMillerGameCore.Interactions
{
    using SaveSystem;
    using UnityEngine;
    using UnityEngine.Events;

    public class KeyChanger : MonoBehaviour
    {
        [SerializeField] private string keyToChange;
        [SerializeField] private bool keyValue;
        
        private Interactive _interactive;
        
        private void ChangeKey()
        {
            if (keyValue)
            {
                DataManager.Instance.AddKey(keyToChange);
            }
            else
            {
                DataManager.Instance.RemoveKey(keyToChange);
            }
            _interactive.InteractionWasCompleted(this);
            this.enabled = false;
        }
        
        private void OnEnable()
        {
            _interactive = GetComponent<Interactive>();
            if (_interactive == null)
            {
                _interactive = gameObject.AddComponent<Interactive>();
            }
            if (_interactive.OnInteract == null)
            {
                _interactive.OnInteract = new UnityEvent();
            }
            _interactive.OnInteract.AddListener(ChangeKey);
        }

        private void OnDisable()
        {
            if (_interactive != null)
            {
                _interactive.OnInteract.RemoveListener(ChangeKey);
            }
        }
    }
}