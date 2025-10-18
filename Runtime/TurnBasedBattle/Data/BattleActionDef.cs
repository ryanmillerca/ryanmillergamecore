
namespace RyanMillerGameCore.TurnBasedBattle {
	using UnityEngine;

	[CreateAssetMenu(menuName = "Battle/Action")]
	public class BattleActionDef : ScriptableObject {
		[Header("Basic Info")]
		public string m_ActionName = "Tackle";
		public ActionType m_ActionType = ActionType.Damage;
		public ActionTargetType m_TargetType = ActionTargetType.SingleEnemy;
		[Range(0f, 1f)] public float m_Accuracy = 1f;
		public float m_CritChance = 0f;

		[Header("Damage / Heal")]
		public int m_Power = 10; // Used for damage or heal
		public float m_StatMultiplier = 1f; // Multiplier for attack stat

		[Header("Buff / Debuff (optional)")]
		public float m_Duration = 0f; // in turns
		public float m_AttackModifier = 0f; // e.g. +0.2 = +20% attack
		public float m_DefenseModifier = 0f;
		public float m_SpeedModifier = 0f;

		[Header("Other")]
		public bool m_TargetSelf = false; // override TargetType if true
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
		Item
	}

}
