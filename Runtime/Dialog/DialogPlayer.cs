namespace RyanMillerGameCore.Dialog {
	using System;
	using System.Collections;
	using System.Diagnostics;
	using TMPro;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using UnityEngine.UI;
	using Animation;
	using Camera;
	using Character;
	using Random = UnityEngine.Random;
	using Debug = UnityEngine.Debug;
	using Unity.Profiling;

	/// <summary>
	/// Handles the display and playback of dialog content in the UI.
	/// Supports optional per-line navigation (via DialogNavigation) before text.
	/// Movement-only lines (no text) keep the UI hidden and auto-advance.
	/// </summary>
	public class DialogPlayer : MonoBehaviour {


		#region Public Methods

		public void OnButtonPress() {
			DoDialogAdvance();
		}

		#endregion


		#region Serialized Fields

		[Header("References")] [SerializeField]
		private Image[] dialogFrames;

		[SerializeField] private TextMeshProUGUI textField;
		[SerializeField] private TextMeshProUGUI speakerField;
		[SerializeField] private DialogSkins dialogSkins;
		[SerializeField] private InputActionReference m_InputSubmitAction;

		[Header("Params")] [SerializeField] private float durationShow = 0.5f;
		[SerializeField] private float durationHide = 1f;
		[SerializeField] private bool controlCanvasAlpha = false;

		[Header("Debug")] [Tooltip("When enabled, the player logs key state changes for debugging.")] [SerializeField]
		private bool verboseLogging = false;

		#endregion


		#region Private Fields

		private CanvasGroup _canvasGroup;
		private SpeakerSkin _currentSpeakerSkin;
		private AudioSource _audioSource;
		private bool _playingDialog = false;
		private float _prevTimeScale = 1f;
		private InputActionMap _actionMap;
		private bool _isTextRevealed = false;
		private bool _waitingForAdvance = false;
		private bool _uiVisible = false;

		private DialogState _state = DialogState.Idle;

		#endregion


		#region Profiling

		private static readonly ProfilerMarker MarkerPlayDialog = new ProfilerMarker("DialogPlayer.PlayDialog");
		private static readonly ProfilerMarker MarkerLine = new ProfilerMarker("DialogPlayer.Line");
		private static readonly ProfilerMarker MarkerReveal = new ProfilerMarker("DialogPlayer.Reveal");
		private static readonly ProfilerMarker MarkerNav = new ProfilerMarker("DialogPlayer.Nav");

		#endregion


		#region Unity Events

		private void OnValidate() {
			if (_canvasGroup == null) {
				_canvasGroup = GetComponent<CanvasGroup>();
			}

			if (_audioSource == null) {
				_audioSource = GetComponent<AudioSource>();
			}
		}

		public void OnEnable() {
			_audioSource = GetComponent<AudioSource>();
			_canvasGroup = GetComponent<CanvasGroup>();

			SetCanvasAlpha(0);
			_uiVisible = false;

			m_InputSubmitAction.action.performed += OnDialogButtonPressed;
			m_InputSubmitAction.action.Enable();
		}

		private void OnDisable() {
			m_InputSubmitAction.action.performed -= OnDialogButtonPressed;
			m_InputSubmitAction.action.Disable();
		}

		#endregion


		#region Action Events

		public event Action DialogOpened;
		public event Action DialogClosed;
		public event Action DialogNextLine;
		public event Action DialogTextChanged;
		public event Action<Transform> SpeakerChanged;
		public event Action NavigationStarted;
		public event Action NavigationEnded;

		public delegate void DialogCompleteEvent();

		public event DialogCompleteEvent DialogComplete;

		#endregion


		#region Dialog Playback

		public bool PlayDialog(DialogContent dialogContent) {
			if (!ValidateDialogContent(dialogContent)) {
				return false;
			}

			if (_playingDialog) {
				Log("PlayDialog called while already playing.", true);
				return false;
			}

			StartCoroutine(PlayDialogCoroutine(dialogContent));
			return true;
		}

		private bool ValidateDialogContent(DialogContent dialogContent) {
			if (dialogContent == null) {
				Debug.LogWarning("Dialog failed to play: null DialogContent.", gameObject);
				return false;
			}

			if (dialogContent.Lines == null || dialogContent.Lines.Count == 0) {
				Log("Dialog has no lines.", true);
				return true; // Still allow (will open/close quickly)
			}

			return true;
		}

		private IEnumerator PlayDialogCoroutine(DialogContent dialogContent) {
			using (MarkerPlayDialog.Auto()) {
				_state = DialogState.Starting;
				_playingDialog = true;

				InitializeForDialog();

				ApplyFreezeInput(dialogContent);
				ApplyFreezeTimeStart(dialogContent);

				yield return DelayStartIfNeeded(dialogContent);

				PrepareFirstSpeakerSkin(dialogContent);

				SetCanvasAlpha(0);
				_uiVisible = false;

				DialogOpened?.Invoke();
				Log("DialogOpened");

				_state = DialogState.Running;

				// ==== Indexed loop for better control/logging ====
				for (int li = 0; li < dialogContent.Lines.Count; li++) {
					using (MarkerLine.Auto()) {
						var line = dialogContent.Lines[li];
						LogLineHeader(li, line);

						Character speakingCharacter;
						GameObject lookTarget;
						HandleSpeakerAndLookTargets(line, out speakingCharacter, out lookTarget);

						// NAVIGATION FIRST
						if (line.HasNavigation && speakingCharacter != null) {
							bool restoredFreezeAfter = false;
							if (Mathf.Approximately(Time.timeScale, 0f)) {
								Time.timeScale = 1f;
								restoredFreezeAfter = true;
							}

							// NEW: tell visuals to hide/suppress
							NavigationStarted?.Invoke();

							// Always hide the base UI (no-op if controlCanvasAlpha == false)
							yield return HideUIIfVisible();

							// Do the move
							yield return MoveSpeakerViaPathfindAndWait(speakingCharacter, line.navigation);

							// nav finished — visuals may show again now (when text arrives)
							NavigationEnded?.Invoke();

							if (restoredFreezeAfter) {
								Time.timeScale = 0f;
							}
						}

						// MOVEMENT-ONLY → advance
						if (!line.HasText) {
							DialogNextLine?.Invoke();
							_audioSource.Stop();
							Log("Movement-only line: advanced.");
							continue;
						}

						// SPEAKING
						yield return EnsureUIVisibleThenPrepareText(line, speakingCharacter);

						using (MarkerReveal.Auto()) {
							yield return RevealTextCharacters(line, dialogContent);
						}

						yield return WaitForAdvanceOrAuto(dialogContent, line);

						DialogNextLine?.Invoke();
						_audioSource.Stop();
						Log("Line complete → Next.");
					}
				}

				// End cleanup
				textField.SetText(string.Empty);
				DialogClosed?.Invoke();
				Log("DialogClosed");

				if (dialogContent.freezeTime) {
					Time.timeScale = _prevTimeScale;
					Log($"Time restored to {_prevTimeScale:0.###}");
				}

				if (_uiVisible) {
					yield return HideUIIfVisible();
				}

				CameraController.Instance.RestoreCameraTarget();

				_playingDialog = false;
				_state = DialogState.Idle;
				DialogComplete?.Invoke();
				Log("DialogComplete");

				if (dialogContent.freezeInputs) {
					CharacterManager.Instance.Player.Brain.SetInputEnabled(true);
					Log("Inputs re-enabled.");
				}
			}
		}

		public void StopDialogImmediate() {
			StopAllCoroutines();
			_isTextRevealed = false;
			_waitingForAdvance = false;

			textField.SetText(string.Empty);
			DialogTextChanged?.Invoke();

			Time.timeScale = _prevTimeScale;
			SetCanvasAlpha(0);
			_uiVisible = false;
			_playingDialog = false;
			_state = DialogState.Idle;

			Log("StopDialogImmediate called. State reset.");
		}

		#endregion


		#region Input Handlers

		private void OnDialogButtonPressed(InputAction.CallbackContext ctx) {
			DoDialogAdvance();
		}

		private void DoDialogAdvance() {
			if (_playingDialog == false) {
				return;
			}

			if (_isTextRevealed && _waitingForAdvance) {
				_waitingForAdvance = false;
				Log("Advance pressed: proceeding to next line.");
			}
			else if (_isTextRevealed == false) {
				_isTextRevealed = true;
				Log("Advance pressed: reveal instantly.");
			}
		}

		#endregion


		#region UI Skin Management

		private void SetCanvasAlpha(float newValue) {
			if (controlCanvasAlpha) {
				_canvasGroup.alpha = newValue;
				_canvasGroup.interactable = Mathf.Approximately(_canvasGroup.alpha, 1);
			}
		}

		private void SetSpeakerColor(ID speaker) {
			_currentSpeakerSkin = dialogSkins.DefaultSkin;
			foreach (SpeakerSkin speakerSkin in dialogSkins.SpeakerSkins) {
				if (speakerSkin != null && speakerSkin.speaker) {
					if (speakerSkin.speaker.Equals(speaker)) {
						_currentSpeakerSkin = speakerSkin;
						break;
					}
				}
			}
			SetSkin(_currentSpeakerSkin);
		}

		private void SetSkin(SpeakerSkin skin) {
			foreach (Image dialogFrame in dialogFrames) {
				dialogFrame.color = skin.frameColor;
			}

			textField.color = skin.bodyTextColor;
			if (speakerField != null) {
				speakerField.color = skin.characterTextColor;
			}
		}

		#endregion


		#region Navigation glue (try/finally + stored delegates)

		private IEnumerator MoveSpeakerViaPathfindAndWait(Character speaker, DialogNavigation nav) {
			if (speaker == null || nav == null || nav.enabled == false) {
				yield break;
			}

			if (nav.targetID == null) {
				Debug.LogWarning("Dialog navigation enabled but no targetID provided.", this);
				yield break;
			}

			Transform destTransform = IDService.Instance.GetTransformWithID(nav.targetID);
			if (destTransform == null) {
				Debug.LogWarning(
					$"Dialog navigation targetID '{nav.targetID?.name}' could not be resolved to a Transform.", this);
				yield break;
			}

			// Build a temporary target to encode offset (pathfinder expects a Transform)
			GameObject tempTarget = new GameObject("DialogNavTarget_TEMP");
			tempTarget.transform.position = destTransform.position + nav.offset;
			tempTarget.transform.rotation = destTransform.rotation;

			var pathfind = speaker.GetComponent<CharacterPathfind>();
			if (pathfind == null) {
				Debug.LogWarning("CharacterPathfind missing on speaking character.", speaker);
				Destroy(tempTarget);
				yield break;
			}

			bool done = false;
			bool failed = false;

			// Store delegate instances so we can unsubscribe the exact same ones (critical!)
			Action<float> onComplete = _ => {
				done = true;
				Log("PathCompleted");
			};

			Action<string> onFailed = _ => {
				failed = true;
				Log("PathFailed");
			};

			Action<float, float> onTooLong = (_, __) => { Log("PathTakingTooLong"); };

			try {
				pathfind.PathCompleted += onComplete;
				pathfind.PathFailed += onFailed;
				pathfind.PathTakingTooLong += onTooLong;

				pathfind.StartPath(CharacterPathfind.StartOptions.Forceful(tempTarget.transform));
				Log($"Path started → target '{nav.targetID?.name}', timeout={nav.timeoutSeconds}s.");

				float elapsed = 0f;
				while (!done && !failed) {
					if (nav.timeoutSeconds > 0f) {
						elapsed += Time.deltaTime;
						if (elapsed >= nav.timeoutSeconds) {
							pathfind.Cancel();
							failed = true;
							Log("Path timeout → cancelled.");
							break;
						}
					}

					yield return null;
				}

				// Optional facing using ID
				if (nav.faceID != null) {
					Transform faceTr = IDService.Instance.GetTransformWithID(nav.faceID);
					if (faceTr != null) {
						speaker.CharacterMovement.LookAt(faceTr.position);
						Log($"Faced towards '{nav.faceID?.name}'.");
					}
				}
			} finally {
				pathfind.PathCompleted -= onComplete;
				pathfind.PathFailed -= onFailed;
				pathfind.PathTakingTooLong -= onTooLong;

				Destroy(tempTarget);
			}
		}

		#endregion


		#region UI show/hide helpers

		private IEnumerator ShowUIIfHidden() {
			if (_uiVisible || !controlCanvasAlpha) {
				_uiVisible = true;
				yield break;
			}

			for (float t = 0f; t <= durationShow; t += Time.unscaledDeltaTime) {
				SetCanvasAlpha(Mathf.Lerp(0f, 1f, t / durationShow));
				yield return new WaitForEndOfFrame();
			}

			SetCanvasAlpha(1f);
			_uiVisible = true;
			Log("UI shown.");
		}

		private IEnumerator HideUIIfVisible() {
			if (!_uiVisible || !controlCanvasAlpha) {
				_uiVisible = false;
				yield break;
			}

			for (float t = 0f; t <= durationHide; t += Time.unscaledDeltaTime) {
				SetCanvasAlpha(Mathf.Lerp(1f, 0f, t / durationHide));
				yield return new WaitForEndOfFrame();
			}

			SetCanvasAlpha(0f);
			_uiVisible = false;
			Log("UI hidden.");
		}

		#endregion


		#region Coroutine Helper Methods (extracted)

		private void InitializeForDialog() {
			if (speakerField) {
				speakerField.SetText(string.Empty);
			}

			if (textField) {
				textField.SetText(string.Empty);
			}

			DialogTextChanged?.Invoke();
			Log("InitializeForDialog");
		}

		private void ApplyFreezeInput(DialogContent dialogContent) {
			if (dialogContent.freezeInputs) {
				CharacterManager.Instance.Player.Brain.SetInputEnabled(false);
				Log("Inputs disabled (freezeInputs).");
			}
		}

		private void ApplyFreezeTimeStart(DialogContent dialogContent) {
			_prevTimeScale = Time.timeScale;
			if (dialogContent.freezeTime) {
				Time.timeScale = 0f;
				Log("Time frozen (freezeTime).");
			}
		}

		private IEnumerator DelayStartIfNeeded(DialogContent dialogContent) {
			if (dialogContent.delay > 0f) {
				Log($"Initial delay: {dialogContent.delay:0.###}s");
				yield return new WaitForSecondsRealtime(dialogContent.delay);
			}
		}

		private void PrepareFirstSpeakerSkin(DialogContent dialogContent) {
			if (dialogContent.Lines != null && dialogContent.Lines.Count > 0) {
				SetSpeakerColor(dialogContent.Lines[0].speaker);
			}
		}

		private void HandleSpeakerAndLookTargets(DialogLine line, out Character speakingCharacter,
		                                         out GameObject lookTarget) {
			speakingCharacter = CharacterManager.Instance.GetCharacter(line.speaker as CharacterID);
			if (speakingCharacter != null) {
				SpeakerChanged?.Invoke(speakingCharacter.transform);

				if (line.speakerAnimation != null) {
					var speakingAnimator = speakingCharacter.GetComponent<CharacterAnimation>();
					if (speakingAnimator != null) {
						speakingAnimator.PlayAnimation(line.speakerAnimation);
					}
				}
			}
			else {
				SpeakerChanged?.Invoke(null);
			}

			lookTarget = null;
			if (line.lookAt != null) {
				if (speakingCharacter != null) {
					lookTarget = IDService.Instance.GetGameObjectWithID(line.lookAt);
					if (lookTarget != null) {
						speakingCharacter.CharacterMovement.LookAt(lookTarget.transform.position);
					}
				}

				if (line.lookAtAnimation != null && lookTarget != null) {
					var lookAnimator = lookTarget.GetComponent<CharacterAnimation>();
					if (lookAnimator != null) {
						lookAnimator.PlayAnimation(line.lookAtAnimation);
					}
				}
			}
		}

		private IEnumerator EnsureUIVisibleThenPrepareText(DialogLine line, Character speakingCharacter) {
			yield return ShowUIIfHidden();

			textField.SetText(string.Empty);
			textField.maxVisibleCharacters = 0;
			if (speakerField) {
				speakerField.SetText(string.Empty);
			}

			DialogTextChanged?.Invoke();

			_isTextRevealed = false;
			_waitingForAdvance = true;

			textField.horizontalAlignment = line.centered
				? HorizontalAlignmentOptions.Center
				: HorizontalAlignmentOptions.Left;

			if (line.speaker != null && speakerField != null) {
				speakerField.SetText(line.speaker.prettyName);
			}

			SetSpeakerColor(line.speaker);

			if (line.focusCameraOnSpeaker && speakingCharacter != null) {
				CameraController.Instance.SetTemporaryCameraTarget(
					speakingCharacter.transform,
					speakingCharacter.CharacterMovement.ForwardTransform.eulerAngles.y + line.cameraOffsetRotation);
			}

			if (line.voiceOver != null && _audioSource != null) {
				_audioSource.pitch = 1f;
				_audioSource.PlayOneShot(line.voiceOver);
			}

			textField.SetText(line.text);
			textField.ForceMeshUpdate();
			textField.maxVisibleCharacters = 0;
			DialogTextChanged?.Invoke();

			Log($"Prepared text. centered={line.centered}, len={line.text?.Length ?? 0}");
		}

		private IEnumerator RevealTextCharacters(DialogLine line, DialogContent dialogContent) {
			bool playTalkingSounds = (_currentSpeakerSkin.sounds != null &&
			                          _currentSpeakerSkin.sounds.Length > 0 &&
			                          line.voiceOver == null);

			for (int i = 0; i < line.text.Length; i++) {
				if (_isTextRevealed) {
					textField.maxVisibleCharacters = line.text.Length;
					break;
				}

				textField.maxVisibleCharacters = i + 1;

				if (playTalkingSounds && _currentSpeakerSkin.rate > 0 && i % _currentSpeakerSkin.rate == 1) {
					_audioSource.pitch = Random.Range(1 - _currentSpeakerSkin.pitchVariance,
						1 + _currentSpeakerSkin.pitchVariance);
					_audioSource.PlayOneShot(
						_currentSpeakerSkin.sounds[Random.Range(0, _currentSpeakerSkin.sounds.Length)]);
				}

				yield return new WaitForSecondsRealtime(dialogContent.charRevealRate);
			}

			_isTextRevealed = true;
		}

		private IEnumerator WaitForAdvanceOrAuto(DialogContent dialogContent, DialogLine line) {
			if (dialogContent.autoAdvance) {
				float waitTime = line.text.Length * dialogContent.autoAdvanceCharTime;
				float elapsed = 0f;
				while (elapsed < waitTime && _waitingForAdvance) {
					elapsed += Time.unscaledDeltaTime;
					yield return null;
				}

				Log($"Auto-advance after {waitTime:0.###}s (elapsed may be shorter if player advanced).");
			}
			else {
				while (_waitingForAdvance) {
					yield return null;
				}

				Log("Manual advance.");
			}
		}

		#endregion


		#region Debug & State helpers

		private enum DialogState {
			Idle,
			Starting,
			Running
		}

		[Conditional("UNITY_EDITOR")]
		private void Log(string msg, bool warn = false) {
			if (!verboseLogging) return;

			string prefix = $"[DialogPlayer:{_state}] ";
			if (warn) Debug.LogWarning(prefix + msg, this);
			else Debug.Log(prefix + msg, this);
		}

		[Conditional("UNITY_EDITOR")]
		private void LogLineHeader(int index, DialogLine line) {
			if (!verboseLogging) return;
			var hasNav = line.HasNavigation ? "nav" : "no-nav";
			var hasText = line.HasText ? "text" : "no-text";
			Debug.Log(
				$"[DialogPlayer:{_state}] Line {index} ({hasNav}, {hasText}) speaker={line.speaker?.prettyName ?? "None"}",
				this);
		}

		#endregion


	}

	public enum DialogStyle {
		BottomBox = 1,
		SpeechBubble = 2
	}
}
