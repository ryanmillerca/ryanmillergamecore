namespace RyanMillerGameCore.Character.SMB
{
    using UnityEngine;
    using UnityEngine.Animations;

    /// <summary>
    /// Controls the character's ability to move from a State Machine Behaviour (SMB).
    /// (Sets the CharacterMovement.CanMove property to true or false)
    /// </summary>
    public class SetPlayerActionsEnabledSMB : CharacterSMB
    {
        [SerializeField] private Ternary canMoveOnEnter;
        [SerializeField] private Ternary canMoveOnExit;
        [SerializeField] private Ternary canInteractOnEnter;
        [SerializeField] private Ternary canInteractOnExit;

        protected override void OnCharacterStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnCharacterStateEnter(animator,stateInfo,layerIndex);
            if (canMoveOnEnter != Ternary.Undefined)
            {
                referenceProvider.Movement.CanMove(canMoveOnEnter == Ternary.True);
            }
            if (canInteractOnEnter != Ternary.Undefined)
            {
                referenceProvider.CharacterBrain.SetInteractEnabled(canInteractOnEnter == Ternary.True);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,
            AnimatorControllerPlayable controller)
        {
            base.OnStateExit(animator, stateInfo, layerIndex, controller);
            if (canMoveOnExit != Ternary.Undefined)
            {
                referenceProvider.Movement.CanMove(canMoveOnExit == Ternary.True);
            }
            if (canInteractOnExit != Ternary.Undefined)
            {
                referenceProvider.CharacterBrain.SetInteractEnabled(canInteractOnExit == Ternary.True);
            }
        }
    }

    public enum Ternary
    {
        Undefined = 0,
        False = 1,
        True = 2
    }
}