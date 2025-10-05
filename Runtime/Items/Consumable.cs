namespace RyanMillerGameCore.Items
{
    using System;
    using UnityEngine;
    using Character; 

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AutoRegisterID))]
    public class Consumable : Item, ITakesDamage, IHasID
    {
        [SerializeField] private float maxHealth = 1f;
        [SerializeField] private float healAmount = 2f;

        public event Action Died;
        public event Action Spawned;

        private float _currentHealth;
        private bool _isDestroyed;

        private void OnEnable()
        {
            ResetState();
            Spawned?.Invoke();
        }

        public bool CanReceiveDamage()
        {
            return !_isDestroyed;
        }

        public bool ReceiveDamage(float damageAmount, Component attacker = null)
        {
            if (!CanReceiveDamage())
            {
                return false;
            }

            _currentHealth -= damageAmount;

            if (_currentHealth <= 0f)
            {
                HandleDeath(attacker);
            }

            return true;
        }

        public bool ReceiveKnockback(Vector3 direction)
        {
            // No knockback behavior
            return false;
        }

        private void HandleDeath(Component attacker)
        {
            _isDestroyed = true;
            Died?.Invoke();

            if (healAmount > 0)
            {
                // Null check before attempting to cast and heal
                if (attacker != null && attacker is Character character && character.CanReceiveHealing())
                {
                    character.ReceiveHeal(healAmount);
                }
            }

            ItemManager.Instance?.ReturnToPool(gameObject);
        }

        public override void OnAcquire()
        {
            base.OnAcquire();
            ResetState();
            Spawned?.Invoke();
        }

        public override void OnRelease()
        {
            base.OnRelease();
            ResetState();
        }

        private void ResetState()
        {
            _currentHealth = maxHealth;
            _isDestroyed = false;
        }
    }
}