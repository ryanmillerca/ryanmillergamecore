namespace RyanMillerGameCore.TurnBasedCombat.UI {

	using UnityEngine;
	using System.Collections.Generic;

	public class UIBattleMenu : MonoBehaviour {

		private CombatInputPlayer combatInputPlayer;
		private BattleManager battleManager;
		private Combatant currentActor;

		public void Show(PlayerInputData inputData) {
			currentActor = inputData.Actor;
			PopulateActions(inputData.AvailableMoves);
			SetAvailableTargets(inputData.ValidTargets);
		}

		private void PopulateActions(List<BattleAction> battleActions) {
			foreach (BattleAction action in battleActions) { }
		}

		private void SetAvailableTargets(List<Combatant> availableTargets) {
			foreach (Combatant combatant in availableTargets) { }
		}

		public void PlayerCommandSubmitted() {
			PlayerInputResponse response = new PlayerInputResponse();
			battleManager.SubmitPlayerInput(response);
		}
	}
}
