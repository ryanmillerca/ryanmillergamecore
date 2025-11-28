using RyanMillerGameCore.TurnBasedCombat;
using UnityEngine;

abstract public class AbstractMinigame : MonoBehaviour {
	public bool GameIsRunning {
		get { return m_GameIsRunning; }
	}

	public BattleCommand BattleCommand {
		get {
			return m_battleCommand;
		}
		set { m_battleCommand = value; }
	}

	public BattleManager BattleManager {
		get {
			return m_battleManager;
		}
		set {
			m_battleManager = value;
		}
	}

	[SerializeField] private bool m_GameIsRunning = false;

	protected BattleManager m_battleManager;
	protected BattleCommand m_battleCommand;

	public virtual void StartGame() {
		m_GameIsRunning = true;
	}

	public virtual void FinishGame() {
		m_GameIsRunning = false;
	}
}
