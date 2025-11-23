namespace RyanMillerGameCore.Character {
	using System;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using Camera;
	using UnityEngine.EventSystems;

	public class CharacterInput : MonoBehaviour {
		[Header("References")]
		[SerializeField] private InputActionReference inputActionMove;
		[SerializeField] private InputActionReference inputActionAttack;
		[SerializeField] private InputActionReference inputActionInteract;
		[SerializeField] private InputActionReference inputActionPause;
		[SerializeField] private InputActionReference inputActionPointerDown;
		[SerializeField] private InputActionReference inputActionPointerPosition;
		[Tooltip("Block CharacterInput when pointer is over any UI")]
		[SerializeField] private bool disableInputWhenPointerOverUI;
		[Tooltip("Directions = Screen Directions. Turn off for local Up/Left/Right/Down that doesn't necessarily match the view")]
		[SerializeField] private bool syncMoveDirectionWithCamera = true;
		[Tooltip("Restrict movement to 4-directional only (no diagonals)")]
		[SerializeField] private bool cartesianMovementOnly = false;

		[NonSerialized] private bool _inputEnabled = true;
		[NonSerialized] private bool _hasPlayerCamera = false;
		[NonSerialized] private Vector2 _movementInput;
		[NonSerialized] private CharacterBrain _characterBrain;
		[NonSerialized] private bool _pointerIsDown;
		[NonSerialized] private bool _isPointerOverGameObject;
		[NonSerialized] private Camera _playerCamera;

		private Camera playerCamera {
			get {
				if (_playerCamera) {
					return _playerCamera;
				}
				if (PlayerCamera.Instance) {
					if (PlayerCamera.Instance.Camera) {
						_playerCamera = PlayerCamera.Instance.Camera;
						_hasPlayerCamera = true;
						return _playerCamera;
					}
				}
				if (Camera.main) {
					_playerCamera = Camera.main;
					_hasPlayerCamera = true;
					return _playerCamera;
				}
				_hasPlayerCamera = false;
				return null;
			}
		}

		private bool IsMoving() {
			return Mathf.Abs(_movementInput.magnitude) > 0;
		}

		public void SetInputEnabled(bool inputEnabled) {
			_inputEnabled = inputEnabled;
			if (!inputEnabled) {
				_movementInput = Vector2.zero;
				_characterBrain.MoveInDirection(Vector3.zero);
			}
		}

		public void SetMovementEnabled(bool movementEnabled) {
			if (movementEnabled) {
				inputActionMove.action.Enable();
			}
			else {
				inputActionMove.action.Disable();
			}
		}

		public void SetAttackEnabled(bool attackEnabled) {
			if (attackEnabled) {
				inputActionAttack.action.Enable();
			}
			else {
				inputActionAttack.action.Disable();
			}
		}

		public void SetInteractEnabled(bool interactEnabled) {
			if (interactEnabled) {
				inputActionInteract.action.Enable();
			}
			else {
				inputActionInteract.action.Disable();
			}
		}

		public void FireInteractFromUI() {
			DoInteract();
		}

		private void OnEnable() {
			_characterBrain = GetComponent<CharacterBrain>();
			if (GameStateManager.Instance) {
				GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
			}

			if (inputActionMove != null) {
				inputActionMove.action.Enable();
			}

			if (inputActionAttack != null) {
				inputActionAttack.action.Enable();
				inputActionAttack.action.performed += OnAttack;
			}

			if (inputActionInteract != null) {
				inputActionInteract.action.Enable();
				inputActionInteract.action.performed += OnInteract;
			}

			if (inputActionPause != null) {
				inputActionPause.action.Enable();
				inputActionPause.action.performed += OnPauseAction;
			}

			if (inputActionPointerDown != null) {
				inputActionPointerDown.action.Enable();
				inputActionPointerDown.action.performed += OnPointerDown;
				inputActionPointerDown.action.canceled += OnPointerDown;
			}

			if (inputActionPointerPosition != null) {
				inputActionPointerPosition.action.Enable();
				inputActionPointerPosition.action.performed += OnPointerPosition;
			}
		}

		private void Start() {
			_playerCamera = playerCamera;
		}

		private void OnGameStateChanged(GameState newGameState) {
			SetInputActionsEnabled(newGameState == GameState.Gameplay);
			if (newGameState == GameState.Paused) {
				inputActionPause.action.Enable();
			}
		}

		private void SetInputActionsEnabled(bool inputEnabled) {
			if (inputEnabled) {
				inputActionMove.action.Enable();
				inputActionPointerDown.action.Enable();
				inputActionPointerPosition.action.Enable();
				inputActionAttack.action.Enable();
				inputActionInteract.action.Enable();
				inputActionPause.action.Enable();
			}
			else {
				inputActionMove.action.Disable();
				inputActionPointerDown.action.Disable();
				inputActionPointerPosition.action.Disable();
				inputActionAttack.action.Disable();
				inputActionInteract.action.Disable();
				inputActionPause.action.Disable();
			}
		}

		private void OnDisable() {
			if (GameStateManager.Instance) {
				GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
			}

			SetInputActionsEnabled(false);
			inputActionPointerDown.action.performed -= OnPointerDown;
			inputActionPointerDown.action.canceled -= OnPointerDown;
			inputActionPointerPosition.action.performed -= OnPointerPosition;
			inputActionAttack.action.performed -= OnAttack;
			inputActionInteract.action.performed -= OnInteract;
			inputActionPause.action.performed -= OnPauseAction;
		}

		private void UpdateMovementDirection() {
			if (_movementInput.sqrMagnitude > 0) {
				Vector3 moveDirection;

				if (syncMoveDirectionWithCamera && _hasPlayerCamera) {
					// Use camera-relative movement
					Vector3 cameraForward = playerCamera.transform.forward;
					Vector3 cameraRight = playerCamera.transform.right;
					cameraForward.y = 0;
					cameraRight.y = 0;
					cameraForward.Normalize();
					cameraRight.Normalize();
					moveDirection = cameraForward * _movementInput.y + cameraRight * _movementInput.x;
				}
				else {
					// Use raw input for local movement
					moveDirection = new Vector3(_movementInput.x, 0, _movementInput.y);
				}

				_characterBrain.MoveInDirection(moveDirection);
			}
		}

		private void OnMove(InputAction.CallbackContext context) { }

		private void ProcessMove() {
			// If input is disabled or the pointer is over UI, make sure the character stops
			if (_inputEnabled == false || IsPointerOverUI) {
				_movementInput = Vector2.zero;
				_characterBrain.GoToIdle(); // ensure the brain isn't left moving
				return;
			}

			Vector2 move = inputActionMove.action.ReadValue<Vector2>();

			// Apply Cartesian restriction if enabled
			if (cartesianMovementOnly && move != Vector2.zero) {
				move = GetCartesianDirection(move);
			}

			// if there's no movement, ensure idle
			if (move.sqrMagnitude <= 0f) {
				_movementInput = Vector2.zero;
				_characterBrain.GoToIdle();
				return;
			}

			_movementInput = move;
			_characterBrain.GoToMove();
		}

		private void OnPointerPosition(InputAction.CallbackContext context) {
			if (_inputEnabled == false) {
				return;
			}
			if (_pointerIsDown == false) {
				return;
			}
			Vector2 posInput = context.ReadValue<Vector2>();
			Vector3 screenPosOfTransform = playerCamera.WorldToScreenPoint(transform.position);
			Vector2 direction = posInput - (Vector2)screenPosOfTransform;

			// Normalize for joystick-style vector (-1 to 1 range)
			Vector2 movementInput = direction.normalized;

			// Apply Cartesian restriction if enabled
			if (cartesianMovementOnly && movementInput != Vector2.zero) {
				movementInput = GetCartesianDirection(movementInput);
			}

			_movementInput = movementInput;
			_characterBrain.GoToMove();

			// only calling this here will result in the movement directions not updating when the camera rotates
			if (!syncMoveDirectionWithCamera) {
				UpdateMovementDirection();
			}
		}

		private void OnPointerDown(InputAction.CallbackContext context) {
			if (context.performed) {
				if (_isPointerOverGameObject) {
					return;
				}
				_pointerIsDown = true;
			}
			else if (context.canceled) {
				_movementInput = Vector2.zero;
				_pointerIsDown = false;
				_characterBrain.GoToIdle();
			}
		}

		private bool IsPointerOverUI {
			get {
				if (!disableInputWhenPointerOverUI) {
					return false;
				}
				return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
			}
		}

		private void Update() {
			ProcessMove();

			if (IsMoving()) {
				UpdateMovementDirection();
			}
		}

		private void OnAttack(InputAction.CallbackContext context) {
			if (_inputEnabled == false) {
				return;
			}
			if (context.performed) {
				_characterBrain.Attack();
			}
		}

		private void OnInteract(InputAction.CallbackContext context) {
			if (context.performed) {
				DoInteract();
			}
		}

		private void DoInteract() {
			if (_inputEnabled == false) {
				return;
			}
			_characterBrain.Interact();
		}

		private void OnPauseAction(InputAction.CallbackContext context) {
			if (_inputEnabled == false) {
				return;
			}
			if (context.performed) {
				GameStateManager.Instance.TogglePause();
			}
		}

		private Vector2 GetCartesianDirection(Vector2 input) {
			// If input magnitude is very small, return zero
			if (input.magnitude < 0.1f) {
				return Vector2.zero;
			}

			// Determine which axis has the larger absolute value
			if (Mathf.Abs(input.x) > Mathf.Abs(input.y)) {
				// Horizontal movement dominates
				return new Vector2(Mathf.Sign(input.x), 0);
			}
			else {
				// Vertical movement dominates
				return new Vector2(0, Mathf.Sign(input.y));
			}
		}
	}
}
