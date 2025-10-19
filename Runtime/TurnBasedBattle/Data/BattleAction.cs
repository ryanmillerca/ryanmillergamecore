using UnityEngine;

namespace RyanMillerGameCore.TurnBasedCombat {
	[CreateAssetMenu(menuName = "Battle/Action")]
	public class BattleAction : ScriptableObject {
		[Header("Basic Info")]
		public string m_ActionName = "Tackle";
		public ActionType m_ActionType = ActionType.Damage;
		public ActionTargetType m_TargetType = ActionTargetType.SingleEnemy;
		[Range(0f, 1f)] public float m_Accuracy = 1f;
		public float m_CritChance = 0f;

		[Header("Damage / Heal")]
		public int m_Power = 10;
		public float m_StatMultiplier = 1f;

		[Header("Buff / Debuff (optional)")]
		public float m_Duration = 0f;
		public float m_AttackModifier = 0f;
		public float m_DefenseModifier = 0f;
		public float m_SpeedModifier = 0f;

		[Header("Defend Action")]
		[Tooltip("Damage reduction multiplier when defending (0.5 = 50% damage taken)")]
		public float m_DamageReduction = 0.5f;
		[Tooltip("Turns the defend state lasts")]
		public int m_DefendDuration = 1;
		[Tooltip("Counter attack chance when defending")]
		public float m_CounterChance = 0f;
		[Tooltip("Counter attack power multiplier")]
		public float m_CounterMultiplier = 1f;

		[Header("Multi-Turn Actions")]
		public bool m_IsMultiTurn = false;
		[Tooltip("How many turns this action takes to complete")]
		public int m_TurnCost = 1;
		[Tooltip("What happens during charge turns")]
		public ChargeTurnBehavior m_ChargeTurnBehavior = ChargeTurnBehavior.Nothing;
		[Tooltip("Message displayed during charge turns")]
		public string m_ChargeMessage = " is charging...";
		[Tooltip("Bonus multiplier when fully charged (applies to final damage/healing)")]
		public float m_ChargeMultiplier = 1.5f;

		[Header("Other")]
		public bool m_TargetSelf = false;
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
