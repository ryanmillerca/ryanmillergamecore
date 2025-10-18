namespace RyanMillerGameCore.TurnBasedBattle {
    using System.Collections.Generic;
    using UnityEngine;

    public class Combatant : MonoBehaviour {
        public string m_CombatantName = "Fighter";
        public int m_MaxHp = 100;
        public int m_Attack = 20;
        public int m_Defense = 10;
        public int m_Speed = 10;
        public int m_CurrentHp = 100;
        public List<MoveDefSo> m_Moves;

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
            if (m_CurrentHp < 0) m_CurrentHp = 0;
            Debug.Log($"{m_CombatantName} takes {dmg} damage. (HP: {m_CurrentHp}/{m_MaxHp})");
        }

        public void Heal(int amount) {
            m_CurrentHp += amount;
            if (m_CurrentHp > m_MaxHp) m_CurrentHp = m_MaxHp;
            Debug.Log($"{m_CombatantName} heals {amount}. (HP: {m_CurrentHp}/{m_MaxHp})");
        }
    }
}