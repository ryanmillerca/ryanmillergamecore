namespace RyanMillerGameCore.Character {

    using UnityEngine;

    [CreateAssetMenu(fileName = "CharacterStats", menuName = "RyanMillerGameCore/Character/CharacterStats")]
    public class CharacterStats : ScriptableObject {
        public CharacterID m_Identifier;
        public float m_MaxHealth = 10;
        public float m_CurrentHealth = 10;
        public float m_DamageCooldown = 1f;
    }
}