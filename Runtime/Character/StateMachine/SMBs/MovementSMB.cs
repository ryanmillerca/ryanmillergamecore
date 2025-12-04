namespace RyanMillerGameCore.Character.SMB
{
    using UnityEngine;

    /// <summary>
    /// Allows movement to be applied to the character (connects Update to the animator).
    /// and updates the animator param to drive the animation via float param
    /// </summary>
    public class MovementSMB : CharacterSMB
    {
        private int _paramHash;
        
        protected override void OnCharacterStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (References == null) {
                Debug.Log($"References are null for {animator.gameObject.name}", animator.gameObject);
            }
            
            _paramHash = Animator.StringToHash(References.paramSpeedHorizontal);
            base.OnCharacterStateEnter(animator, stateInfo, layerIndex);
            References.movement.OnVelocityApplied += OnVelocityApplied;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (References == null) {
                return;
            }
            References.movement.OnVelocityApplied -= OnVelocityApplied;
            base.OnStateExit(animator, stateInfo, layerIndex);
        }
        
        private void OnVelocityApplied(float velocity)
        {
            animator.SetFloat(_paramHash, velocity);
        }
    }
}