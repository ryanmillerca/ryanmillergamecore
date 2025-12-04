namespace RyanMillerGameCore.TurnBasedCombat.UI {
	using UnityEngine;
	using TMPro;
	using UnityEngine.UI;

	public class TargetButton : MonoBehaviour {

		[SerializeField] private TextMeshProUGUI label;
		private PlayerTurnUI playerTurnUI;
		private Combatant combatant;
		private Button button;

		private void Awake() {
			button = GetComponent<Button>();
			if (label == null) {
				label = GetComponentInChildren<TextMeshProUGUI>();
			}
		}

		public void Configure(PlayerTurnUI playerTurnUI, Combatant combatant) {
			this.playerTurnUI = playerTurnUI;
			this.combatant = combatant;
			if (combatant) {
				if (label) {
					label.SetText(combatant.CombatantName);
				}
				button.interactable = true;
			}
			else {
				if (label) {
					label.SetText("");
				}
				button.interactable = false;
			}
		}

		public void Reset() {
			this.combatant = null;
			if (label) {
				label.SetText("");
			}
		}

		public void OnClick() {
			if (combatant) {
				playerTurnUI.OnTargetButtonClicked(combatant);
			}
		}
	}
}
