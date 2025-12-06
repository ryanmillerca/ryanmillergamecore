namespace RyanMillerGameCore.Character.SMB
{
    using UnityEngine;

    public class ChaseTargetSimpleSMB : CharacterSMB
    {
        protected override void OnCharacterStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (referenceProvider.CharacterBrain.Target != null)
            {
                // Get the target position
                Vector3 targetPosition = referenceProvider.CharacterBrain.Target.position;

                // Calculate the direction to the target
                Vector3 directionToTarget = (targetPosition - animator.transform.position).normalized;

                // Set the character's velocity towards the target
                referenceProvider.Movement.Move(directionToTarget);
                referenceProvider.Movement.ApplyMovement();
            }
        }
    }
}