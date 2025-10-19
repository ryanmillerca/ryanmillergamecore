using UnityEngine;
using System.Linq;

namespace RyanMillerGameCore.TurnBasedCombat {
	public class DebugBattleListener : MonoBehaviour {
		[Header("References")]
		public BattleManager battleManager;

		[Header("Log Settings")]
		public bool logBattleEvents = true;
		public bool logTurnEvents = true;
		public bool logCombatantEvents = true;
		public bool logMoveResolution = true;
		public bool logPlayerInputEvents = true;

		private void OnEnable() {
			if (battleManager == null) {
				battleManager = FindObjectOfType<BattleManager>();
			}

			if (battleManager != null) {
				battleManager.BattleEvent += OnBattleEvent;
				battleManager.TurnEvent += OnTurnEvent;
				battleManager.MoveResolved += OnMoveResolved;
				battleManager.BattleEnded += OnBattleEnded;
				battleManager.PlayerInputRequired += OnPlayerInputRequired;
				battleManager.PlayerInputReceived += OnPlayerInputReceived;
			}
		}

		private void OnDisable() {
			if (battleManager != null) {
				battleManager.BattleEvent -= OnBattleEvent;
				battleManager.TurnEvent -= OnTurnEvent;
				battleManager.MoveResolved -= OnMoveResolved;
				battleManager.BattleEnded -= OnBattleEnded;
				battleManager.PlayerInputRequired -= OnPlayerInputRequired;
				battleManager.PlayerInputReceived -= OnPlayerInputReceived;
			}
		}

		private void OnPlayerInputRequired(PlayerInputData inputData) {
			if (!logPlayerInputEvents) return;

			string colorTag = $"<color={inputData.Actor.ColorAsHex}>";
			Debug.Log($"{colorTag}🎮 PLAYER INPUT REQUIRED for {inputData.Actor.m_CombatantName}</color>");
			Debug.Log($"{colorTag}   Moves: {string.Join(", ", inputData.AvailableMoves.Select(m => m.m_ActionName))}</color>");
			Debug.Log($"{colorTag}   Targets: {string.Join(", ", inputData.ValidTargets.Select(t => t.m_CombatantName))}</color>");
		}

		private void OnPlayerInputReceived(PlayerInputResponse response) {
			if (!logPlayerInputEvents) return;

			if (response.IsValid) {
				string colorTag = $"<color=#00FF00>";
				Debug.Log($"{colorTag}🎮 PLAYER SELECTED: {response.SelectedAction.m_ActionName} -> {response.SelectedTarget.m_CombatantName}</color>");
			}
			else {
				string colorTag = $"<color=#FF0000>";
				Debug.Log($"{colorTag}🎮 INVALID PLAYER INPUT: {response.ValidationMessage}</color>");
			}
		}

		private void OnBattleEvent(BattleEventData eventData) {
			if (!logBattleEvents) return;

			string colorTag = eventData.Combatant != null ? $"<color={eventData.Combatant.ColorAsHex}>" : "";
			string colorClose = eventData.Combatant != null ? "</color>" : "";

			switch (eventData.EventType) {
				case BattleEventType.BattleStarted:
					Debug.Log($"🚀 BATTLE STARTED: {eventData.Message}");
					break;
				case BattleEventType.BattleEnded:
					Debug.Log($"🏁 BATTLE ENDED: {eventData.Message}");
					break;
				case BattleEventType.TurnSkipped:
					Debug.LogWarning($"{colorTag}⏭️ {eventData.Message}{colorClose}");
					break;
				case BattleEventType.NoValidTargets:
					Debug.LogWarning($"{colorTag}🎯 {eventData.Message}{colorClose}");
					break;
				case BattleEventType.NoMovesAvailable:
					Debug.LogWarning($"{colorTag}❌ {eventData.Message}{colorClose}");
					break;
				case BattleEventType.CommandError:
				case BattleEventType.ResolutionError:
				case BattleEventType.EventHandlerError:
					Debug.LogError($"{colorTag}💥 {eventData.Message}{colorClose}");
					break;
				case BattleEventType.BattleEndConditionMet:
					Debug.Log($"{colorTag}⚡ {eventData.Message}{colorClose}");
					break;
				case BattleEventType.TurnOrderUpdated:
					Debug.Log($"📋 {eventData.Message}");
					break;
				case BattleEventType.TargetChanged:
					Debug.Log($"{colorTag}🎯 {eventData.Message}{colorClose}");
					break;
			}
		}

		private void OnTurnEvent(TurnEventData eventData) {
			if (!logTurnEvents) return;

			string colorTag = $"<color={eventData.Combatant.ColorAsHex}>";

			switch (eventData.EventType) {
				case TurnEventType.TurnStarted:
					string brainInfo = eventData.Combatant.m_AIBrain != null ?
					$" ({eventData.Combatant.m_AIBrain.GetType().Name})" : "";
					Debug.Log($"{colorTag}🎪 It's {eventData.Combatant.m_CombatantName}'s turn!{brainInfo}</color>");
					break;
				case TurnEventType.ActionSelected:
					string aiNote = eventData.Combatant.m_Team == Team.Enemy ? " [AI]" : "";
					Debug.Log($"{colorTag}🎯 {eventData.Combatant.m_CombatantName} selects {eventData.Action.m_ActionName} targeting {eventData.Target.m_CombatantName}{aiNote}</color>");
					break;
				case TurnEventType.MultiTurnStarted:
					Debug.Log($"{colorTag}⚡ {eventData.Combatant.m_CombatantName} starts charging {eventData.Action.m_ActionName} for {eventData.Action.m_TurnCost} turns!</color>");
					break;
				case TurnEventType.TurnEnded:
					Debug.Log($"{colorTag}✅ {eventData.Combatant.m_CombatantName}'s turn ended</color>");
					break;
			}
		}

