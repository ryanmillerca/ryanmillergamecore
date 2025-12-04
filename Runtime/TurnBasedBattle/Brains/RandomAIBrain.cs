namespace RyanMillerGameCore.TurnBasedCombat.Brains {
	using System.Collections.Generic;
	using UnityEngine;

	[CreateAssetMenu(menuName = "Battle/AI/Random Brain")]
	public class RandomAIBrain : EnemyAIBrain {
		public override (BattleAction action, Combatant target) ChooseAction(Combatant self, List<Combatant> validTargets, List<BattleAction> availableMoves) {
			if (availableMoves.Count == 0 || validTargets.Count == 0)
				return (null, null);

			var randomAction = availableMoves[Random.Range(0, availableMoves.Count)];
			var randomTarget = validTargets[Random.Range(0, validTargets.Count)];

			return (randomAction, randomTarget);
		}
	}
}
