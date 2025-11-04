namespace RyanMillerGameCore.TurnBasedCombat.UI {
	using System.Collections.Generic;
	using System.Linq;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;
	using TurnBasedCombat;
	using System;

	/// <summary>
	/// UI controller for player turn input using prepopulated button arrays + TextMeshPro labels.
	/// - Hook this up in the inspector: buttons arrays, TMP labels, panels, BattleManager.
	/// - Expects BattleManager to expose: OnPlayerActionRequested(Combatant, List<BattleAction>),
	///   MoveResolved(BattleResult), SubmitPlayerCommand(BattleCommand).
	/// </summary>
	public class PlayerTurnUI : MonoBehaviour {
		[Header("References")]
		public BattleManager m_BattleManager; // drag your BattleManager here

		[Tooltip("Precreated action buttons (enable/disable). The corresponding labels should be in actionLabels at same indices.")]
		public GameObject actionButtonPrefab;
		public List<CombatActionButton> actionButtons = new List<CombatActionButton>();
		public TMP_Text[] actionLabels;

		[Header("Target UI (prepopulated)")]
		[Tooltip("Precreated target buttons (enable/disable). The corresponding labels should be in targetLabels at same indices.")]
		public Button[] targetButtons;
		public TMP_Text[] targetLabels;

		[Header("Panels")]
		public GameObject actionPanel; // parent panel for action buttons
		public GameObject targetPanel; // parent panel for target buttons

		// Current turn state
		private Combatant m_CurrentActor;
		private List<BattleAction> m_CurrentActions;
		private BattleAction m_SelectedAction;

		// Available candidate targets (populated when target selection is needed)
		private List<Combatant> m_CurrentTargets = new List<Combatant>();

		private void OnEnable() {
			if (m_BattleManager == null) {
				Debug.LogWarning("PlayerTurnUI: BattleManager not assigned in inspector.");
				return;
			}

			// Subscribe to BattleManager events
			m_BattleManager.OnPlayerActionRequested += HandlePlayerActionRequested;
			m_BattleManager.MoveResolved += HandleMoveResolved;
			m_BattleManager.OnTurnOrderUpdated += HandleTurnOrderUpdated; // optional
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
			m_CurrentActor = actor;
			m_CurrentActions = (availableActions != null && availableActions.Count > 0)
				? availableActions
				: (actor.m_Moves ?? new List<BattleAction>());
			ShowActionPanel();
		}

		private void ShowActionPanel() {

			// Clear previous state
			ClearTargetSelectionState();
			ClearActionSelectionState();

			if (actionPanel != null) actionPanel.SetActive(true);
			if (targetPanel != null) targetPanel.SetActive(false);

			if (m_CurrentActions == null || m_CurrentActions.Count == 0) {
				Debug.LogWarning($"PlayerTurnUI: No actions for actor {m_CurrentActor?.m_CombatantName ?? "null"}");
				return;
			}

			// Ensure there are enough buttons
			PrepareActionButtons(m_CurrentActions.Count);

			// Enable and populate prepopulated action buttons up to array length
			int count = Mathf.Min(actionButtons.Count, m_CurrentActions.Count);
			for (int i = 0; i < actionButtons.Count; i++) {
				bool used = i < count;
				if (used) {
					actionButtons[i].gameObject.SetActive(true);
					actionButtons[i].Configure(this, m_CurrentActions[i]);
				}
				else {
					actionButtons[i].Reset();
					actionButtons[i].gameObject.SetActive(false);
				}
			}
		}

		/// <summary>
		/// Adds more action buttons if we don't have enough
		/// </summary>
		/// <param name="numberOfButtons"></param>
		private void PrepareActionButtons(int numberOfButtons) {
			int difference = numberOfButtons - actionButtons.Count;
			if (difference <= 0) return;

			Transform parentTransform = (actionPanel != null) ? actionPanel.transform : this.transform;
			for (int i = 0; i < difference; i++) {
				GameObject newActionButton = Instantiate(actionButtonPrefab, parentTransform, false);
				newActionButton.transform.localScale = Vector3.one;
				var cab = newActionButton.GetComponent<CombatActionButton>();
				if (cab != null) actionButtons.Add(cab);
				else {
					Debug.LogWarning("Prepared action button prefab is missing CombatActionButton component.");
					actionButtons.Add(null); // keep index consistency
				}
			}
		}

		public void OnActionButtonClicked(BattleAction battleAction) {
			if (battleAction == null || m_CurrentActor == null) return;

			// Record selected action for later (target selection)
			m_SelectedAction = battleAction;

			bool requiresSingleTarget = battleAction.m_TargetType == ActionTargetType.SingleEnemy || battleAction.m_TargetType == ActionTargetType.SingleAlly;

			// Immediate submit for self/group actions
			if (battleAction.m_TargetSelf ||
			    battleAction.m_TargetType == ActionTargetType.Self ||
			    battleAction.m_TargetType == ActionTargetType.AllEnemies ||
			    battleAction.m_TargetType == ActionTargetType.AllAllies) {

				var cmd = new BattleCommand(m_CurrentActor, battleAction, m_CurrentActor);
				m_BattleManager.SubmitPlayerCommand(cmd);
				HideAllPanels();
				return;
			}

			if (requiresSingleTarget) {
				BuildAndShowTargets();
				return;
			}

			// fallback: submit with actor as target
			var fallback = new BattleCommand(m_CurrentActor, battleAction, m_CurrentActor);
			m_BattleManager.SubmitPlayerCommand(fallback);
			HideAllPanels();
		}

		private void BuildAndShowTargets() {
			ClearTargetSelectionState();

			if (actionPanel != null) actionPanel.SetActive(false);
			if (targetPanel != null) targetPanel.SetActive(true);

			// Preferred: use BattleManager.GetValidTargets if public. If not, fallback to filtering.
			try {
				var mi = m_BattleManager.GetType().GetMethod("GetValidTargets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
				if (mi != null) {
					var res = mi.Invoke(m_BattleManager, new object[] { m_CurrentActor }) as System.Collections.IEnumerable;
					m_CurrentTargets.Clear();
					if (res != null) {
						foreach (var o in res)
							if (o is Combatant c)
								m_CurrentTargets.Add(c);
					}
				}
				else {
					m_CurrentTargets = m_BattleManager.m_Combatants.Where(c => c.isAlive && c != m_CurrentActor).ToList();
				}
			}
			catch (Exception ex) {
				Debug.LogWarning("BuildAndShowTargets reflection failed: " + ex.Message);
				m_CurrentTargets = m_BattleManager.m_Combatants.Where(c => c.isAlive && c != m_CurrentActor).ToList();
			}

			for (int i = 0; i < targetButtons.Length; i++) {
				bool used = i < m_CurrentTargets.Count;
				targetButtons[i].gameObject.SetActive(used);
				targetButtons[i].onClick.RemoveAllListeners();

				if (used) {
					var target = m_CurrentTargets[i];
					if (i < targetLabels.Length && targetLabels[i] != null)
						targetLabels[i].text = $"{target.m_CombatantName} ({target.m_CurrentHp}/{target.m_MaxHp})";

					int idx = i; // capture
					targetButtons[i].onClick.AddListener(() => OnTargetButtonClicked(m_CurrentTargets[idx]));
				}
			}

			// If there are zero targets, submit action with actor as target
			if (m_CurrentTargets.Count == 0) {
				var cmd = new BattleCommand(m_CurrentActor, m_SelectedAction ?? m_CurrentActions.FirstOrDefault(), m_CurrentActor);
				m_BattleManager.SubmitPlayerCommand(cmd);
				HideAllPanels();
			}
		}

		private void OnTargetButtonClicked(Combatant chosenTarget) {
			if (m_CurrentActor == null || m_SelectedAction == null || chosenTarget == null) return;

			var cmd = new BattleCommand(m_CurrentActor, m_SelectedAction, chosenTarget);
			m_BattleManager.SubmitPlayerCommand(cmd);
			HideAllPanels();
		}

		private void HideAllPanels() {
			if (actionPanel != null) actionPanel.SetActive(false);
			if (targetPanel != null) targetPanel.SetActive(false);
			ClearActionSelectionState();
			ClearTargetSelectionState();
			m_CurrentActor = null;
			m_CurrentActions = null;
			m_SelectedAction = null;
			m_CurrentTargets.Clear();
		}

		private void ClearActionSelectionState() {
			if (actionButtons == null) return;
			foreach (var b in actionButtons) {
				if (b == null) continue;
				b.Reset();
				b.gameObject.SetActive(false);
			}

			if (actionLabels != null) {
				foreach (var l in actionLabels)
					if (l != null)
						l.text = "";
			}
		}

		private void ClearTargetSelectionState() {
			if (targetButtons == null) return;
			foreach (var b in targetButtons) {
				if (b == null) continue;
				b.onClick.RemoveAllListeners();
				b.gameObject.SetActive(false);
			}

			if (targetLabels != null) {
				foreach (var l in targetLabels)
					if (l != null)
						l.text = "";
			}
		}

		// Optional: respond to moveResolved (show simple logs or spawn floating text)
		private void HandleMoveResolved(BattleResult result) {
			// Example: show debug text — replace with floating text or other VFX instantiation
			Debug.Log($"[UI] {result.Message}");
		}

		// Optional: react to turn order updates (refresh queue display if you have one)
		private void HandleTurnOrderUpdated(List<Combatant> upcoming) {
			// UI hook if you want notifications when queue changes.
			// Example: Debug.Log("Turn queue updated");
		}
	}
}
