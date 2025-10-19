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

		public delegate void OnMoveResolved(BattleResult result);
		public event OnMoveResolved MoveResolved;

		private void Start() {
			StartCoroutine(BattleLoop());
		}

		bool EnemiesAreAlive() {
			foreach (Combatant c in m_Combatants) {
				if (c.isAlive && c.m_IsPlayer == false) {
					return true;
				}
			}
			return false;
		}

		bool CombatantsAreAlive() {
			int aliveCombatants = 0;
			foreach (Combatant c in m_Combatants) {
				if (c.isAlive) {
					aliveCombatants++;
				}
			}
			return aliveCombatants > 1;
		}

		private IEnumerator BattleLoop() {
			m_BattleActive = true;

			var turnQueue = new Queue<Combatant>();

			while (EnemiesAreAlive() && CombatantsAreAlive()) {
				// Recalculate and fill turn queue every cycle
				FillTurnQueueEvenly(turnQueue);

				// Process turns from the queue
				while (turnQueue.Count > 0 && EnemiesAreAlive() && CombatantsAreAlive()) {
					var next = turnQueue.Dequeue();

					if (next.isAlive) {
//						Debug.Log($"<color={next.ColorAsHex}>Starting turn coroutine for {next.m_CombatantName}</color>");
						yield return StartCoroutine(TakeTurn(next));
					}
				}

				DisplayTurnOrder();
				yield return new WaitForSeconds(m_TickRate);
			}

			m_BattleActive = false;
			Debug.Log("Battle ended!");
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
				Debug.LogWarning($"<color={c.ColorAsHex}>TakeTurn: {c?.m_CombatantName ?? "NULL"} was dead at turn start — skipping.<color>");
				yield break;
			}

//			Debug.Log($"<color={c.ColorAsHex}>TakeTurn started for {c.m_CombatantName} (alive={c.isAlive}).</color>");

			// Pick first alive target that is not self (snapshot target for this action)
			var target = m_Combatants.FirstOrDefault(t => t.isAlive && t != c);
			if (target == null) {
				Debug.LogWarning($"<color={c.ColorAsHex}>TakeTurn: no valid target found for {c.m_CombatantName}. Ending turn.</color>");
				yield break;
			}

			// Safety: make sure there is at least one move
			if (c.m_Moves == null || c.m_Moves.Count == 0) {
				Debug.LogWarning($"<color={c.ColorAsHex}>TakeTurn: {c.m_CombatantName} has no moves. Ending turn.</color>");
				yield break;
			}

			BattleCommand cmd;
			try {
				cmd = new BattleCommand(c, c.m_Moves[0], target);
			}
			catch (System.Exception ex) {
				Debug.LogError($"TakeTurn: exception constructing BattleCommand for {c.m_CombatantName}: {ex}");
				yield break;
			}

			// LOG: show the player's intention before Resolve so logs read intuitively
			Debug.Log($"<color={c.ColorAsHex}>It's {c.m_CombatantName}'s turn. They {cmd.BattleAction?.m_ActionName ?? "NULL ACTION"} with target {cmd.Target?.m_CombatantName ?? "NULL"}.</color>");

			// Resolve the move (this is where damage/heal/buffs are applied)
			List<BattleResult> results = null;
			try {
				results = MoveResolver.Resolve(cmd, m_Combatants);
			}
			catch (System.Exception ex) {
				Debug.LogError($"TakeTurn: MoveResolver.Resolve threw for {c.m_CombatantName}: {ex}");
				yield break;
			}

			// Diagnostics: how many results, how many dealt damage/healing
			int damageCount = results?.Count(r => r.DamageDealt > 0) ?? 0;
			int healCount = results?.Count(r => r.HealingDone > 0) ?? 0;

			// If resolve somehow killed the last enemy, bail out early (prevents further processing)
			if (!EnemiesAreAlive() || !CombatantsAreAlive()) {
				Debug.Log($"After {c.m_CombatantName}'s action, battle end condition met. Ending turn early.");
				yield break;
			}

			// Fire events for each affected target — guard subscriber exceptions so the coroutine continues
			if (results != null) {
				foreach (var result in results) {
					try {
						MoveResolved?.Invoke(result);
					}
					catch (System.Exception ex) {
						Debug.LogError($"MoveResolved handler threw for {c.m_CombatantName}: {ex}");
						// continue so one bad subscriber doesn't kill the coroutine
					}
				}
			}

			yield return new WaitForSeconds(0.5f); // placeholder for animations
		}

		private void DisplayTurnOrder() {
			var upcoming = GetUpcomingTurns();
			string queue = string.Join(" -> ", upcoming.Select(c => $"{c.m_CombatantName} ({c.m_TurnGauge:0})"));
			// Debug.Log("Upcoming Turns: " + queue);
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
}
