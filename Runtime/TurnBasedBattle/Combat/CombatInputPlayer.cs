namespace RyanMillerGameCore.TurnBasedCombat {
	using System.Linq;
	using UnityEngine;
	using UI;

	/// <summary>
	/// Connects BattleManager to Input UI for Player
	/// </summary>
	public class CombatInputPlayer : MonoBehaviour {
		[SerializeField] private BattleManager m_BattleManager;
		[SerializeField] private UIBattleMenu m_UIBattleMenu;

		private void OnEnable() {
			if (m_BattleManager == null) {
				return;
			}
			m_BattleManager.PlayerInputRequired += OnPlayerInputRequired;
			m_BattleManager.PlayerInputReceived += OnPlayerInputReceived;
		}

		private void OnDisable() {
			if (m_BattleManager == null) {
				return;
			}
			m_BattleManager.PlayerInputRequired -= OnPlayerInputRequired;
			m_BattleManager.PlayerInputReceived -= OnPlayerInputReceived;
		}

		private void OnPlayerInputRequired(PlayerInputData inputData) {
			Debug.Log($"Waiting on Player input for {inputData.Actor.CombatantName}");
			Debug.Log($"Available moves: {string.Join(", ", inputData.AvailableMoves.Select(m => m.ActionName))}");
			Debug.Log($"Valid targets: {string.Join(", ", inputData.ValidTargets.Select(t => t.CombatantName))}");

			// Here you would:
			// 1. Show your battle UI menu
			// 2. Populate it with available moves and targets
			// 3. Wait for player selection
			// 4. Call SubmitPlayerInput when ready

			m_UIBattleMenu.Show(inputData);
		}

		private void OnPlayerInputReceived(PlayerInputResponse response) {
			Debug.Log($"🎮 Player input received: {response.SelectedAction.ActionName} on {response.SelectedTarget.CombatantName}");
		}
	}
}
