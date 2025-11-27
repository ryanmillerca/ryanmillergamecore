namespace RyanMillerGameCore.SceneControl {
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using System.Collections;
	using Character;
	using SaveSystem;

	public class SceneTransitioner : Singleton<SceneTransitioner> {
		[SerializeField] [Range(0, 5)] private float fadeInTime = 1f;
		[SerializeField] private float loadDelay = 0.5f;
		[SerializeField] [Range(0, 5)] private float fadeOutTime = 1f;
		[SerializeField] private bool freezePlayerOnTransition = true;
		[SerializeField] private bool freezeTimeOnTransition = false;
		[SerializeField] private CanvasGroup sceneLoadCanvasGroup;


		#region Static Methods

		public void FadeToScene(string sceneName, CheckpointData checkpointData = null) {
			StartCoroutine(FadeAndLoadScene(sceneName, checkpointData));
		}

		public void GoToSceneLocation(LocationID sceneLocation) {
			if (sceneLocation == null) {
				Debug.LogWarning("SceneLocation is null");
				return;
			}
			FadeToScene(sceneLocation.SceneName, sceneLocation.CheckpointData);
		}

		public void FadeIn() {
			DoFade(0, 1);
		}

		public void FadeOut() {
			DoFade(1, 0);
		}

		public void DoFade(float from, float to) {
			StartCoroutine(DoFadeCoroutine(from, to));
		}

		#endregion


		#region Private Methods

		private IEnumerator DoFadeCoroutine(float from, float to) {
			for (float i = 0; i <= fadeInTime; i += Time.unscaledDeltaTime) {
				float t = i / fadeInTime;
				sceneLoadCanvasGroup.alpha = Mathf.Lerp(from, to, t);
				yield return new WaitForEndOfFrame();
			}
			sceneLoadCanvasGroup.alpha = to;
		}

		private IEnumerator FadeAndLoadScene(string sceneName, CheckpointData checkpointData = null) {
			if (freezeTimeOnTransition) {
				Time.timeScale = 0;
			}

			SetPlayerInputEnabled(false);

			// turn on load screen (fade in)
			for (float i = 0; i <= fadeInTime; i += Time.unscaledDeltaTime) {
				float t = i / fadeInTime;
				sceneLoadCanvasGroup.alpha = t;
				yield return new WaitForEndOfFrame();
			}
			sceneLoadCanvasGroup.alpha = 1f;

			var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
			while (asyncLoad is { isDone: false }) {
				yield return null;
			}

			yield return new WaitForSecondsRealtime(loadDelay);
			if (GameStateManager.Instance?.CurrentGameState == GameState.Gameplay) {
				CharacterManager.Instance?.Player?.GoToCheckpoint(checkpointData);
			}

			// turn off load screen (fade out)
			for (float i = 0; i <= fadeOutTime; i += Time.unscaledDeltaTime) {
				float t = i / fadeOutTime;
				sceneLoadCanvasGroup.alpha = 1 - t;
				yield return new WaitForEndOfFrame();
			}
			sceneLoadCanvasGroup.alpha = 0f;

			if (GameStateManager.Instance?.CurrentGameState == GameState.Gameplay) {
				SetPlayerInputEnabled(true);
			}

			if (freezeTimeOnTransition) {
				Time.timeScale = 1;
			}
		}

		private void SetPlayerInputEnabled(bool inputEnabled) {
			if (freezePlayerOnTransition) {
				CharacterManager.Instance?.Player.Brain.SetInputEnabled(inputEnabled);
			}
		}

		#endregion


	}
}
