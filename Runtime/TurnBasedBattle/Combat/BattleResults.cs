namespace RyanMillerGameCore.TurnBasedBattle
{
	public class BattleResult
	{
		public Combatant Actor;
		public Combatant Target;
		public BattleActionDef BattleAction;

		public int DamageDealt;
		public int HealingDone;
		public bool Missed;
		public bool CriticalHit;

		public string Message;
	}
}
