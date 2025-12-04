namespace RyanMillerGameCore.Character
{
    using System;
    using Interactions;
    using UnityEngine;

    [RequireComponent(typeof(ColliderSensor))]
    public class DamageDealer : MonoBehaviour
    {
        [SerializeField] private float damage = 1;
        [SerializeField] private float knockBackStrength;
        [SerializeField] private int maxTargets = 0;

        [NonSerialized] private ColliderSensor _colliderSensor;
        [NonSerialized] private Character _character;
        
        private float DamageDealt
        {
            get { return damage; }
        }

        public void DealDamage(GameObject sourceObject, float damageAmount = -1)
        {
            if (damageAmount < 0)
            {
                damageAmount = damage;
            }

            Collider[] collidersInSensor = _colliderSensor.GetCollidersAsArray();
            int numTargets = maxTargets == 0 ? collidersInSensor.Length : maxTargets;
            for (var index = 0; index < numTargets; index++)
            {
                var c = collidersInSensor[index];
                if (!c)
                {
                    continue;
                }
                if (c.transform != transform)
                {
                    DealDamageTo(c, sourceObject.transform.position, damageAmount);
                }
            }
        }

        private void DealDamageTo(Collider c, Vector3 sourcePosition, float damageAmount = -1)
        {
            if (damageAmount < 0)
            {
                damageAmount = damage;
            }

            Vector3 knockBackValue = transform.position - sourcePosition;
            knockBackValue.y = 0;
            knockBackValue.Normalize();
            knockBackValue *= knockBackStrength;

            ITakesDamage damageTaker = c.GetComponentInParent<ITakesDamage>();
            if (damageTaker != null)
            {
                if (damageTaker.CanReceiveDamage())
                {
                    if (knockBackStrength > 0)
                    {
                        damageTaker.ReceiveKnockback(knockBackValue);
                    }
                    damageTaker.ReceiveDamage(DamageDealt, _character);
                }
            }
        }

        private void Awake()
        {
            _colliderSensor = GetComponent<ColliderSensor>();
            _character = GetComponentInParent<Character>();
        }
    }
}