using System;
namespace RyanMillerGameCore.TurnBasedCombat.UI {
	using UnityEngine;
	using TMPro;
	using UnityEngine.UI;

	public class CombatActionButton : MonoBehaviour {

		[SerializeField] private TextMeshProUGUI label;
		private PlayerTurnUI playerTurnUI;
		private BattleAction battleAction;
		private Button button;

		private void Awake() {
			button = GetComponent<Button>();
			if (label == null) {
				label = GetComponentInChildren<TextMeshProUGUI>();
			}
		}

		public void Configure(PlayerTurnUI playerTurnUI, BattleAction battleAction) {
			this.playerTurnUI = playerTurnUI;
			this.battleAction = battleAction;
			if (battleAction) {
				label.SetText(battleAction.m_ActionName);
				button.interactable = true;
			}
			else {
				label.SetText("");
				button.interactable = false;
			}
		}

		public void Reset() {
			this.battleAction = null;
			label.SetText("");
		}

		public void OnClick() {
			if (battleAction) {
				playerTurnUI.OnActionButtonClicked(battleAction);
			}
		}
	}
}
