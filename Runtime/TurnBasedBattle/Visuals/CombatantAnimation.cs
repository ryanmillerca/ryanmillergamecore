using UnityEngine;
using System.Collections.Generic;

namespace RyanMillerGameCore.TurnBasedCombat {

    public class CombatantAnimation : MonoBehaviour {
        [Tooltip("You don't have to set this; we'll do GetComponentInChildren on Awake")]
        [SerializeField] private Animator animator;
        [Tooltip("You don't have to set this; we'll do GetComponent on Awake")]
        [SerializeField] private Combatant combatant;

        // Optional: You can make this a serialized dictionary or use a switch
        private Dictionary<CombatantEventType, string> animationMap = new Dictionary<CombatantEventType, string>() {
            { CombatantEventType.AttackStarted, "Attack1" },
            { CombatantEventType.AttackHit, "" },
            { CombatantEventType.AttackMissed, "" },
            { CombatantEventType.DamageTaken, "Hurt" },
            { CombatantEventType.CriticalDamageTaken, "Hurt" },
            { CombatantEventType.HealingReceived, "" },
            { CombatantEventType.Died, "Death" },
            { CombatantEventType.DefendStarted, "Block" },
            { CombatantEventType.DefendEnded, "" },
            { CombatantEventType.SkillUsed, "" },
            { CombatantEventType.CounterAttack, "" },
            { CombatantEventType.TurnStarted, "" },
            { CombatantEventType.TurnEnded, "" }
        };

        private void Awake() {
            if (combatant == null) {
                combatant = GetComponent<Combatant>();
            }
            if (animator == null) {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void OnEnable() {
            if (combatant != null) {
                combatant.CombatantEvent += OnCombatantEvent;
            }
        }

        private void OnDisable() {
            if (combatant != null) {
                combatant.CombatantEvent -= OnCombatantEvent;
            }
        }

        private void OnCombatantEvent(CombatantEventData eventData) {
            if (animationMap.TryGetValue(eventData.EventType, out string animationName)) {
                if (!string.IsNullOrEmpty(animationMap[eventData.EventType])) {
                    animator.Play(animationName);
                }
            }
        }
    }
}
