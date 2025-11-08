namespace RyanMillerGameCore.TurnBasedCombat.Brains {
	using System.Collections.Generic;
	using UnityEngine;

	[CreateAssetMenu(menuName = "Battle/AI/Aggressive Brain")]
	public class AggressiveAIBrain : EnemyAIBrain {
		[Header("Aggressive Behavior")]
		public float damageMoveWeight = 2f;
		public float healMoveWeight = 0.5f;
		public float buffMoveWeight = 0.8f;
		public float defendMoveWeight = 0.3f;

		public override (BattleAction action, Combatant target) ChooseAction(Combatant self, List<Combatant> validTargets, List<BattleAction> availableMoves) {
			if (availableMoves.Count == 0 || validTargets.Count == 0)
				return (null, null);

			// Weight moves by type
			var weightedMoves = new List<(BattleAction action, float weight)>();
			foreach (var move in availableMoves) {
				float weight = GetMoveWeight(move);
				weightedMoves.Add((move, weight));
			}

			// Select weighted random move
			var selectedMove = WeightedRandomSelection(weightedMoves);

			// For aggressive AI, always target lowest HP enemy
			var target = GetLowestHPTarget(validTargets);

			return (selectedMove, target);
		}

		private float GetMoveWeight(BattleAction move) {
			return move.ActionType switch {
				ActionType.Damage => damageMoveWeight,
				ActionType.Heal => healMoveWeight,
				ActionType.Buff => buffMoveWeight,
				ActionType.Debuff => buffMoveWeight,
				ActionType.Defend => defendMoveWeight,
				_ => 1f
			};
		}

		private BattleAction WeightedRandomSelection(List<(BattleAction action, float weight)> weightedMoves) {
			float totalWeight = 0f;
			foreach (var move in weightedMoves) {
				totalWeight += move.weight;
			}

			float randomValue = Random.Range(0f, totalWeight);
			float currentWeight = 0f;

			foreach (var move in weightedMoves) {
				currentWeight += move.weight;
				if (randomValue <= currentWeight)
					return move.action;
			}

			return weightedMoves[0].action;
		}

		private Combatant GetLowestHPTarget(List<Combatant> targets) {
			Combatant lowestHPTarget = targets[0];
			foreach (var target in targets) {
				if (target.CurrentHp < lowestHPTarget.CurrentHp)
					lowestHPTarget = target;
			}
			return lowestHPTarget;
		}
	}
}
