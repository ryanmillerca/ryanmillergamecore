namespace RyanMillerGameCore.Utilities
{
    using UnityEngine;
    using System;

    public class EnableDisabler : MonoBehaviour
    {
        public event Action Completed;
        
        
        [SerializeField] private GameObject[] enableThese;
        [SerializeField] private GameObject[] disableThese;
        [SerializeField] private bool startEnabled = true;
        
        public void DoIt()
        {
            foreach (GameObject go in enableThese)
            {
                go.SetActive(true);
            }

            foreach (GameObject go in disableThese)
            {
                go.SetActive(false);
            }
            
            Completed?.Invoke();
        }

        private void Start()
        {
            if (startEnabled)
            {
                DoIt();
            }
        }
    }
}