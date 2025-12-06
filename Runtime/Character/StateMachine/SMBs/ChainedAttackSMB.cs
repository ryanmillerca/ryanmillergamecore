namespace RyanMillerGameCore.Character.SMB
{
    using UnityEngine;
    using UnityEngine.Animations;

    /// <summary>
    /// Listens to the CharacterBrain for Attack commands, but does not allow for input right away
    /// Used for subsequent/combo/chain attacks. Otherwise, similar to ListenforActionsSMB 
    /// </summary>
    public class ChainedAttackSMB : CharacterSMB
    {
        [SerializeField, Range(0,1)] private float inputReadyAt = 0.5f;
        private int _attackParamHash;
        private bool _listeningForAttack;

        protected override void OnCharacterStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _attackParamHash = Animator.StringToHash(referenceProvider.CharacterAnimParamMappings.m_ParamTriggerAttack);
            _listeningForAttack = false;
            base.OnCharacterStateEnter(animator, stateInfo, layerIndex);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,
            AnimatorControllerPlayable controller)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex, controller);
            if (_listeningForAttack)
            {
                return;
            }
            if (stateInfo.normalizedTime < inputReadyAt)
            {
                return;
            }
            _listeningForAttack = true;
            referenceProvider.CharacterBrain.OnAttackAction += AttackAction;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            referenceProvider.CharacterBrain.OnAttackAction -= AttackAction;
            base.OnStateExit(animator, stateInfo, layerIndex);
        }
        
        private void AttackAction()
        {
            animator.ResetTrigger(_attackParamHash);
            animator.SetTrigger(_attackParamHash);
        }
    }
}