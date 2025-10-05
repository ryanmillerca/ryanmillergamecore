namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;
    using System;

    public class SensorWatcher : MonoBehaviour
    {
        public Action<Collider> onDisabled;
        private Collider c;
        private bool _quitting = false;
        
        private void OnEnable()
        {
            c = GetComponent<Collider>();
            Application.quitting += ApplicationIsQuitting;
        }

        private void ApplicationIsQuitting()
        {
            _quitting = true;
        }

        private void OnDisable()
        {
            if (Application.isPlaying && _quitting == false && onDisabled != null)
            {
                onDisabled?.Invoke(c);
            }
            Application.quitting -= ApplicationIsQuitting;
        }
    }
}