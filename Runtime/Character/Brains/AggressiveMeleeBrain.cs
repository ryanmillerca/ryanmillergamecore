namespace RyanMillerGameCore.Character
{
    using UnityEngine;

    public class AggressiveMeleeBrain : CharacterBrain
    {
        private BrainSwitcher _brainSwitcher;
        
        protected override void OnEnable()
        {
            if (_brainSwitcher == null)
            {
                _brainSwitcher = GetComponent<BrainSwitcher>();
            }
            
            base.OnEnable();
            
            // subscribe to sensors
            if (characterReferences == null) {
                Debug.LogWarning("CharacterReferences is null",gameObject);
            }
            if (characterReferences._aggroColliderSensor == null) {
                Debug.LogWarning("characterReferences.aggroColliderSensor is null",gameObject);
            }
            characterReferences._aggroColliderSensor.ObjectEnteredSensor += OnObjectEnteredAggroColliderSensor;
            characterReferences._aggroColliderSensor.ObjectExitedSensor += OnObjectExitedAggroColliderSensor;
            characterReferences._attackColliderSensor.ObjectEnteredSensor += OnObjectEnteredAttackColliderSensor;
            characterReferences._attackColliderSensor.ObjectExitedSensor += OnObjectExitedAttackColliderSensor;

            // Check if the aggro sensor has any objects in it at the start
            Collider c = characterReferences._aggroColliderSensor.GetFirstItemInSensor();
            if (c)
            {
                OnObjectEnteredAggroColliderSensor(c);
            }
            
            // Check if the attack sensor has any objects in it at the start
            c = characterReferences._attackColliderSensor.GetFirstItemInSensor();
            if (c)
            {
                OnObjectEnteredAttackColliderSensor(c);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            // unsubscribe from sensors
            characterReferences._aggroColliderSensor.ObjectEnteredSensor -= OnObjectEnteredAggroColliderSensor;
            characterReferences._aggroColliderSensor.ObjectExitedSensor -= OnObjectExitedAggroColliderSensor;
            characterReferences._attackColliderSensor.ObjectEnteredSensor -= OnObjectEnteredAttackColliderSensor;
            characterReferences._attackColliderSensor.ObjectExitedSensor -= OnObjectExitedAttackColliderSensor;
            
            SetAggroTarget(null, false);
        }

        private void OnObjectEnteredAggroColliderSensor(Collider obj)
        {
            SetAggroTarget(obj.transform, true);
            Aggro();
        }
        
        private void OnObjectExitedAggroColliderSensor(Collider obj)
        {
            _target = null;
            SetAggroTarget(null, false);
        }
        
        private void OnObjectEnteredAttackColliderSensor(Collider obj)
        {
            Attack();
        }
        
        private void OnObjectExitedAttackColliderSensor(Collider obj)
        {
        }
        
        protected override void AggroTargetDied()
        {
            base.AggroTargetDied();
            _brainSwitcher.SwitchToBrain(0);
        }
    }
}