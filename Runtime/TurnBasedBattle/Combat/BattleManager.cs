using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace RyanMillerGameCore.TurnBasedCombat {
	public class BattleManager : MonoBehaviour {
		[Header("Combatants")]
		public List<Combatant> m_Combatants = new List<Combatant>();

		[Header("Turn Settings")]
		public float m_GaugeThreshold = 100f;
		public float m_TickRate = 1f;
		public int m_LookaheadTurns = 5;

#pragma warning disable CS0414
		private bool m_BattleActive = false;

		private bool m_WaitingForPlayerInput = false;
		private Combatant m_CurrentPlayerActor = null;
		private List<Combatant> m_CurrentValidTargets = null;

		// Pending player command storage
		private BattleCommand m_PendingPlayerCommand = null;

		public delegate void OnMoveResolved(BattleResult result);
		public event OnMoveResolved MoveResolved;

		public delegate void OnBattleEvent(BattleEventData eventData);
		public event OnBattleEvent BattleEvent;

		public delegate void OnTurnEvent(TurnEventData eventData);
		public event OnTurnEvent TurnEvent;

		public delegate void OnBattleOutcome(BattleOutcome outcome);
		public event OnBattleOutcome BattleEnded;

		public delegate void OnPlayerInputRequired(PlayerInputData inputData);
		public event OnPlayerInputRequired PlayerInputRequired;

		public delegate void OnPlayerInputReceived(PlayerInputResponse response);
		public event OnPlayerInputReceived PlayerInputReceived;
		public event Action<Combatant, List<BattleAction>> OnPlayerActionRequested;
		public event Action<List<Combatant>> OnTurnOrderUpdated;
		// New API for the UI to submit a full BattleCommand (preferred)
		public void SubmitPlayerCommand(BattleCommand cmd) {
			if (!m_WaitingForPlayerInput) return;
			if (cmd == null) return;
			if (m_CurrentPlayerActor == null) return;
			if (cmd.Actor != m_CurrentPlayerActor) return; // ensure actor matches the one requesting input
			if (!m_CurrentPlayerActor.isAlive) return;

			// Accept the command
			m_PendingPlayerCommand = cmd;

			// Mark input as received so the waiting coroutine continues
			m_WaitingForPlayerInput = false;

			// Fire legacy PlayerInputReceived event for compatibility (optional)
			PlayerInputReceived?.Invoke(new PlayerInputResponse {
				SelectedAction = cmd.BattleAction,
				SelectedTarget = cmd.Target,
				IsValid = true,
				ValidationMessage = null
			});
		}

		public void SubmitPlayerInput(PlayerInputResponse response) {
			if (m_WaitingForPlayerInput && m_CurrentPlayerActor != null) {
				m_WaitingForPlayerInput = false;
				PlayerInputReceived?.Invoke(response);
			}
		}

		public void CancelPlayerInput() {
			if (m_WaitingForPlayerInput) {
				m_WaitingForPlayerInput = false;
				m_CurrentPlayerActor = null;
				m_CurrentValidTargets = null;
				m_PendingPlayerCommand = null;
			}
		}

		private void RaiseBattleEvent(BattleEventType eventType, string message, Combatant combatant = null, Combatant target = null) {
			BattleEvent?.Invoke(new BattleEventData {
				EventType = eventType,
				Message = message,
				Combatant = combatant,
				Target = target,
				Timestamp = Time.time
			});
		}

		private void RaiseTurnEvent(TurnEventType eventType, Combatant combatant, Combatant target = null, BattleAction action = null) {
			TurnEvent?.Invoke(new TurnEventData {
				EventType = eventType,
				Combatant = combatant,
				Target = target,
				Action = action,
				Timestamp = Time.time
			});
		}

		bool PlayersAreAlive() {
			foreach (Combatant c in m_Combatants) {
				if (c.isAlive && c.m_Team == Team.Player) {
					return true;
				}
			}
			return false;
		}

		bool EnemiesAreAlive() {
			foreach (Combatant c in m_Combatants) {
				if (c.isAlive && c.m_Team == Team.Enemy) {
					return true;
				}
			}
			return false;
		}

		private void Start() {
			StartCoroutine(BattleLoop());
		}

		private IEnumerator BattleLoop() {
			m_BattleActive = true;
			RaiseBattleEvent(BattleEventType.BattleStarted, "Battle started!");

			var turnQueue = new Queue<Combatant>();

			while (PlayersAreAlive() && EnemiesAreAlive()) {
				FillTurnQueueEvenly(turnQueue);

				while (turnQueue.Count > 0 && PlayersAreAlive() && EnemiesAreAlive()) {
					var next = turnQueue.Dequeue();

					if (next.isAlive) {
						yield return StartCoroutine(TakeTurn(next));
					}
				}

				DisplayTurnOrder();
				yield return new WaitForSeconds(m_TickRate);
			}

			m_BattleActive = false;

			BattleOutcome outcome;
			if (PlayersAreAlive() && !EnemiesAreAlive()) {
				outcome = BattleOutcome.Victory;
				RaiseBattleEvent(BattleEventType.BattleEnded, "Victory! All enemies defeated!");
			}
			else if (!PlayersAreAlive() && EnemiesAreAlive()) {
				outcome = BattleOutcome.Defeat;
				RaiseBattleEvent(BattleEventType.BattleEnded, "Defeat! All players are down!");
			}
			else {
				outcome = BattleOutcome.Undefined;
				RaiseBattleEvent(BattleEventType.BattleEnded, "Battle ended unexpectedly!");
			}

			BattleEnded?.Invoke(outcome);
		}

		private void FillTurnQueueEvenly(Queue<Combatant> turnQueue) {
			var aliveCombatants = m_Combatants.Where(c => c.isAlive).ToList();
			if (aliveCombatants.Count == 0) return;

			int totalSpeed = aliveCombatants.Sum(c => c.m_Speed);

			var combatantList = new List<CombatantData>();
			foreach (var combatant in aliveCombatants) {
				combatantList.Add(new CombatantData {
					Combatant = combatant,
					Speed = combatant.m_Speed,
					Step = (double)totalSpeed / combatant.m_Speed,
					Position = 0.0
				});
			}

			foreach (var data in combatantList) {
				data.Position = data.Step / 2;
			}

			for (int i = 0; i < totalSpeed; i++) {
				var nextData = combatantList.OrderBy(d => d.Position).First();
				turnQueue.Enqueue(nextData.Combatant);
				nextData.Position += nextData.Step;
			}
		}

		private class CombatantData {
			public Combatant Combatant { get; set; }
			public int Speed { get; set; }
			public double Step { get; set; }
			public double Position { get; set; }
		}

		private IEnumerator TakeTurn(Combatant c) {
			if (c == null) yield break;

			if (!c.isAlive) {
				RaiseBattleEvent(BattleEventType.TurnSkipped, $"{c.m_CombatantName} was dead at turn start — skipping.", c);
				yield break;
			}

			RaiseTurnEvent(TurnEventType.TurnStarted, c);

			foreach (var combatant in m_Combatants) {
				if (combatant.isAlive) {
					combatant.AdvanceDefendTurn();
					combatant.AdvanceAttackBuffTurn();
				}
			}

			if (c.m_Team == Team.Player) {
				yield return StartCoroutine(HandlePlayerTurn(c));
			}
			else {
				yield return StartCoroutine(HandleAITurn(c));
			}

			if (!PlayersAreAlive() || !EnemiesAreAlive()) {
				RaiseBattleEvent(BattleEventType.BattleEndConditionMet, $"After {c.m_CombatantName}'s action, battle end condition met.", c);
				yield break;
			}

			RaiseTurnEvent(TurnEventType.TurnEnded, c);
			yield return new WaitForSeconds(0.5f);
		}

		private IEnumerator HandlePlayerTurn(Combatant player) {
			var availableMoves = player.m_Moves?.Where(m => m != null).ToList() ?? new List<BattleAction>();
			var validTargets = GetValidTargets(player);

			if (availableMoves.Count == 0) {
				RaiseBattleEvent(BattleEventType.NoMovesAvailable, $"{player.m_CombatantName} has no moves. Ending turn.", player);
				yield break;
			}

			if (validTargets.Count == 0) {
				RaiseBattleEvent(BattleEventType.NoValidTargets, $"No valid targets found for {player.m_CombatantName}. Ending turn.", player);
				yield break;
			}

			var inputData = new PlayerInputData {
				Actor = player,
				AvailableMoves = availableMoves,
				ValidTargets = validTargets,
				Timestamp = Time.time
			};

			// set waiting state and current actor/targets
			m_WaitingForPlayerInput = true;
			m_CurrentPlayerActor = player;
			m_CurrentValidTargets = validTargets;
			m_PendingPlayerCommand = null; // clear any previous pending command

			// Fire both legacy and new style events so UI can subscribe either way
			PlayerInputRequired?.Invoke(inputData);
			OnPlayerActionRequested?.Invoke(player, availableMoves);

			// Wait until the UI submits a command (SubmitPlayerCommand) or uses the legacy SubmitPlayerInput
			yield return new WaitUntil(() => !m_WaitingForPlayerInput);

			// If the UI submitted a BattleCommand, execute it; otherwise fallback to random selection
			if (m_PendingPlayerCommand != null) {
				yield return StartCoroutine(ExecuteSubmittedCommand(m_PendingPlayerCommand));
				m_PendingPlayerCommand = null;
			}
			else {
				yield return StartCoroutine(ExecutePlayerAction(player, availableMoves, validTargets));
			}

			// clear current actor/targets
			m_CurrentPlayerActor = null;
			m_CurrentValidTargets = null;
		}

		private IEnumerator ExecuteSubmittedCommand(BattleCommand cmd) {
			if (cmd == null) yield break;

			// Validate the command a little
			if (cmd.Actor == null || cmd.BattleAction == null || cmd.Target == null) {
				RaiseBattleEvent(BattleEventType.CommandError, $"Submitted command invalid for {cmd.Actor?.m_CombatantName ?? "null actor"}", cmd.Actor, cmd.Target);
				yield break;
			}

			// Fire action selected
			RaiseTurnEvent(TurnEventType.ActionSelected, cmd.Actor, cmd.Target, cmd.BattleAction);

			List<BattleResult> results = null;
			try {
				results = MoveResolver.Resolve(cmd, m_Combatants);
			}
			catch (System.Exception ex) {
				RaiseBattleEvent(BattleEventType.ResolutionError, $"MoveResolver.Resolve threw for {cmd.Actor.m_CombatantName}: {ex}", cmd.Actor);
				yield break;
			}

			if (results != null) {
				foreach (var result in results) {
					try {
						MoveResolved?.Invoke(result);
					}
					catch (System.Exception ex) {
						RaiseBattleEvent(BattleEventType.EventHandlerError, $"MoveResolved handler threw for {cmd.Actor.m_CombatantName}: {ex}", cmd.Actor);
					}
				}
			}

			yield return null;
		}

		private IEnumerator ExecutePlayerAction(Combatant player, List<BattleAction> availableMoves, List<Combatant> validTargets) {
			// fallback/random selection for when no player command was submitted
			var action = availableMoves[Random.Range(0, availableMoves.Count)];
			var target = validTargets[Random.Range(0, validTargets.Count)];

			yield return StartCoroutine(ExecuteAction(player, action, target));
		}

		private IEnumerator HandleAITurn(Combatant ai) {
			var validTargets = GetValidTargets(ai);
			if (validTargets.Count == 0) {
				RaiseBattleEvent(BattleEventType.NoValidTargets, $"No valid targets found for {ai.m_CombatantName}. Ending turn.", ai);
				yield break;
			}

			if (ai.m_Moves == null || ai.m_Moves.Count == 0) {
				RaiseBattleEvent(BattleEventType.NoMovesAvailable, $"{ai.m_CombatantName} has no moves. Ending turn.", ai);
				yield break;
			}

			var (action, target) = ai.DecideAIAction(validTargets);

			if (action == null || target == null) {
				RaiseBattleEvent(BattleEventType.NoValidTargets, $"AI could not decide action for {ai.m_CombatantName}. Ending turn.", ai);
				yield break;
			}

			yield return StartCoroutine(ExecuteAction(ai, action, target));
		}

		private IEnumerator ExecuteAction(Combatant actor, BattleAction action, Combatant target) {
			if (action.m_IsMultiTurn && action.m_TurnCost > 1) {
				actor.StartMultiTurnAction(action, target);
				RaiseTurnEvent(TurnEventType.MultiTurnStarted, actor, target, action);
			}
			else {
				BattleCommand cmd;
				try {
					cmd = new BattleCommand(actor, action, target);
				}
				catch (System.Exception ex) {
					RaiseBattleEvent(BattleEventType.CommandError, $"Exception constructing BattleCommand for {actor.m_CombatantName}: {ex}", actor);
					yield break;
				}

				RaiseTurnEvent(TurnEventType.ActionSelected, actor, target, action);

				List<BattleResult> results = null;
				try {
					results = MoveResolver.Resolve(cmd, m_Combatants);
				}
				catch (System.Exception ex) {
					RaiseBattleEvent(BattleEventType.ResolutionError, $"MoveResolver.Resolve threw for {actor.m_CombatantName}: {ex}", actor);
					yield break;
				}

				if (results != null) {
					foreach (var result in results) {
						try {
							MoveResolved?.Invoke(result);
						}
						catch (System.Exception ex) {
							RaiseBattleEvent(BattleEventType.EventHandlerError, $"MoveResolved handler threw for {actor.m_CombatantName}: {ex}", actor);
						}
					}
				}
			}
		}

		private List<Combatant> GetValidTargets(Combatant attacker) {
			var validTargets = new List<Combatant>();

			foreach (var combatant in m_Combatants) {
				if (!combatant.isAlive || combatant == attacker)
					continue;

				if (attacker.m_Team != combatant.m_Team) {
					validTargets.Add(combatant);
				}
			}

			return validTargets;
		}

		private void DisplayTurnOrder() {
			var upcoming = GetUpcomingTurns();
			string queue = string.Join(" -> ", upcoming.Select(c => $"{c.m_CombatantName} ({c.m_TurnGauge:0})"));
			RaiseBattleEvent(BattleEventType.TurnOrderUpdated, $"Upcoming Turns: {queue}");
			OnTurnOrderUpdated?.Invoke(upcoming);
		}

		public List<Combatant> GetUpcomingTurns() {
			var tempList = m_Combatants
			.Where(c => c.isAlive)
			.Select(c => new { combatant = c, gauge = c.m_TurnGauge })
			.ToList();

			List<Combatant> upcoming = new List<Combatant>();

			for (int i = 0; i < m_LookaheadTurns; i++) {
				if (tempList.Count == 0) break;

				var next = tempList.OrderBy(c => (m_GaugeThreshold - c.gauge) / c.combatant.m_Speed).First();
				upcoming.Add(next.combatant);

				float timeToAct = (m_GaugeThreshold - next.gauge) / next.combatant.m_Speed;

				for (int j = 0; j < tempList.Count; j++)
					tempList[j] = new { combatant = tempList[j].combatant, gauge = tempList[j].gauge + tempList[j].combatant.m_Speed * timeToAct };

				int idx = tempList.FindIndex(t => t.combatant == next.combatant);
				tempList[idx] = new { combatant = next.combatant, gauge = tempList[idx].gauge - m_GaugeThreshold };
			}

			return upcoming;
		}
	}

	public struct PlayerInputData {
		public Combatant Actor;
		public List<BattleAction> AvailableMoves;
		public List<Combatant> ValidTargets;
		public float Timestamp;
	}

	public struct PlayerInputResponse {
		public BattleAction SelectedAction;
		public Combatant SelectedTarget;
		public bool IsValid;
		public string ValidationMessage;
	}

	public enum BattleEventType {
		BattleStarted,
		BattleEnded,
		TurnSkipped,
		NoValidTargets,
		NoMovesAvailable,
		CommandError,
		ResolutionError,
		EventHandlerError,
		BattleEndConditionMet,
		TurnOrderUpdated,
		TargetChanged
	}

	public enum TurnEventType {
		TurnStarted,
		ActionSelected,
		MultiTurnStarted,
		TurnEnded
	}

	public enum BattleOutcome {
		Victory,
		Defeat,
		Undefined
	}

	public struct BattleEventData {
		public BattleEventType EventType;
		public string Message;
		public Combatant Combatant;
		public Combatant Target;
		public float Timestamp;
	}

	public struct TurnEventData {
		public TurnEventType EventType;
		public Combatant Combatant;
		public Combatant Target;
		public BattleAction Action;
		public float Timestamp;
	}
}