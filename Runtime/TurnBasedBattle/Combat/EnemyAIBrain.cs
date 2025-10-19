namespace RyanMillerGameCore.TurnBasedCombat {
	using System.Collections.Generic;
	using UnityEngine;

	abstract public class EnemyAIBrain : ScriptableObject {
		abstract public (BattleAction action, Combatant target) ChooseAction(Combatant self, List<Combatant> validTargets, List<BattleAction> availableMoves);
	}
}
