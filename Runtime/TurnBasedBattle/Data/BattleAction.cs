namespace RyanMillerGameCore.TurnBasedCombat {
	using UnityEngine;

	[CreateAssetMenu(menuName = "Battle/Action")]
	public class BattleAction : ScriptableObject {
		[Header("Basic Info")]
		public string ActionName = "Tackle";
		public ActionType ActionType = ActionType.Damage;
		public ActionTargetType TargetType = ActionTargetType.SingleEnemy;
		[Range(0f, 1f)] public float Accuracy = 1f;
		public float CritChance = 0f;

		[Header("Damage / Heal")]
		public int Power = 10;
		public float StatMultiplier = 1f;

		[Header("Buff / Debuff (optional)")]
		public float Duration = 0f;
		public float AttackModifier = 0f;
		public float DefenseModifier = 0f;
		public float SpeedModifier = 0f;

		[Header("Defend Action")]
		[Tooltip("Damage reduction multiplier when defending (0.5 = 50% damage taken)")]
		public float DamageReduction = 0.5f;
		[Tooltip("Turns the defend state lasts")]
		public int DefendDuration = 1;
		[Tooltip("Counter attack chance when defending")]
		public float CounterChance = 0f;
		[Tooltip("Counter attack power multiplier")]
		public float CounterMultiplier = 1f;
		[Tooltip("Attack buff multiplier applied after defending (1.2 = +20% attack)")]
		public float DefendAttackBuff = 1f;
		[Tooltip("Turns the attack buff lasts")]
		public int AttackBuffDuration = 1;

		[Header("Multi-Turn Actions")]
		public bool IsMultiTurn = false;
		[Tooltip("How many turns this action takes to complete")]
		public int TurnCost = 1;
		[Tooltip("What happens during charge turns")]
		public ChargeTurnBehavior ChargeTurnBehavior = ChargeTurnBehavior.Nothing;
		[Tooltip("Message displayed during charge turns")]
		public string ChargeMessage = " is charging...";
		[Tooltip("Bonus multiplier when fully charged (applies to final damage/healing)")]
		public float ChargeMultiplier = 1.5f;

		[Header("Other")]
		public bool TargetSelf = false;
	}

	public enum ActionTargetType {
		Self,
		SingleEnemy,
		AllEnemies,
		SingleAlly,
		AllAllies
	}

	public enum ActionType {
		Damage,
		Heal,
		Buff,
		Debuff,
		Item,
		Defend
	}

	public enum ChargeTurnBehavior {
		Nothing,
		ApplyDefenseBuff,
		ApplySpeedDebuff,
		TakeDamage,
		CustomEffect
	}
}
