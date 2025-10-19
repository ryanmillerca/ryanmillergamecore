namespace RyanMillerGameCore.TurnBasedCombat {
	using System.Collections.Generic;
	using UnityEngine;

	// Team enum to define affiliations
	public enum Team {
		Player,
		Enemy,
		Neutral // Optional: for future expansion
	}

	public class Combatant : MonoBehaviour {
		public string m_CombatantName;
		public int m_MaxHp = 100;
		public int m_Attack = 20;
		public int m_Defense = 10;
		public int m_Speed = 10;
		public int m_CurrentHp = 100;
		public List<BattleAction> m_Moves;
		public Team m_Team = Team.Enemy; // Replace m_IsPlayer with Team
		[HideInInspector] public float m_TurnGauge = 0f; // 0-100 gauge
		public Color m_Color;
		
		// Backwards compatibility property
		public bool m_IsPlayer {
			get { return m_Team == Team.Player; }
			set { m_Team = value ? Team.Player : Team.Enemy; }
		}
		
		private void Awake() {
			m_CurrentHp = Mathf.Clamp(m_CurrentHp, 0, m_MaxHp);
		}

		public string ColorAsHex {
			get {
				return "#" + ColorUtility.ToHtmlStringRGB(m_Color);
			}
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
				Die();
			}
			Debug.Log($"{m_CombatantName} takes {dmg} damage. (HP: {m_CurrentHp}/{m_MaxHp})");
			if (isAlive == false) {
				Debug.Log($"{m_CombatantName} died!");
			}
		}

		public void Die() {
			
		}

		public void Heal(int amount) {
			m_CurrentHp += amount;
			if (m_CurrentHp > m_MaxHp) {
				Debug.Log($"<color={ColorAsHex}>{m_CombatantName} heals {amount}. (HP: {m_CurrentHp}/{m_MaxHp})</color>");
				m_CurrentHp = m_MaxHp;
			}
			Debug.Log($"<color={ColorAsHex}>{m_CombatantName} heals {amount}. (HP: {m_CurrentHp}/{m_MaxHp})<color>");
			if (m_CurrentHp == m_MaxHp) {
				Debug.Log($"<color={ColorAsHex}>{m_CombatantName} is at full health!<color>");
			}
		}
	}
}
