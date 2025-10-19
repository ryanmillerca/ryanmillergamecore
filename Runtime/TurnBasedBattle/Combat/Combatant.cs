namespace RyanMillerGameCore.TurnBasedCombat {
	using System.Collections.Generic;
	using UnityEngine;

	public class Combatant : MonoBehaviour {
		public string m_CombatantName;
		public int m_MaxHp = 100;
		public int m_Attack = 20;
		public int m_Defense = 10;
		public int m_Speed = 10;
		public int m_CurrentHp = 100;
		public List<BattleAction> m_Moves;
		public bool m_IsPlayer = false;
		[HideInInspector] public float m_TurnGauge = 0f; // 0-100 gauge

		private void Awake() {
			m_CurrentHp = Mathf.Clamp(m_CurrentHp, 0, m_MaxHp);
		}

		public bool isAlive {
			get {
				return m_CurrentHp > 0;
			}
		}

		public void TakeDamage(int dmg) {
			m_CurrentHp -= dmg;
			if (m_CurrentHp < 0) {
				m_CurrentHp = 0;
			}
			Debug.Log($"{m_CombatantName} takes {dmg} damage. (HP: {m_CurrentHp}/{m_MaxHp})");
			if (isAlive == false) {
				Debug.Log($"{m_CombatantName} died!");
			}
		}

		public void Heal(int amount) {
			m_CurrentHp += amount;
			if (m_CurrentHp > m_MaxHp) {
				Debug.Log($"{m_CombatantName} heals {amount}. (HP: {m_CurrentHp}/{m_MaxHp})");
				m_CurrentHp = m_MaxHp;
			}
			Debug.Log($"{m_CombatantName} heals {amount}. (HP: {m_CurrentHp}/{m_MaxHp})");
			if (m_CurrentHp == m_MaxHp) {
				Debug.Log($"{m_CombatantName} is at full health!");
			}
		}
	}
}