		private void OnMoveResolved(BattleResult result) {
			if (!logMoveResolution) return;

			if (result.Actor == null) {
				Debug.LogError("💥 BattleResult has null Actor!");
				return;
			}

			string colorTag = $"<color={result.Actor.ColorAsHex}>";

			if (result.Missed) {
				Debug.Log($"{colorTag}❌ {result.Message}</color>");
			}
			else if (result.DamageDealt > 0) {
				string critTag = result.CriticalHit ? "💥 " : "";
				string critInfo = result.CriticalHit ? $"[Crit Chance: {result.CriticalChance:P1}]" : "";

				string targetName = result.Target != null ? result.Target.m_CombatantName : "DEAD TARGET";
				string safeMessage = result.Message ?? $"{result.Actor.m_CombatantName} used attack on {targetName} for {result.DamageDealt} damage.";

				Debug.Log($"{colorTag}⚔️ {critTag}{safeMessage} {critInfo}</color>");
			}
			else if (result.HealingDone > 0) {
				string targetName = result.Target != null ? result.Target.m_CombatantName : "SELF";
				string safeMessage = result.Message ?? $"{result.Actor.m_CombatantName} used heal on {targetName} for {result.HealingDone} healing.";

				Debug.Log($"{colorTag}💚 {safeMessage}</color>");
			}
			else {
				string safeMessage = result.Message ?? $"{result.Actor.m_CombatantName} used {result.BattleAction?.m_ActionName ?? "UNKNOWN ACTION"}.";
				Debug.Log($"{colorTag}✨ {safeMessage}</color>");
			}
		}

		private void OnBattleEnded(BattleOutcome outcome) {
			switch (outcome) {
				case BattleOutcome.Victory:
					Debug.Log("🎉 VICTORY! Players win!");
					break;
				case BattleOutcome.Defeat:
					Debug.Log("💀 DEFEAT! Enemies win!");
					break;
				case BattleOutcome.Undefined:
					Debug.Log("❓ Battle ended with undefined outcome");
					break;
			}
		}

		public void SubscribeToCombatant(Combatant combatant) {
			if (combatant != null) {
				combatant.CombatantEvent += OnCombatantEvent;
			}
		}

		public void UnsubscribeFromCombatant(Combatant combatant) {
			if (combatant != null) {
				combatant.CombatantEvent -= OnCombatantEvent;
			}
		}

		private void OnCombatantEvent(CombatantEventData eventData) {
			if (!logCombatantEvents) return;

			string colorTag = $"<color={eventData.Combatant.ColorAsHex}>";

			switch (eventData.EventType) {
				case CombatantEventType.DamageTaken:
					Debug.Log($"{colorTag}💔 {eventData.Message}</color>");
					break;
				case CombatantEventType.HealingReceived:
					Debug.Log($"{colorTag}💚 {eventData.Message}</color>");
					break;
				case CombatantEventType.Died:
					Debug.Log($"{colorTag}☠️ {eventData.Message}</color>");
					break;
				case CombatantEventType.FullHealth:
					Debug.Log($"{colorTag}⭐ {eventData.Message}</color>");
					break;
				case CombatantEventType.ChargeStarted:
					Debug.Log($"{colorTag}⚡ {eventData.Message}</color>");
					break;
				case CombatantEventType.Charging:
					Debug.Log($"{colorTag}⏳ {eventData.Message}</color>");
					break;
				case CombatantEventType.ChargeComplete:
					Debug.Log($"{colorTag}✅ {eventData.Message}</color>");
					break;
				case CombatantEventType.ChargeCancelled:
					Debug.Log($"{colorTag}❌ {eventData.Message}</color>");
					break;
				case CombatantEventType.ChargeDamage:
					Debug.Log($"{colorTag}⚡ {eventData.Message}</color>");
					break;
				case CombatantEventType.DefendStarted:
					Debug.Log($"{colorTag}🛡️ {eventData.Message}</color>");
					break;
				case CombatantEventType.DefendEnded:
					Debug.Log($"{colorTag}🛡️ {eventData.Message}</color>");
					break;
				case CombatantEventType.CounterAttack:
					Debug.Log($"{colorTag}⚔️ {eventData.Message}</color>");
					break;
				case CombatantEventType.AttackBuffed:
					Debug.Log($"{colorTag}📈 {eventData.Message}</color>");
					break;
				case CombatantEventType.AttackBuffEnded:
					Debug.Log($"{colorTag}📉 {eventData.Message}</color>");
					break;
			}
		}
	}
}
