namespace RyanMillerGameCore.Utilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;

    public class CollisionHandler : MonoBehaviour
    {
        [Foldout("Unity Events"), SerializeField] private UnityEvent<Collider> collisionEnter;
        [Foldout("Unity Events"), SerializeField] private UnityEvent<Collider> collisionExit;
        [Foldout("Unity Events"), SerializeField] private UnityEvent<Collider> triggerEnter;
        [Foldout("Unity Events"), SerializeField] private UnityEvent<Collider> triggerExit;
        [Foldout("Unity Events"), SerializeField] private UnityEvent<Vector3> slammed;
        
        [SerializeField] private float slamMultiplier = 0.5f;

        [Header("Params"), Tooltip("The relative force of a collision that is considered a Slam")] [SerializeField]
        private float slamForce = 10f;

        public event Action<Collision> CollisionEnter;
        public event Action<Collision> CollisionExit;
        public event Action<Collider> TriggerEnter;
        public event Action<Collider> TriggerExit;
        public event Action<Vector3> Slammed;

        private void OnCollisionEnter(Collision collision)
        {
            collisionEnter?.Invoke(collision.collider);
            CollisionEnter?.Invoke(collision);
            
            if (collision.relativeVelocity.magnitude > slamForce)
            {
                Slammed?.Invoke(collision.relativeVelocity * slamMultiplier);
                slammed?.Invoke(collision.relativeVelocity * slamMultiplier);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            collisionExit?.Invoke(collision.collider);
            CollisionExit?.Invoke(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            triggerEnter?.Invoke(other);
            TriggerEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            triggerExit.Invoke(other);
            TriggerExit?.Invoke(other);
        }
    }
}