namespace RyanMillerGameCore.TurnBasedCombat {
	public class BattleCommand {
		public readonly Combatant Actor;
		public readonly BattleAction BattleAction;
		public readonly Combatant Target; // primary target for single-target actions

		public BattleCommand(Combatant actor, BattleAction action, Combatant target) {
			Actor = actor;
			BattleAction = action;
			Target = target;
		}
	}
}
