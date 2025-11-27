namespace RyanMillerGameCore.TurnBasedCombat {
	using UnityEngine;

	/// <summary>
	/// Connects BattleManager to Input UI for Player
	/// </summary>
	public class StartCombat : MonoBehaviour {

		[SerializeField] private Combatant[] m_Enemies;
		private GameStateSwitcher m_gameStateSwitcher;
		private GameStateSwitcher gameStateSwitcher {
			get {
				if (m_gameStateSwitcher == null) {
					m_gameStateSwitcher = FindFirstObjectByType<GameStateSwitcher>();
				}
				return m_gameStateSwitcher;
			}
		}

		public void TriggerCombat() {
			gameStateSwitcher.SwitchToCombat(m_Enemies);
		}
	}
}
