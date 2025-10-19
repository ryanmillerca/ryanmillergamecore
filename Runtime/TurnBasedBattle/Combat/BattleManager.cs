namespace RyanMillerGameCore.TurnBasedCombat {
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class BattleManager : MonoBehaviour {
		[Header("Combatants")]
		public List<Combatant> m_Combatants = new List<Combatant>();

		[Header("Turn Settings")]
		public float m_GaugeThreshold = 100f;
		public float m_TickRate = 1f;
		public int m_LookaheadTurns = 5;

        #pragma warning disable CS0414
		private bool m_BattleActive = false;

		// Event declarations
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

		// Team-based victory conditions
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

			// Battle continues while both teams have living members
			while (PlayersAreAlive() && EnemiesAreAlive()) {
				// Recalculate and fill turn queue every cycle
				FillTurnQueueEvenly(turnQueue);

				// Process turns from the queue
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

			// Determine battle outcome
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

			// Calculate total speed and create combatant data
			int totalSpeed = aliveCombatants.Sum(c => c.m_Speed);

			// Create a list to hold combatant data with position tracking
			var combatantList = new List<CombatantData>();
			foreach (var combatant in aliveCombatants) {
				combatantList.Add(new CombatantData {
					Combatant = combatant,
					Speed = combatant.m_Speed,
					Step = (double)totalSpeed / combatant.m_Speed, // Ideal spacing between turns
					Position = 0.0
				});
			}

			// Initialize positions in the middle of their first segment
			foreach (var data in combatantList) {
				data.Position = data.Step / 2;
			}

			// Generate turns
			for (int i = 0; i < totalSpeed; i++) {
				// Find combatant with the smallest position (should act next)
				var nextData = combatantList.OrderBy(d => d.Position).First();
				turnQueue.Enqueue(nextData.Combatant);

				// Move this combatant to their next turn position
				nextData.Position += nextData.Step;
			}
		}

		// Helper class to track combatant turn data
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

			// Get valid targets based on team affiliation
			var validTargets = GetValidTargets(c);
			if (validTargets.Count == 0) {
				RaiseBattleEvent(BattleEventType.NoValidTargets, $"No valid targets found for {c.m_CombatantName}. Ending turn.", c);
				yield break;
			}

			// Safety: make sure there is at least one move
			if (c.m_Moves == null || c.m_Moves.Count == 0) {
				RaiseBattleEvent(BattleEventType.NoMovesAvailable, $"{c.m_CombatantName} has no moves. Ending turn.", c);
				yield break;
			}

			// Pick a random valid target
			var target = validTargets[Random.Range(0, validTargets.Count)];
			var action = c.m_Moves[0];

			BattleCommand cmd;
			try {
				cmd = new BattleCommand(c, action, target);
			}
			catch (System.Exception ex) {
				RaiseBattleEvent(BattleEventType.CommandError, $"Exception constructing BattleCommand for {c.m_CombatantName}: {ex}", c);
				yield break;
			}

			RaiseTurnEvent(TurnEventType.ActionSelected, c, target, action);

			// Resolve the move (this is where damage/heal/buffs are applied)
			List<BattleResult> results = null;
			try {
				results = MoveResolver.Resolve(cmd, m_Combatants);
			}
			catch (System.Exception ex) {
				RaiseBattleEvent(BattleEventType.ResolutionError, $"MoveResolver.Resolve threw for {c.m_CombatantName}: {ex}", c);
				yield break;
			}

			// If resolve somehow killed the last enemy, bail out early (prevents further processing)
			if (!PlayersAreAlive() || !EnemiesAreAlive()) {
				RaiseBattleEvent(BattleEventType.BattleEndConditionMet, $"After {c.m_CombatantName}'s action, battle end condition met. Ending turn early.", c);
				yield break;
			}

			// Fire events for each affected target — guard subscriber exceptions so the coroutine continues
			if (results != null) {
				foreach (var result in results) {
					try {
						MoveResolved?.Invoke(result);
					}
					catch (System.Exception ex) {
						RaiseBattleEvent(BattleEventType.EventHandlerError, $"MoveResolved handler threw for {c.m_CombatantName}: {ex}", c);
						// continue so one bad subscriber doesn't kill the coroutine
					}
				}
			}

			RaiseTurnEvent(TurnEventType.TurnEnded, c);
			yield return new WaitForSeconds(0.5f); // placeholder for animations
		}

		// Method to get valid targets based on team
		private List<Combatant> GetValidTargets(Combatant attacker) {
			var validTargets = new List<Combatant>();

			foreach (var combatant in m_Combatants) {
				if (!combatant.isAlive || combatant == attacker)
					continue;

				// Players attack enemies, enemies attack players
				// Same team members don't attack each other
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

	// Event data structures
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
		TurnOrderUpdated
	}

	public enum TurnEventType {
		TurnStarted,
		ActionSelected,
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
