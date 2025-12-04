namespace RyanMillerGameCore.TurnBasedCombat {
	using UnityEngine;
	using System;
	using System.Collections.Generic;
	using System.Collections;

	public class MinigameResolver : MonoBehaviour {

		[SerializeField] private MinigamePair[] m_Minigames;

		public AbstractMinigame GetMinigameByType(MinigameType minigameType) {
			if (minigameType == MinigameType.None) {
				return null;
			}
			
			foreach (var mini in m_Minigames) {
				if (mini.minigameType == minigameType) {
					if (mini.miniGame) {
						return mini.miniGame;
					}
				}
			}
			Debug.LogWarning($"No minigame found with type {minigameType}");
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
		public MinigameType minigameType;
		public AbstractMinigame miniGame;
	}
}
