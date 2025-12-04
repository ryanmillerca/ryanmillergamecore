namespace RyanMillerGameCore.TurnBasedCombat {
    using System;
    using RyanMillerGameCore;
    using System.Collections;
    using UnityEngine;

    public class CombatantTransformAnimation : MonoBehaviour {
        [Tooltip("You don't have to set this; we'll do GetComponent on Awake")]
        [SerializeField] private Combatant combatant;
        [SerializeField] private EventMotionPair[] motionPairs;

        private void Awake() {
            if (combatant == null) {
                combatant = GetComponent<Combatant>();
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
            foreach (EventMotionPair pair in motionPairs) {
                if (pair.combatantEvent == eventData.EventType) {
                    StartCoroutine(PlayTransformAnimation(pair.motion));
                }
            }
        }

        IEnumerator PlayTransformAnimation(TransformMotion motion) {
            if (motion.initialDelay > 0) {
                yield return new WaitForSeconds(motion.initialDelay);
            }
            Transform combatantTransform = combatant.transform;
            Vector3 combatantPosition = combatantTransform.localPosition;
            for (float i = 0; i <= motion.duration; i += Time.deltaTime) {
                float t = i / motion.duration;
                Vector3 curveValue = new Vector3(
                    motion.animOnX.Evaluate(t),
                    motion.animOnY.Evaluate(t),
                    motion.animOnZ.Evaluate(t)
                ) * motion.multiplier;
                Vector3 offsetPos = Vector3.Lerp(motion.startOffset * motion.multiplier, motion.endOffset * motion.multiplier, t);
                combatantTransform.localPosition = combatantPosition + offsetPos + curveValue;
                yield return new WaitForEndOfFrame();
            }
            if (motion.resetAtEnd) {
                combatantTransform.localPosition = combatantPosition;
            }
            if (motion.disableAtEnd) {
                combatantTransform.gameObject.SetActive(false);
            }
        }
    }

    [Serializable]
    public class EventMotionPair {
        public CombatantEventType combatantEvent;
        public TransformMotion motion;
    }
}
