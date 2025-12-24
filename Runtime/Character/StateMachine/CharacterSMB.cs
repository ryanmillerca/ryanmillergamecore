namespace RyanMillerGameCore.Character.SMB {
	using UnityEngine;

	/// <summary>
	/// Base/Abstract class for all character state machine behaviours.
	/// </summary>
	abstract public class CharacterSMB : StateMachineBehaviour {
		protected Animator animator;
		protected CharacterReferenceProvider referenceProvider {
			get {
				return (CharacterReferenceProvider)animator.GetComponentReference();
			}
		}

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			this.animator = animator;
			OnCharacterStateEnter(animator, stateInfo, layerIndex);
		}

		protected virtual void OnCharacterStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			base.OnStateExit(animator, stateInfo, layerIndex);
			OnCharacterStateExit(this.animator, stateInfo, layerIndex);
		}

		protected virtual void OnCharacterStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			base.OnStateUpdate(animator, stateInfo, layerIndex);
			OnCharacterStateUpdate(this.animator, stateInfo, layerIndex);
		}

		protected virtual void OnCharacterStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
	}
}
