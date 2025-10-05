namespace RyanMillerGameCore.Utilities
{
    using UnityEngine;

    /// <summary>
    /// Destroys a Rigidbody when it's velocity is below a certain threshold.
    /// </summary>
    public class RigidbodySleepDisabler : MonoBehaviour
    {
        [SerializeField] private float sleepThreshold = 0.01f;
        [SerializeField] private float initialDelay = 0.5f;
        [SerializeField] private bool removeRigidbody = true;
        [SerializeField] private bool removeCollider = true;
        
        private Rigidbody _rigidbody;
        private Collider _collider;
        private float _wakeTime; 
        
        private void Awake()
        {
            _wakeTime = Time.time;
            if (removeRigidbody)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }
            if (removeCollider)
            {
                _collider = GetComponent<Collider>();
            }
        }

        void FixedUpdate()
        {
            if (Time.time < _wakeTime + initialDelay)
            {
                return;
            }
            if (_rigidbody.linearVelocity.magnitude < sleepThreshold && 
                _rigidbody.angularVelocity.magnitude < sleepThreshold)
            {
                if (removeRigidbody)
                {
                    Destroy(_rigidbody);
                }
                if (removeCollider)
                {
                    Destroy(_collider);
                }
                Destroy(this); 
            }
        }
    }
}