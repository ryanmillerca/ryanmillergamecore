namespace RyanMillerGameCore.TurnBasedCombat.Brains {
	using System.Collections.Generic;
	using UnityEngine;

	[CreateAssetMenu(menuName = "Battle/AI/Strategic Brain")]
	public class StrategicAIBrain : EnemyAIBrain {
		[Header("Strategic Behavior")]
		[SerializeField] private float m_LowHealthThreshold = 0.3f;
		[SerializeField] private float m_DefendHealthThreshold = 0.4f;

		public override (BattleAction action, Combatant target) ChooseAction(Combatant self, List<Combatant> validTargets, List<BattleAction> availableMoves) {
			if (availableMoves.Count == 0 || validTargets.Count == 0)
				return (null, null);

			// Check self health for defensive actions
			float selfHealthPercent = (float)self.CurrentHp / self.MaxHp;

			if (selfHealthPercent < m_DefendHealthThreshold) {
				// Try to defend or heal
				var defendMoves = availableMoves.FindAll(m => m.ActionType == ActionType.Defend);
				var healMoves = availableMoves.FindAll(m => m.ActionType == ActionType.Heal && m.TargetSelf);

				if (defendMoves.Count > 0 && selfHealthPercent < m_LowHealthThreshold) {
					return (defendMoves[0], self);
				}
				else if (healMoves.Count > 0) {
					return (healMoves[0], self);
				}
			}

			// If healthy, be aggressive but strategic
			var enemyTargets = GetEnemyTargets(self, validTargets);
			var weakestEnemy = GetWeakestEnemy(enemyTargets);

			// Prefer high-damage moves against weak enemies
			var damageMoves = availableMoves.FindAll(m => m.ActionType == ActionType.Damage);
			if (damageMoves.Count > 0 && weakestEnemy != null) {
				var strongestDamageMove = GetStrongestDamageMove(damageMoves);
				return (strongestDamageMove, weakestEnemy);
			}

			// Fallback to random selection
			var randomAction = availableMoves[Random.Range(0, availableMoves.Count)];
			var randomTarget = validTargets[Random.Range(0, validTargets.Count)];

			return (randomAction, randomTarget);
		}

		private List<Combatant> GetEnemyTargets(Combatant self, List<Combatant> allTargets) {
			var enemies = new List<Combatant>();
			foreach (var target in allTargets) {
				if (target.Team != self.Team)
					enemies.Add(target);
			}
			return enemies;
		}

		private Combatant GetWeakestEnemy(List<Combatant> enemies) {
			if (enemies.Count == 0) return null;

			Combatant weakest = enemies[0];
			foreach (var enemy in enemies) {
				if (enemy.CurrentHp < weakest.CurrentHp)
					weakest = enemy;
			}
			return weakest;
		}

		private BattleAction GetStrongestDamageMove(List<BattleAction> damageMoves) {
			BattleAction strongest = damageMoves[0];
			foreach (var move in damageMoves) {
				if (move.Power > strongest.Power)
					strongest = move;
			}
			return strongest;
		}
	}
}
