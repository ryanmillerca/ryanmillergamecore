namespace RyanMillerGameCore.Character {
	using UnityEngine;

	public class ControlPlayerInputActive : MonoBehaviour {

		public void SetPlayerInputEnabled() {
			characterInput?.SetInputEnabled(true);
		}

		public void SetPlayerInputDisabled() {
			characterInput?.SetInputEnabled(false);
		}

		CharacterInput characterInput {
			get {
				return FindFirstObjectByType<CharacterInput>();
			}
		}

	}
}
