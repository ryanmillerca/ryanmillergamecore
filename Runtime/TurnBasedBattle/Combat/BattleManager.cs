using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

		public delegate void OnMoveResolved(BattleResult result);
		public event OnMoveResolved MoveResolved;

		public delegate void OnBattleEvent(BattleEventData eventData);
		public event OnBattleEvent BattleEvent;

		public delegate void OnTurnEvent(TurnEventData eventData);
		public event OnTurnEvent TurnEvent;

		public delegate void OnBattleOutcome(BattleOutcome outcome);
		public event OnBattleOutcome BattleEnded;

		private void Start() {
			StartCoroutine(BattleLoop());
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

			// Advance defend turns for all combatants at the start of each turn
			foreach (var combatant in m_Combatants) {
				if (combatant.isAlive) {
					combatant.AdvanceDefendTurn();
				}
			}

			if (c.isCharging && c.currentMultiTurnAction != null) {
				bool actionReady = c.AdvanceMultiTurnAction();

				if (actionReady) {
					var multiTurnAction = c.currentMultiTurnAction;

					Combatant finalTarget = multiTurnAction.target;
					if (finalTarget == null || !finalTarget.isAlive) {
						var validTargets = GetValidTargets(c);
						if (validTargets.Count > 0) {
							finalTarget = validTargets[Random.Range(0, validTargets.Count)];
							RaiseBattleEvent(BattleEventType.TargetChanged,
								$"{c.m_CombatantName}'s {multiTurnAction.action.m_ActionName} retargeted to {finalTarget.m_CombatantName}!", c, finalTarget);
						}
						else {
							RaiseBattleEvent(BattleEventType.NoValidTargets,
								$"{c.m_CombatantName}'s {multiTurnAction.action.m_ActionName} has no valid targets! Action wasted.", c);
							c.CompleteMultiTurnAction();
							yield break;
						}
					}

					BattleCommand cmd = new BattleCommand(c, multiTurnAction.action, finalTarget);

					RaiseTurnEvent(TurnEventType.ActionSelected, c, finalTarget, multiTurnAction.action);

					List<BattleResult> results = null;
					try {
						results = MoveResolver.Resolve(cmd, m_Combatants, true, multiTurnAction.action.m_ChargeMultiplier);
					}
					catch (System.Exception ex) {
						RaiseBattleEvent(BattleEventType.ResolutionError, $"MoveResolver.Resolve threw for {c.m_CombatantName}: {ex}", c);
						c.CompleteMultiTurnAction();
						yield break;
					}

					c.CompleteMultiTurnAction();

					if (results != null) {
						foreach (var result in results) {
							try {
								MoveResolved?.Invoke(result);
							}
							catch (System.Exception ex) {
								RaiseBattleEvent(BattleEventType.EventHandlerError, $"MoveResolved handler threw for {c.m_CombatantName}: {ex}", c);
							}
						}
					}
				}
			}
			else {
				var validTargets = GetValidTargets(c);
				if (validTargets.Count == 0) {
					RaiseBattleEvent(BattleEventType.NoValidTargets, $"No valid targets found for {c.m_CombatantName}. Ending turn.", c);
					yield break;
				}

				if (c.m_Moves == null || c.m_Moves.Count == 0) {
					RaiseBattleEvent(BattleEventType.NoMovesAvailable, $"{c.m_CombatantName} has no moves. Ending turn.", c);
					yield break;
				}

				var action = c.m_Moves[Random.Range(0, c.m_Moves.Count)];
				var target = validTargets[Random.Range(0, validTargets.Count)];

				if (action.m_IsMultiTurn && action.m_TurnCost > 1) {
					c.StartMultiTurnAction(action, target);
					RaiseTurnEvent(TurnEventType.MultiTurnStarted, c, target, action);
				}
				else {
					BattleCommand cmd;
					try {
						cmd = new BattleCommand(c, action, target);
					}
					catch (System.Exception ex) {
						RaiseBattleEvent(BattleEventType.CommandError, $"Exception constructing BattleCommand for {c.m_CombatantName}: {ex}", c);
						yield break;
					}

					RaiseTurnEvent(TurnEventType.ActionSelected, c, target, action);

					List<BattleResult> results = null;
					try {
						results = MoveResolver.Resolve(cmd, m_Combatants);
					}
					catch (System.Exception ex) {
						RaiseBattleEvent(BattleEventType.ResolutionError, $"MoveResolver.Resolve threw for {c.m_CombatantName}: {ex}", c);
						yield break;
					}

					if (results != null) {
						foreach (var result in results) {
							try {
								MoveResolved?.Invoke(result);
							}
							catch (System.Exception ex) {
								RaiseBattleEvent(BattleEventType.EventHandlerError, $"MoveResolved handler threw for {c.m_CombatantName}: {ex}", c);
							}
						}
					}
				}
			}

			if (!PlayersAreAlive() || !EnemiesAreAlive()) {
				RaiseBattleEvent(BattleEventType.BattleEndConditionMet, $"After {c.m_CombatantName}'s action, battle end condition met.", c);
				yield break;
			}

			RaiseTurnEvent(TurnEventType.TurnEnded, c);
			yield return new WaitForSeconds(0.5f);
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
