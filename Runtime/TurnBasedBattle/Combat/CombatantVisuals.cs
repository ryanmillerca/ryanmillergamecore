using System;
using System.Collections.Generic;
using UnityEngine;

namespace RyanMillerGameCore.TurnBasedCombat {

    public class CombatantVisuals : MonoBehaviour {

        [SerializeField] private Combatant combatant;
        [SerializeField] private CombatantEventAnimPair[] combatantEventAnimPairs;
        [SerializeField] private Animator animator;
        
        private void Start() {
            if (combatant) {
                combatant.CombatantEvent += CombatantOnCombatantEvent;
            }
        }

        private void Destroy() {
            if (combatant) {
                combatant.CombatantEvent -= CombatantOnCombatantEvent;
            }
        }
        private void CombatantOnCombatantEvent(CombatantEventData eventData) {
            foreach (var pair in combatantEventAnimPairs) {
                if (pair.eventType == eventData.EventType) {
                    PlayAnimation(pair.clip);
                    break;
                }
            }
        }

        private void PlayAnimation(AnimationClip clip) {
            animator.Play(clip.name);
        }
    }

    [Serializable]
    public class CombatantEventAnimPair {
        public CombatantEventType eventType;
        public AnimationClip clip;
    }
}