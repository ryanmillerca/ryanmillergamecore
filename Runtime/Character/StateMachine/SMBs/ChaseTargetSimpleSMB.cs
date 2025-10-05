namespace RyanMillerGameCore.Character.SMB
{
    using UnityEngine;

    public class ChaseTargetSimpleSMB : CharacterSMB
    {
        protected override void OnCharacterStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (References.characterBrain.Target != null)
            {
                // Get the target position
                Vector3 targetPosition = References.characterBrain.Target.position;

                // Calculate the direction to the target
                Vector3 directionToTarget = (targetPosition - animator.transform.position).normalized;

                // Set the character's velocity towards the target
                References.movement.Move(directionToTarget);
                References.movement.ApplyMovement();
            }
        }
    }
}