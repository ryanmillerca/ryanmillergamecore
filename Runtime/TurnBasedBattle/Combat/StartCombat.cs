namespace RyanMillerGameCore.TurnBasedCombat {
	using UnityEngine;

	/// <summary>
	/// Connects BattleManager to Input UI for Player
	/// </summary>
	public class StartCombat : MonoBehaviour {

		[SerializeField] private Combatant[] m_Enemies;
		[SerializeField] private bool m_DisableAfterCombat = true;

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
			if (m_DisableAfterCombat) {
				gameStateSwitcher.SwitchedToTraversal += DisableSelf;
			}
		}

		void DisableSelf() {
			gameObject.SetActive(false);
			gameStateSwitcher.SwitchedToTraversal -= DisableSelf;
		}
	}
}
