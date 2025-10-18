namespace RyanMillerGameCore.TurnBasedBattle
{
	public class BattleCommand
	{
		public readonly Combatant Actor;
		public readonly BattleActionDef BattleAction;
		public readonly Combatant Target; // primary target for single-target actions

		public BattleCommand(Combatant actor, BattleActionDef action, Combatant target)
		{
			Actor = actor;
			BattleAction = action;
			Target = target;
		}
	}
}
