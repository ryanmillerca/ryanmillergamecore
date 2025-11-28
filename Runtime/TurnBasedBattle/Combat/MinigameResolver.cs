namespace RyanMillerGameCore.TurnBasedCombat {
	using UnityEngine;
	using System;
	using System.Collections.Generic;
	using System.Collections;

	public class MinigameResolver : MonoBehaviour {

		[SerializeField] private MinigamePair[] m_Minigames;

		public AbstractMinigame GetMinigameWithName(string miniGameName) {
			foreach (var mini in m_Minigames) {
				if (mini.name.Equals(miniGameName)) {
					if (mini.miniGame) {
						return mini.miniGame;
					}
				}
			}
			Debug.Log($"No minigame found with name " + miniGameName);
			return null;
		}

		public IEnumerator StartMinigame(AbstractMinigame abstractMinigame) {
			abstractMinigame.gameObject.SetActive(true);
			abstractMinigame.StartGame();
			while (abstractMinigame.GameIsRunning) {
				yield return null;
			}
		}
	}

	[Serializable]
	public class MinigamePair {
		public string name;
		public AbstractMinigame miniGame;
	}
}
