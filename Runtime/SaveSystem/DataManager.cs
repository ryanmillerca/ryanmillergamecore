using RyanMillerGameCore.SceneControl;
namespace RyanMillerGameCore.SaveSystem {
	using UnityEngine.SceneManagement;
	using UnityEngine;
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using System.Collections;
	using UnityEngine.Events;
	using Character;
	using Utilities;

	public class DataManager : Singleton<DataManager> {
		[SerializeField] private SaveStrategy saveStrategy = SaveStrategy.AfterEveryChange;
		[SerializeField] private float autoSaveInterval = 20f;
		[SerializeField] private string saveFileName = "saveData.json";
		[SerializeField] private SaveData saveData;
		[SerializeField] private bool autoLoad = true;
		[Foldout("Unity Events"), SerializeField]
		private UnityEvent GameLoaded;
		[Foldout("Unity Events"), SerializeField]
		private UnityEvent GameSaved;

		private float lastCheckpointTime = 0;
		private Coroutine postLoadGame;

		public event Action SaveDataChanged;
		public event Action SaveComplete;
		public event Action LoadComplete;

		private float lastSaveTime = 0;
		private bool needsToSave = false;
		private bool isSaving = false;

		protected override void Awake() {
			base.Awake();
			if (autoLoad) {
				LoadGame();
			}
		}


		#region Read Data

		private void LoadGame() {
			string path = Path.Combine(Application.persistentDataPath, saveFileName);
			if (File.Exists(path)) {
				try {
					string json = File.ReadAllText(path);
					saveData = JsonUtility.FromJson<SaveData>(json);
//                    Debug.Log("Game loaded from " + path);
					if (postLoadGame != null) {
						StopCoroutine(postLoadGame);
					}
					postLoadGame = StartCoroutine(PostLoadGame());
				}
				catch (Exception e) {
					Debug.LogError("Failed to load game: " + e.Message);
					saveData = new SaveData();
				}
			}
			else {
				Debug.Log("No save file found at: " + path + ". Creating new save data.");
				saveData = new SaveData();
			}
		}

		private IEnumerator PostLoadGame() {
			yield return new WaitForEndOfFrame();
			if (CharacterManager.Instance) {
//                Debug.Log("Post Load Game Complete");
				CharacterManager.Instance.Player.GoToCheckpoint();
			}
			LoadComplete?.Invoke();
			GameLoaded?.Invoke();
		}

		public bool HasKey(string key) {
			return saveData.HasKey(key);
		}

		public List<string> ReadAllKeys() {
			return new List<string>(saveData.keys);
		}

		#endregion


		#region Change Data

		public void AddKey(string key, bool allowDuplicateEntries = false) {
			bool addedKey = saveData.AddKey(key, allowDuplicateEntries);
			if (addedKey) {
				StartCoroutine(DataWasChanged());
			}
		}

		public void RemoveKey(string key) {
			bool removedKey = saveData.RemoveKey(key);
			if (removedKey) {
				StartCoroutine(DataWasChanged());
			}
		}

		private IEnumerator DataWasChanged() {
			yield return new WaitForEndOfFrame();
			DataChanged();
		}

		private void DataChanged() {
			needsToSave = true;
			SaveDataChanged?.Invoke();
			if (saveStrategy == SaveStrategy.AfterEveryChange) {
				SaveGame();
			}
		}

		public void EraseGame() {
			string path = Path.Combine(Application.persistentDataPath, saveFileName);
			if (File.Exists(path)) {
				File.Delete(path);
				Debug.Log("Save file erased: " + path);
			}
			else {
				Debug.LogWarning("No save file found to erase at: " + path);
			}
		}

		#endregion


		#region Save On Interval

		private void Update() {
			if (Time.unscaledTime > lastSaveTime + autoSaveInterval) {
				if (needsToSave) {
					SaveGame();
				}

				lastSaveTime = Time.unscaledTime;
			}
		}

		#endregion


		private async void SaveGame() {
			await Task.Delay(100); // Prevent race conditions
			await BeginSavingGame();
		}

		private async Task BeginSavingGame() {
			if (isSaving) return;
			isSaving = true;

			string json = JsonUtility.ToJson(saveData, true);
			string path = Path.Combine(Application.persistentDataPath, saveFileName);
			try {
				using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
				using (StreamWriter writer = new StreamWriter(stream)) {
					await writer.WriteAsync(json);
				}

//                Debug.Log("Game saved to " + path);
				needsToSave = false;
				SaveComplete?.Invoke();
				GameSaved?.Invoke();
			}
			catch (Exception e) {
				Debug.LogError("Failed to save game: " + e.Message);
			} finally {
				isSaving = false;
			}
		}

		private enum SaveStrategy {
			DontSave,
			AfterEveryChange,
			OnInterval
		}


		#region Checkpoint

		public void SetCheckpoint(Vector3 position, float facing, bool saveImmediately = false) {
			string currentScene = SceneManager.GetActiveScene().name;
			saveData.sceneCheckpoints.SetCheckpoint(currentScene, position, facing);
			lastCheckpointTime = Time.time;
			needsToSave = true;
			if (saveImmediately) {
				SaveGame();
			}
		}

		public CheckpointData TryGetCheckpoint() {
			string currentScene = SceneManager.GetActiveScene().name;
			CheckpointData checkPointData = saveData.sceneCheckpoints.TryGetCheckpoint(currentScene);
			return checkPointData;
		}

		/// <summary>
		/// Remove all saved checkpoints for the given scene.
		/// Returns true if any entries were removed.
		/// </summary>
		public bool ClearCheckpointsInScene(string sceneName) {
			if (string.IsNullOrEmpty(sceneName)) {
				return false;
			}
			bool removed = saveData.sceneCheckpoints.ClearScene(sceneName);
			return removed;
		}

		#endregion


	}
}
