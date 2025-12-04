namespace RyanMillerGameCore.TurnBasedCombat {

	using UnityEngine;
	using System.Collections.Generic;

	public class CombatantAudio : MonoBehaviour {
		[Tooltip("You don't have to set this; we'll do GetComponent on Awake")]
		[SerializeField] private Combatant combatant;

		[Header("Audio Clips")]
		[SerializeField] private AudioClip attackClip;
		[SerializeField] private AudioClip hitClip;
		[SerializeField] private AudioClip missClip;
		[SerializeField] private AudioClip hurtClip;
		[SerializeField] private AudioClip deathClip;
		[SerializeField] private AudioClip blockClip;
		[SerializeField] private AudioClip healClip;
		[SerializeField] private AudioClip skillClip;
		[SerializeField] private AudioClip counterClip;

		// Optional: You can make this a serialized dictionary or use a switch
		private Dictionary<CombatantEventType, AudioClip> audioMap = new Dictionary<CombatantEventType, AudioClip>() {
			{ CombatantEventType.AttackStarted, null },
			{ CombatantEventType.AttackHit, null },
			{ CombatantEventType.AttackMissed, null },
			{ CombatantEventType.DamageTaken, null },
			{ CombatantEventType.CriticalDamageTaken, null },
			{ CombatantEventType.HealingReceived, null },
			{ CombatantEventType.Died, null },
			{ CombatantEventType.DefendStarted, null },
			{ CombatantEventType.DefendEnded, null },
			{ CombatantEventType.SkillUsed, null },
			{ CombatantEventType.CounterAttack, null },
			{ CombatantEventType.TurnStarted, null },
			{ CombatantEventType.TurnEnded, null }
		};

		private void Awake() {
			if (combatant == null) {
				combatant = GetComponent<Combatant>();
			}

			// Set up audio map with actual clips
			audioMap[CombatantEventType.AttackStarted] = attackClip;
			audioMap[CombatantEventType.AttackHit] = hitClip;
			audioMap[CombatantEventType.AttackMissed] = missClip;
			audioMap[CombatantEventType.DamageTaken] = hurtClip;
			audioMap[CombatantEventType.CriticalDamageTaken] = hurtClip;
			audioMap[CombatantEventType.HealingReceived] = healClip;
			audioMap[CombatantEventType.Died] = deathClip;
			audioMap[CombatantEventType.DefendStarted] = blockClip;
			audioMap[CombatantEventType.SkillUsed] = skillClip;
			audioMap[CombatantEventType.CounterAttack] = counterClip;
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
			if (audioMap.TryGetValue(eventData.EventType, out AudioClip clip)) {
				if (clip != null) {
					// Play audio clip using AudioSource
					AudioSource audioSource = GetComponent<AudioSource>();
					if (audioSource == null) {
						audioSource = gameObject.AddComponent<AudioSource>();
					}
					audioSource.PlayOneShot(clip);
				}
			}
		}
	}
}
