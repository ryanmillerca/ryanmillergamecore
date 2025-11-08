namespace RyanMillerGameCore.TurnBasedCombat.Brains {
	using System.Collections.Generic;
	using UnityEngine;

	[CreateAssetMenu(menuName = "Battle/AI/Healer Brain")]
	public class HealerAIBrain : EnemyAIBrain {
		[Header("Healer Behavior")]
		public float healThreshold = 0.5f; // Heal when allies are below 50% HP

		public override (BattleAction action, Combatant target) ChooseAction(Combatant self, List<Combatant> validTargets, List<BattleAction> availableMoves) {
			if (availableMoves.Count == 0 || validTargets.Count == 0)
				return (null, null);

			// Check if any allies need healing
			var allies = GetAllyTargets(self, validTargets);
			var injuredAllies = GetInjuredAllies(allies, healThreshold);

			if (injuredAllies.Count > 0) {
				// Try to find a heal move
				var healMoves = availableMoves.FindAll(m => m.ActionType == ActionType.Heal);
				if (healMoves.Count > 0) {
					var mostInjuredAlly = GetMostInjuredAlly(injuredAllies);
					return (healMoves[0], mostInjuredAlly);
				}
			}

			// No healing needed or no heal moves available, use random target selection
			var randomAction = availableMoves[Random.Range(0, availableMoves.Count)];
			var randomTarget = validTargets[Random.Range(0, validTargets.Count)];

			return (randomAction, randomTarget);
		}

		private List<Combatant> GetAllyTargets(Combatant self, List<Combatant> allTargets) {
			var allies = new List<Combatant>();
			foreach (var target in allTargets) {
				if (target.Team == self.Team && target != self)
					allies.Add(target);
			}
			return allies;
		}

		private List<Combatant> GetInjuredAllies(List<Combatant> allies, float threshold) {
			var injured = new List<Combatant>();
			foreach (var ally in allies) {
				float healthPercent = (float)ally.CurrentHp / ally.MaxHp;
				if (healthPercent < threshold)
					injured.Add(ally);
			}
			return injured;
		}

		private Combatant GetMostInjuredAlly(List<Combatant> injuredAllies) {
			Combatant mostInjured = injuredAllies[0];
			foreach (var ally in injuredAllies) {
				if (ally.CurrentHp < mostInjured.CurrentHp)
					mostInjured = ally;
			}
			return mostInjured;
		}
	}
}
