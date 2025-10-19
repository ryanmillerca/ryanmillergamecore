namespace RyanMillerGameCore.TurnBasedCombat
{
	public class BattleCommand
	{
		public Combatant Actor { get; private set; }
		public BattleAction BattleAction { get; private set; }
		public Combatant Target { get; private set; }

		public BattleCommand(Combatant actor, BattleAction action, Combatant target)
		{
			Actor = actor;
			BattleAction = action;
			Target = target;
		}
	}
}
