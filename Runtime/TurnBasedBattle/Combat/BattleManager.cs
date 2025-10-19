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

				foreach (var c in readyToAct) {
					if (!c.isAlive) {
						Debug.Log($"{c.m_CombatantName} died before their turn; skipping.");
						continue;
					}

					Debug.Log($"Starting turn coroutine for {c.m_CombatantName} (gauge={c.m_TurnGauge})");
					yield return StartCoroutine(TakeTurn(c));

					// make sure they still exist/alive? subtract gauge regardless, or only if alive?
					c.m_TurnGauge -= m_GaugeThreshold;
				}

				DisplayTurnOrder();
				yield return new WaitForSeconds(m_TickRate);
			}

			m_BattleActive = false;
			Debug.Log("Battle ended!");
		}

		private IEnumerator TakeTurn(Combatant c) {
			if (c == null) {
				yield break;
			}

			if (!c.isAlive) {
				Debug.LogWarning($"TakeTurn: {c.m_CombatantName} was dead at turn start — skipping.");
				yield break;
			}

			Debug.Log($"TakeTurn started for {c?.m_CombatantName ?? "NULL"} (alive={c?.isAlive}).");

			// Pick first alive target that is not self
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

			List<BattleResult> results = null;
			try {
				results = MoveResolver.Resolve(cmd, m_Combatants);
			}
			catch (System.Exception ex) {
				Debug.LogError($"TakeTurn: MoveResolver.Resolve threw for {c.m_CombatantName}: {ex}");
				yield break;
			}

			// This log should always run after Resolve (unless an exception happened)
			Debug.Log($"It's {c.m_CombatantName}'s turn. They {cmd.BattleAction?.m_ActionName ?? "NULL ACTION"} with target {cmd.Target?.m_CombatantName ?? "NULL"}.");

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
