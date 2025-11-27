namespace RyanMillerGameCore.TurnBasedCombat.UI {
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using TurnBasedCombat;
	using UnityEngine.EventSystems;

	/// <summary>
	/// UI controller for player turn input
	/// </summary>
	public class PlayerTurnUI : MonoBehaviour {

		[SerializeField] private BattleManager m_BattleManager;
		[SerializeField] private GameObject m_ActionButtonPrefab;
		[SerializeField] private GameObject m_TargetButtonPrefab;
		[SerializeField] private GameObject m_ActionPanel;
		[SerializeField] private GameObject m_TargetPanel;

		private Combatant m_currentActor;
		private List<BattleAction> m_currentActions;
		private BattleAction m_selectedAction;
		private readonly List<TargetButton> m_targetButtons = new List<TargetButton>();
		private readonly List<CombatActionButton> m_actionButtons = new List<CombatActionButton>();
		private List<Combatant> m_currentTargets = new List<Combatant>();

		private void OnEnable() {
			if (m_BattleManager == null) {
				Debug.LogWarning("PlayerTurnUI: BattleManager not assigned in inspector.");
				return;
			}
			m_BattleManager.OnPlayerActionRequested += HandlePlayerActionRequested;
			m_BattleManager.MoveResolved += HandleMoveResolved;
			m_BattleManager.OnTurnOrderUpdated += HandleTurnOrderUpdated;
			foreach (Transform child in m_ActionPanel.transform) {
				child.gameObject.SetActive(false);
			}
			foreach (Transform child in m_TargetPanel.transform) {
				child.gameObject.SetActive(false);
			}
		}

		private void OnDisable() {
			if (m_BattleManager != null) {
				m_BattleManager.OnPlayerActionRequested -= HandlePlayerActionRequested;
				m_BattleManager.MoveResolved -= HandleMoveResolved;
				m_BattleManager.OnTurnOrderUpdated -= HandleTurnOrderUpdated;
			}
			ClearActionSelectionState();
			ClearTargetSelectionState();
		}

		// Called by BattleManager when player input is required
		private void HandlePlayerActionRequested(Combatant actor, List<BattleAction> availableActions) {
			m_currentActor = actor;
			m_currentActions = (availableActions != null && availableActions.Count > 0)
				? availableActions
				: (actor.Moves ?? new List<BattleAction>());
			ShowActionPanel();
		}

		private void ShowActionPanel() {
			ClearTargetSelectionState();
			ClearActionSelectionState();

			if (m_ActionPanel != null) {
				m_ActionPanel.SetActive(true);
			}
			if (m_TargetPanel != null) {
				m_TargetPanel.SetActive(false);
			}

			if (m_currentActions == null || m_currentActions.Count == 0) {
				Debug.LogWarning($"PlayerTurnUI: No actions for actor {m_currentActor?.CombatantName ?? "null"}");
				return;
			}

			// Ensure there are enough buttons
			PrepareActionButtons(m_currentActions.Count);

			// Enable and populate prepopulated action buttons up to array length
			int count = Mathf.Min(m_actionButtons.Count, m_currentActions.Count);
			for (int i = 0; i < m_actionButtons.Count; i++) {
				bool used = i < count;
				if (used) {
					m_actionButtons[i].gameObject.SetActive(true);
					m_actionButtons[i].Configure(this, m_currentActions[i]);
				}
				else {
					if (m_actionButtons[i] != null) m_actionButtons[i].Reset();
					m_actionButtons[i].gameObject.SetActive(false);
				}
			}
			
			EventSystem.current.SetSelectedGameObject(m_actionButtons[0].gameObject);
		}

		/// <summary>
		/// Adds more action buttons if we don't have enough
		/// </summary>
		private void PrepareActionButtons(int numberOfButtons) {
			int difference = numberOfButtons - m_actionButtons.Count;
			if (difference <= 0) return;

			Transform parentTransform = (m_ActionPanel != null) ? m_ActionPanel.transform : this.transform;
			for (int i = 0; i < difference; i++) {
				GameObject newActionButton = Instantiate(m_ActionButtonPrefab, parentTransform, false);
				newActionButton.transform.localScale = Vector3.one;
				var cab = newActionButton.GetComponent<CombatActionButton>();
				if (cab != null) m_actionButtons.Add(cab);
				else {
					Debug.LogWarning("Prepared action button prefab is missing CombatActionButton component.");
					m_actionButtons.Add(null); // keep index consistency
				}
			}
		}

		public void OnActionButtonClicked(BattleAction battleAction) {
			if (battleAction == null || m_currentActor == null) return;

			// Record selected action for later (target selection)
			m_selectedAction = battleAction;

			bool requiresSingleTarget = battleAction.TargetType == ActionTargetType.SingleEnemy || battleAction.TargetType == ActionTargetType.SingleAlly;

			// Immediate submit for self/group actions
			if (battleAction.TargetSelf ||
			    battleAction.TargetType == ActionTargetType.Self ||
			    battleAction.TargetType == ActionTargetType.AllEnemies ||
			    battleAction.TargetType == ActionTargetType.AllAllies) {

				var cmd = new BattleCommand(m_currentActor, battleAction, m_currentActor);
				m_BattleManager.SubmitPlayerCommand(cmd);
				HideAllPanels();
				return;
			}

			if (requiresSingleTarget) {
				BuildAndShowTargets();
				return;
			}

			// fallback: submit with actor as target
			var fallback = new BattleCommand(m_currentActor, battleAction, m_currentActor);
			m_BattleManager.SubmitPlayerCommand(fallback);
			HideAllPanels();
		}

		private void BuildAndShowTargets()
		{
			ClearTargetSelectionState();

			if (m_ActionPanel != null) m_ActionPanel.SetActive(false);
			if (m_TargetPanel != null) m_TargetPanel.SetActive(true);

			m_currentTargets.Clear();

			// Determine targets based on selected action's target type
			switch (m_selectedAction.TargetType)
			{
				case ActionTargetType.Self:
					m_currentTargets.Add(m_currentActor);
					break;

				case ActionTargetType.SingleAlly:
				case ActionTargetType.AllAllies:
					m_currentTargets = m_BattleManager.Combatants
						.Where(c => c.isAlive && c.Team == m_currentActor.Team)
						.ToList();
					break;

				case ActionTargetType.SingleEnemy:
				case ActionTargetType.AllEnemies:
					m_currentTargets = m_BattleManager.Combatants
						.Where(c => c.isAlive && c.Team != m_currentActor.Team)
						.ToList();
					break;
			}

			PrepareTargetButtons(m_currentTargets.Count);

			int count = Mathf.Min(m_targetButtons.Count, m_currentTargets.Count);
			for (int i = 0; i < m_targetButtons.Count; i++)
			{
				bool used = i < count;
				if (used)
				{
					m_targetButtons[i].gameObject.SetActive(true);
					m_targetButtons[i].Configure(this, m_currentTargets[i]);
				}
				else
				{
					if (m_targetButtons[i] != null) m_targetButtons[i].Reset();
					m_targetButtons[i].gameObject.SetActive(false);
				}
			}

			EventSystem.current.SetSelectedGameObject(m_targetButtons[0].gameObject);

			// If there are zero targets (shouldn't happen normally), submit action with actor as fallback
			if (m_currentTargets.Count == 0)
			{
				var cmd = new BattleCommand(
					m_currentActor,
					m_selectedAction ?? m_currentActions.FirstOrDefault(),
					m_currentActor
				);
				m_BattleManager.SubmitPlayerCommand(cmd);
				HideAllPanels();
			}
		}

		/// <summary>
		/// Adds more target buttons if we don't have enough
		/// </summary>
		void PrepareTargetButtons(int numberOfButtons) {
			int difference = numberOfButtons - m_targetButtons.Count;
			if (difference <= 0) return;

			Transform parentTransform = (m_TargetPanel != null) ? m_TargetPanel.transform : this.transform;
			for (int i = 0; i < difference; i++) {
				GameObject newTargetButton = Instantiate(m_TargetButtonPrefab, parentTransform, false);
				newTargetButton.transform.localScale = Vector3.one;
				var tb = newTargetButton.GetComponent<TargetButton>();
				if (tb != null) m_targetButtons.Add(tb);
				else {
					Debug.LogWarning("Prepared target button prefab is missing TargetButton component.");
					m_targetButtons.Add(null);
				}
			}
		}

		public void OnTargetButtonClicked(Combatant chosenTarget) {
			if (m_currentActor == null || m_selectedAction == null || chosenTarget == null) return;

			var cmd = new BattleCommand(m_currentActor, m_selectedAction, chosenTarget);
			m_BattleManager.SubmitPlayerCommand(cmd);
			HideAllPanels();
		}

		private void HideAllPanels() {
			if (m_ActionPanel != null) m_ActionPanel.SetActive(false);
			if (m_TargetPanel != null) m_TargetPanel.SetActive(false);
			ClearActionSelectionState();
			ClearTargetSelectionState();
			m_currentActor = null;
			m_currentActions = null;
			m_selectedAction = null;
			m_currentTargets.Clear();
		}

		private void ClearActionSelectionState() {
			if (m_actionButtons == null) return;
			foreach (CombatActionButton b in m_actionButtons.Where(b => b != null)) {
				b.Reset();
				b.gameObject.SetActive(false);
			}
		}

		private void ClearTargetSelectionState() {
			if (m_targetButtons == null) return;
			foreach (TargetButton t in m_targetButtons.Where(t => t != null)) {
				t.Reset();
				t.gameObject.SetActive(false);
			}
		}

		private void HandleMoveResolved(BattleResult result) {
			// Example: show debug text — replace with floating text or other VFX instantiation
			Debug.Log($"[UI] {result.Message}");
		}

		private void HandleTurnOrderUpdated(List<Combatant> upcoming) {
			// UI hook if you want notifications when queue changes.
			// Example: Debug.Log("Turn queue updated");
		}
	}
}