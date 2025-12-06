namespace RyanMillerGameCore.Character.SMB
{
    using UnityEngine;

    public class ChaseTargetNavSMB : CharacterSMB
    {
        [SerializeField] private string attackTriggerParam = "attack";
        [SerializeField] private string hasAggroParam = "HasAggro";

        private int _attackTriggerHash;
        private int _hasAggroHash;

        protected override void OnCharacterStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnCharacterStateEnter(animator, stateInfo, layerIndex);
            _attackTriggerHash = Animator.StringToHash(attackTriggerParam);
            _hasAggroHash = Animator.StringToHash(hasAggroParam);

            // subscribe
            if (!References._characterPathfind)
            {
                Debug.LogError(References._character.gameObject + " has no pathfind.", References._character.gameObject);
                return;
            }

            References._characterPathfind.StartPath(References._characterBrain.Target);
            References._characterPathfind.PathCompleted += OnPathComplete;
            References._characterPathfind.PathFailed += OnPathFailedStuck;
            References._characterPathfind.PathTakingTooLong += OnPathTakingTooLong;
            References._attackColliderSensor.ObjectEnteredSensor += AttackColliderSensorOnObjectEnteredSensor;
        }

        private void AttackColliderSensorOnObjectEnteredSensor(Collider obj)
        {
            animator.SetTrigger(_attackTriggerHash);
        }

        private void OnPathTakingTooLong(float arg1, float arg2)
        {
            animator.SetBool(_hasAggroHash, false);
        }

        private void OnPathFailedStuck(string obj)
        {
            animator.SetBool(_hasAggroHash, false);
        }

        private void OnPathComplete(float obj)
        {
            animator.SetTrigger(_attackTriggerHash);
        }

        protected override void OnCharacterStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnCharacterStateExit(animator, stateInfo, layerIndex);

            // unsubscribe 
            References._attackColliderSensor.ObjectEnteredSensor -= AttackColliderSensorOnObjectEnteredSensor;
            References._characterPathfind.PathCompleted -= OnPathComplete;
            References._characterPathfind.PathFailed -= OnPathFailedStuck;
            References._characterPathfind.PathTakingTooLong -= OnPathTakingTooLong;

            // stop movement
            References._movement.Move(Vector3.zero);
            References._movement.ApplyMovement();
        }
    }
}