namespace RyanMillerGameCore.TurnBasedCombat
{
	[System.Serializable]
	public class BattleResult
	{
		public Combatant Actor;
		public Combatant Target;
		public BattleAction BattleAction;
		public string Message;
		public int DamageDealt;
		public int HealingDone;
		public bool CriticalHit;
		public bool Missed;
		public bool Success;
		public float CriticalChance;
		public bool IsChargedAction;
		public float ChargeMultiplier;
	}
}
