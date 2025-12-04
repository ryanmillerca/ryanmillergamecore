namespace RyanMillerGameCore.TurnBasedCombat {
	using UnityEngine;
	using UnityEngine.Events;
	using System.Collections;

	public class GameStateSwitcher : MonoBehaviour {

		[SerializeField] private GameObject m_DefaultState;
		[SerializeField] private BattleManager m_BattleManager;
		[SerializeField] private GameObject m_Traversal;
		[SerializeField] private GameObject m_BattleScene;
		[SerializeField] private UnityEvent m_SceneTransitionToCombatStart;
		[SerializeField] private float m_SceneTransitionToCombatDuration = 0.5f;
		[SerializeField] private UnityEvent m_SceneTransitionToCombatEnd;
		[SerializeField] private UnityEvent m_SceneTransitionToTraversalStart;
		[SerializeField] private float m_SceneTransitionToTraversalDuration = 0.5f;
		[SerializeField] private UnityEvent m_SceneTransitionToTraversalEnd;
		[SerializeField] private bool m_FreezeTimeWhenSwitching = true;

		#region Events

		public delegate void OnSwitchedToCombat();
		public event OnSwitchedToCombat SwitchedToCombat;

		public delegate void OnSwitchedToTraversal();
		public event OnSwitchedToTraversal SwitchedToTraversal;

		#endregion

		public void SwitchToCombat(Combatant[] combatants) {
			StartCoroutine(SwitchToCombatCoroutine(combatants));
		}

		public void SwitchToTraversal() {
			StartCoroutine(SwitchToTraversalCoroutine());
		}

		IEnumerator SwitchToCombatCoroutine(Combatant[] combatants) {
			if (m_FreezeTimeWhenSwitching) {
				Time.timeScale = 0;
			}
			m_SceneTransitionToCombatStart.Invoke();
			yield return new WaitForSecondsRealtime(m_SceneTransitionToCombatDuration);
			m_Traversal.SetActive(false);
			m_BattleScene.SetActive(true);
			SwitchedToCombat?.Invoke();
			m_BattleManager.SetupNewBattle(combatants);
			m_SceneTransitionToCombatEnd.Invoke();
			if (m_FreezeTimeWhenSwitching) {
				Time.timeScale = 1;
			}
		}

		IEnumerator SwitchToTraversalCoroutine() {
			if (m_FreezeTimeWhenSwitching) {
				Time.timeScale = 0;
			}
			m_SceneTransitionToTraversalStart.Invoke();
			yield return new WaitForSecondsRealtime(m_SceneTransitionToTraversalDuration);
			m_BattleScene.SetActive(false);
			m_Traversal.SetActive(true);
			SwitchedToTraversal?.Invoke();
			m_SceneTransitionToTraversalEnd.Invoke();
			if (m_FreezeTimeWhenSwitching) {
				Time.timeScale = 1;
			}
		}

		private void PlaceCombatant(Combatant combatant) { }

		void Start() {
			if (transform.parent == null) {
				DontDestroyOnLoad(gameObject);
			}
			if (m_BattleScene) {
				m_BattleScene.SetActive(false);
			}
			if (m_Traversal) {
				m_Traversal.SetActive(false);
			}
			if (m_DefaultState) {
				m_DefaultState.SetActive(true);
			}
		}
	}
}
