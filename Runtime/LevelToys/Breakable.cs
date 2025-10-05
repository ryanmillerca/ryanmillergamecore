namespace RyanMillerGameCore.LevelToys
{
    using System;
    using Random = UnityEngine.Random;
    using Character;
    using UnityEngine;
    using UnityEngine.Events;
    using Items;
    using Utilities;

    public class Breakable : MonoBehaviour, ITakesDamage
    {
        #pragma warning disable CS0067
        public event Action Died;
        public event Action Spawned;

        [SerializeField] private bool isBroken = false;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _collider;
        [SerializeField] private float maxDurability = 1;
        [SerializeField] private UnityEvent OnBroken;
        [SerializeField] private Rigidbody[] passOnForceToThese;
        [SerializeField] private bool unparentForceTakers = true;
        [SerializeField] private float randomAngularForce = 100;
        [SerializeField] private WeightedIDTable dropTable;
        private EnableDisabler _enableDisabler;
        private float currentDurability;
        private Vector3 _storedVelocity;
        
        private void Awake()
        {
            _enableDisabler = GetComponent<EnableDisabler>();
            if (_enableDisabler)
            {
                _enableDisabler.Completed += EnableDisablerCompleted;
            }
            currentDurability = maxDurability;
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }

        private void OnDestroy()
        {
            if (_enableDisabler)
            {
                _enableDisabler.Completed -= EnableDisablerCompleted;
            }
        }

        private void EnableDisablerCompleted()
        {
            if (unparentForceTakers)
            {
                foreach (Rigidbody rb in passOnForceToThese)
                {
                    rb.transform.SetParent(null);
                }
            }
            foreach (Rigidbody rb in passOnForceToThese)
            {
                rb.AddForce(_storedVelocity, ForceMode.Impulse);
            }
        }

        public bool CanReceiveDamage()
        {
            return !isBroken;
        }

        public bool ReceiveDamage(float damageAmount, Component attacker = null)
        {
            if (isBroken)
            {
                return false;
            }
            currentDurability -= damageAmount;
            if (currentDurability <= 0)
            {
                Break();
            }

            return true;
        }

        private void Break()
        {
            isBroken = true;

            if (_collider)
            {
                _collider.enabled = false;
            }
            if (_rigidbody)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }
            
            OnBroken?.Invoke();

            DropItem();
            
        }

        private void DropItem()
        {
            ID dropID = null;
            if (dropTable != null)
            {
                dropID = dropTable.GetRandomID();
            }
            if (dropID != null && ItemManager.Instance)
            {
                GameObject item = ItemManager.Instance.GetItem(dropID);
                item.transform.position = transform.position;
                item.transform.rotation = Quaternion.identity;
            }
        }

        public bool ReceiveKnockback(Vector3 direction)
        {
            _storedVelocity = direction;
            if (_rigidbody)
            {
                _rigidbody.AddForce(direction, ForceMode.Impulse);
                return true;
            }
            if (randomAngularForce > 0)
            {
                _rigidbody.AddTorque(Random.Range(-randomAngularForce,randomAngularForce), 
                    Random.Range(-randomAngularForce,randomAngularForce), 
                    Random.Range(-randomAngularForce,randomAngularForce), ForceMode.Impulse);
            }
            return false;
        }
    }
}