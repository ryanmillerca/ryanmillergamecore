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

			while (EnemiesAreAlive() && CombatantsAreAlive()) {
				// Tick gauges
				foreach (var c in m_Combatants.Where(c => c.isAlive)) {
					c.m_TurnGauge += c.m_Speed * m_TickRate;
					c.m_TurnGauge = Mathf.Min(c.m_TurnGauge, m_GaugeThreshold);
				}

				// Process ready combatants
				var readyToAct = m_Combatants
				.Where(c => c.isAlive && c.m_TurnGauge >= m_GaugeThreshold)
				.OrderByDescending(c => c.m_TurnGauge)
				.ToList();

				// Process ready combatants (dynamic, recalculated each iteration)
				while (true) {
					// If no enemies or not enough combatants, stop before selecting anyone
					if (!EnemiesAreAlive() || !CombatantsAreAlive()) {
						Debug.Log("Battle condition met before selecting next actor — stopping action processing.");
						break;
					}

					// pick the next ready-to-act combatant (highest gauge, alive, >= threshold)
					var next = m_Combatants
					.Where(c => c.isAlive && c.m_TurnGauge >= m_GaugeThreshold)
					.OrderByDescending(c => c.m_TurnGauge)
					.FirstOrDefault();

					if (next == null) {
						// Nothing ready right now
						break;
					}

					Debug.Log($"[BattleLoop] Next actor: {next.m_CombatantName} (gauge={next.m_TurnGauge}, alive={next.isAlive}). EnemiesAlive={EnemiesAreAlive()}. CombatantsAliveCount={m_Combatants.Count(c => c.isAlive)}");

					// Start their turn
					yield return StartCoroutine(TakeTurn(next));

					// After the action resolved, check if the battle ended
					if (!EnemiesAreAlive() || !CombatantsAreAlive()) {
						Debug.Log("Battle condition met after action — stopping further turns this tick.");
						break;
					}

					// Subtract gauge only if they still exist (defensive)
					if (next != null) {
						next.m_TurnGauge -= m_GaugeThreshold;
					}
				}

				DisplayTurnOrder();
				yield return new WaitForSeconds(m_TickRate);
			}

			m_BattleActive = false;
			Debug.Log("Battle ended!");
		}

		private IEnumerator TakeTurn(Combatant c) {
			if (c == null) yield break;

			if (!c.isAlive) {
				Debug.LogWarning($"TakeTurn: {c?.m_CombatantName ?? "NULL"} was dead at turn start — skipping.");
				yield break;
			}

			Debug.Log($"TakeTurn started for {c.m_CombatantName} (alive={c.isAlive}).");

			// Pick first alive target that is not self (snapshot target for this action)
			var target = m_Combatants.FirstOrDefault(t => t.isAlive && t != c);
			if (target == null) {
				Debug.LogWarning($"TakeTurn: no valid target found for {c.m_CombatantName}. Ending turn.");
				yield break;
			}

			// Safety: make sure there is at least one move
			if (c.m_Moves == null || c.m_Moves.Count == 0) {
				Debug.LogWarning($"TakeTurn: {c.m_CombatantName} has no moves. Ending turn.");
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
			Debug.Log($"It's {c.m_CombatantName}'s turn. They {cmd.BattleAction?.m_ActionName ?? "NULL ACTION"} with target {cmd.Target?.m_CombatantName ?? "NULL"}.");

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
			Debug.Log($"[Resolve] Results={results?.Count ?? 0}, DamageResults={damageCount}, HealResults={healCount}, AliveCombatants={m_Combatants.Count(x => x.isAlive)}, EnemiesAlive={m_Combatants.Count(x => x.isAlive && !x.m_IsPlayer)}");

			// If resolve somehow killed the last enemy, bail out early (prevents further processing)
			if (!EnemiesAreAlive() || !CombatantsAreAlive()) {
				Debug.Log($"After {c.m_CombatantName}'s action, battle end condition met. Ending turn early.");
				// subtract gauge because they did act (optional—choose your rule)
				c.m_TurnGauge -= m_GaugeThreshold;
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

			// Subtract gauge after everything finished for this actor
			c.m_TurnGauge -= m_GaugeThreshold;

			yield return new WaitForSeconds(0.5f); // placeholder for animations
		}

		private void DisplayTurnOrder() {
			var upcoming = GetUpcomingTurns();
			string queue = string.Join(" -> ", upcoming.Select(c => $"{c.m_CombatantName} ({c.m_TurnGauge:0})"));
			//     Debug.Log("Upcoming Turns: " + queue);
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
