using RyanMillerGameCore.TurnBasedCombat;
using UnityEngine;

/// <summary>
/// Result from a minigame execution, used to modify BattleAction effects.
/// </summary>
[System.Serializable]
public struct MinigameResult {
	
	/// <summary>
	/// Whether the minigame was completed successfully (false = action fails)
	/// </summary>
	public bool success;
	
	/// <summary>
	/// Multiplier applied to action effects (damage, healing, etc.)
	/// Range: 0.0 (complete failure) to 2.0+ (exceptional performance)
	/// Default: 1.0 (normal execution)
	/// </summary>
	public float performanceMultiplier;
	
	/// <summary>
	/// Whether the player achieved perfect execution (for bonus effects)
	/// </summary>
	public bool perfectExecution;
}

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

	public MinigameResult Result {
		get { return m_result; }
		protected set { m_result = value; }
	}

	[SerializeField] private bool m_GameIsRunning = false;

	protected BattleManager m_battleManager;
	protected BattleCommand m_battleCommand;
	protected MinigameResult m_result;

	public virtual void StartGame() {
		m_GameIsRunning = true;
		// Initialize result with default values
		m_result = new MinigameResult {
			success = true,
			performanceMultiplier = 1.0f,
			perfectExecution = false
		};
	}

	/// <summary>
	/// Call this from your minigame when it's complete, passing the result.
	/// </summary>
	protected virtual void FinishGame(MinigameResult result) {
		m_result = result;
		m_GameIsRunning = false;
	}

	/// <summary>
	/// Backward compatibility - finish with default result.
	/// </summary>
	public virtual void FinishGame() {
		FinishGame(m_result);
	}
}