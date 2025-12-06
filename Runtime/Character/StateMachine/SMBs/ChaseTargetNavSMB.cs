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
            if (!referenceProvider.CharacterPathfind)
            {
                Debug.LogError(referenceProvider.Character.Transform.gameObject + " has no pathfind.", referenceProvider.Character.Transform.gameObject);
                return;
            }

            referenceProvider.CharacterPathfind.StartPath(referenceProvider.CharacterBrain.Target);
            referenceProvider.CharacterPathfind.PathCompleted += OnPathComplete;
            referenceProvider.CharacterPathfind.PathFailed += OnPathFailedStuck;
            referenceProvider.CharacterPathfind.PathTakingTooLong += OnPathTakingTooLong;
            referenceProvider.AttackColliderSensor.ObjectEnteredSensor += AttackColliderSensorOnObjectEnteredSensor;
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
            referenceProvider.AttackColliderSensor.ObjectEnteredSensor -= AttackColliderSensorOnObjectEnteredSensor;
            referenceProvider.CharacterPathfind.PathCompleted -= OnPathComplete;
            referenceProvider.CharacterPathfind.PathFailed -= OnPathFailedStuck;
            referenceProvider.CharacterPathfind.PathTakingTooLong -= OnPathTakingTooLong;

            // stop movement
            referenceProvider.Movement.Move(Vector3.zero);
            referenceProvider.Movement.ApplyMovement();
        }
    }
}