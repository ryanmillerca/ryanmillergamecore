namespace RyanMillerGameCore.TurnBasedBattle {
    using UnityEngine;

    [CreateAssetMenu(menuName = "Battle/Move")]
    public class MoveDefSo : ScriptableObject {
        public string m_MoveName = "Tackle";
        public int m_Power = 10;
        [Range(0f, 1f)] public float m_Accuracy = 1f;
        public bool m_IsHeal = false;
        public bool m_TargetSelf = false;
        public float m_StatMultiplier = 1f;
    }
}