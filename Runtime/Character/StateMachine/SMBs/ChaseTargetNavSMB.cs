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
            if (!References.characterPathfind)
            {
                Debug.LogError(References.character.gameObject + " has no pathfind.", References.character.gameObject);
                return;
            }

            References.characterPathfind.StartPath(References.characterBrain.Target);
            References.characterPathfind.PathCompleted += OnPathComplete;
            References.characterPathfind.PathFailed += OnPathFailedStuck;
            References.characterPathfind.PathTakingTooLong += OnPathTakingTooLong;
            References.attackColliderSensor.ObjectEnteredSensor += AttackColliderSensorOnObjectEnteredSensor;
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
            References.attackColliderSensor.ObjectEnteredSensor -= AttackColliderSensorOnObjectEnteredSensor;
            References.characterPathfind.PathCompleted -= OnPathComplete;
            References.characterPathfind.PathFailed -= OnPathFailedStuck;
            References.characterPathfind.PathTakingTooLong -= OnPathTakingTooLong;

            // stop movement
            References.movement.Move(Vector3.zero);
            References.movement.ApplyMovement();
        }
    }
}