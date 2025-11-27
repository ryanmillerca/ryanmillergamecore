namespace RyanMillerGameCore.TurnBasedCombat {
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// Connects BattleManager to Input UI for Player
	/// </summary>
	public class StartCombat : MonoBehaviour {

		[SerializeField] private Combatant[] m_Enemies;
		[SerializeField] private bool m_DisableAfterCombat = true;

		[SerializeField] private UnityEvent OnSwitchedToCombat;
		[SerializeField] private UnityEvent OnSwitchedToTraversal;

		private GameStateSwitcher m_gameStateSwitcher;
		private GameStateSwitcher gameStateSwitcher {
			get {
				if (!m_gameStateSwitcher) {
					m_gameStateSwitcher = FindFirstObjectByType<GameStateSwitcher>();
				}
				return m_gameStateSwitcher;
			}
		}

		public void TriggerCombat() {
			gameStateSwitcher.SwitchToCombat(m_Enemies);
			gameStateSwitcher.SwitchedToTraversal += SwitchedToTraversal;
			gameStateSwitcher.SwitchedToCombat += SwitchedToCombat;
		}

		void SwitchedToTraversal() {
			OnSwitchedToTraversal?.Invoke();
			if (m_DisableAfterCombat) {
				gameObject.SetActive(false);
				gameStateSwitcher.SwitchedToTraversal -= SwitchedToTraversal;
			}
		}

		void SwitchedToCombat() {
			gameStateSwitcher.SwitchedToCombat -= SwitchedToCombat;
			OnSwitchedToCombat?.Invoke();
		}
	}
}
