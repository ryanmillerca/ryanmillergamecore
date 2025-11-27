namespace RyanMillerGameCore.TurnBasedCombat {
	using UnityEngine;
	using UnityEngine.Events;
	using System.Collections;

	public class GameStateSwitcher : MonoBehaviour {

		[SerializeField] private Combatant[] m_Players;
		[SerializeField] private Combatant[] m_Enemies;
		[SerializeField] private Transform[] m_PlayerSlots;
		[SerializeField] private Transform[] m_EnemySlots;

		[SerializeField] private GameObject m_Traversal;
		[SerializeField] private GameObject m_BattleScene;

		[SerializeField] private UnityEvent m_SceneTransitionToCombatStart;
		[SerializeField] private float m_SceneTransitionToCombatDuration = 0.5f;
		[SerializeField] private UnityEvent m_SceneTransitionToCombatEnd;
		[SerializeField] private UnityEvent m_SceneTransitionToTraversalStart;
		[SerializeField] private float m_SceneTransitionToTraversalDuration = 0.5f;
		[SerializeField] private UnityEvent m_SceneTransitionToTraversalEnd;

		public void SwitchToCombat(Combatant[] combatants) {
			m_Enemies = combatants;

			foreach (var combatant in m_Enemies) { }
			StartCoroutine(SwitchToCombatCoroutine());
		}

		IEnumerator SwitchToCombatCoroutine() {
			m_SceneTransitionToCombatStart.Invoke();
			yield return new WaitForSecondsRealtime(m_SceneTransitionToCombatDuration);
			m_Traversal.SetActive(false);

			m_BattleScene.SetActive(true);
			m_SceneTransitionToCombatEnd.Invoke();
		}

		IEnumerator SwitchToTraversalCoroutine() {
			m_SceneTransitionToTraversalStart.Invoke();
			yield return new WaitForSecondsRealtime(m_SceneTransitionToTraversalDuration);
			m_BattleScene.SetActive(false);

			m_Traversal.SetActive(true);
			m_SceneTransitionToTraversalEnd.Invoke();
		}

		private void PlaceCombatant(Combatant combatant) { }

		public void SwitchToTraversal() { }

		void Start() {
			if (transform.parent == null) {
				DontDestroyOnLoad(gameObject);
			}
		}
	}
}
