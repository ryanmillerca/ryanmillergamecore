namespace RyanMillerGameCore.TurnBasedCombat.Brains {
	using System.Collections.Generic;
	using UnityEngine;

	[CreateAssetMenu(menuName = "Battle/AI/Healer Brain")]
	public class HealerAIBrain : EnemyAIBrain {
		[Header("Healer Behavior")]
		public float healThreshold = 0.5f; // Heal when allies are below 50% HP

		public override (BattleAction action, Combatant target) ChooseAction(Combatant self, List<Combatant> validTargets, List<BattleAction> availableMoves) {

			if (availableMoves.Count == 0)
				return (null, null);

			// Get all allies including self from the BattleManager
			List<Combatant> allAllies = new List<Combatant>();
			foreach (var c in this.BattleManager.Combatants) {
				if (c.Team == self.Team && c.isAlive)
					allAllies.Add(c);
			}

			// Find injured allies
			var healMoves = availableMoves.FindAll(m => m.ActionType == ActionType.Heal);
			if (healMoves.Count > 0) {
				List<Combatant> injuredAllies = allAllies.FindAll(a => ((float)a.CurrentHp / a.MaxHp) < healThreshold);
				if (injuredAllies.Count > 0) {
					// Pick the most injured ally
					Combatant mostInjured = injuredAllies[0];
					foreach (var a in injuredAllies) {
						if (a.CurrentHp < mostInjured.CurrentHp)
							mostInjured = a;
					}
					return (healMoves[0], mostInjured);
				}
			}

			// No healing needed — pick random damage move against enemy
			var damageMoves = availableMoves.FindAll(m => m.ActionType == ActionType.Damage);
			if (damageMoves.Count > 0) {
				var randomMove = damageMoves[Random.Range(0, damageMoves.Count)];
				List<Combatant> enemies = this.BattleManager.Combatants.FindAll(c => c.Team != self.Team && c.isAlive);
				if (enemies.Count > 0) {
					var target = enemies[Random.Range(0, enemies.Count)];
					return (randomMove, target);
				}
			}

			// Fallback
			return (availableMoves[0], self);
		}
		
		private List<Combatant> GetAllyTargets(Combatant self, List<Combatant> allTargets) {
			var allies = new List<Combatant>();
			foreach (var target in allTargets) {
				// Include self + any living allies on the same team
				if (target.Team == self.Team && target.isAlive)
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
