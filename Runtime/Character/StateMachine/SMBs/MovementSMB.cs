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
            if (referenceProvider == null) {
                Debug.Log($"References are null for {animator.gameObject.name}", animator.gameObject);
            }
            
            _paramHash = Animator.StringToHash(referenceProvider.CharacterAnimParamMappings.m_ParamSpeedHorizontal);
            base.OnCharacterStateEnter(animator, stateInfo, layerIndex);
            referenceProvider.Movement.OnVelocityApplied += OnVelocityApplied;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (referenceProvider == null) {
                return;
            }
            referenceProvider.Movement.OnVelocityApplied -= OnVelocityApplied;
            base.OnStateExit(animator, stateInfo, layerIndex);
        }
        
        private void OnVelocityApplied(float velocity)
        {
            animator.SetFloat(_paramHash, velocity);
        }
    }
}