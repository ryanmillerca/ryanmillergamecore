namespace RyanMillerGameCore.Character.SMB
{
    using UnityEngine;
    using Interactions;

    /// <summary>
    /// Listens for Attack or Interact actions from the CharacterBrain
    /// and triggers the corresponding animator trigger.
    /// </summary>
    public class ListenForActionsSMB : CharacterSMB
    {
        [SerializeField] private bool listenForAttack = true;
        [SerializeField] private bool listenForInteract = true;
        
        private int _attackParamHash;
        private int _interactParamHash;

        protected override void OnCharacterStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _attackParamHash = Animator.StringToHash(References.paramTriggerAttack);
            _interactParamHash = Animator.StringToHash(References.paramTriggerInteract);
            base.OnCharacterStateEnter(animator, stateInfo, layerIndex);
            if (References.characterBrain) {
                References.characterBrain.OnAttackAction += AttackAction;
                References.characterBrain.OnInteractAction += InteractAction;
            }
        }
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (References == null) {
                return;
            }
            if (References.characterBrain) {
                References.characterBrain.OnAttackAction -= AttackAction;
                References.characterBrain.OnInteractAction -= InteractAction;
            }
            base.OnStateExit(animator, stateInfo, layerIndex);
        }
        
        private void AttackAction()
        {
            if (listenForAttack == false)
            {
                return;
            }
            animator.ResetTrigger(_attackParamHash);
            animator.SetTrigger(_attackParamHash);
        }
        
        private void InteractAction()
        {
            if (listenForInteract == false)
            {
                return;
            }
            animator.ResetTrigger(_interactParamHash);
            animator.SetTrigger(_interactParamHash);
            Interactive interactive = References.interactColliderSensor.CurrentInteractive; 
            if (interactive)
            {
                interactive.Interact(References.character);
            }
        }
    }
}