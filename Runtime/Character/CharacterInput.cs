namespace RyanMillerGameCore.Character
{
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Camera;
    using UnityEngine.EventSystems;

    public class CharacterInput : MonoBehaviour
    {
        [Header("References")] 
        
        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private bool syncMoveDirectionWithCamera = true;
        [SerializeField] private Camera playerCamera;
        
        [NonSerialized] private bool _inputEnabled = true;
        [NonSerialized] private bool _hasPlayerCamera = false;
        [NonSerialized] private Vector2 _movementInput;
        [NonSerialized] private CharacterBrain _characterBrain;
        [NonSerialized] private InputAction _moveAction;
        [NonSerialized] private InputAction _attackAction;
        [NonSerialized] private InputAction _interactAction;
        [NonSerialized] private InputAction _pauseAction;

        // pointer only
        private InputAction _pointerDown;
        private InputAction _pointerPosition;
        private bool _pointerIsDown;
        private bool _isPointerOverGameObject;
        
        public bool IsMoving()
        {
            return Mathf.Abs(_movementInput.magnitude) > 0;
        }

        public void SetInputEnabled(bool inputEnabled)
        {
            _inputEnabled = inputEnabled;
            if (!inputEnabled)
            {
                _movementInput = Vector2.zero;
                _characterBrain.MoveInDirection(Vector3.zero);
            }
        }

        public void SetMovementEnabled(bool movementEnabled)
        {
            if (movementEnabled)
            {
                _moveAction.Enable();
            }
            else
            {
                _moveAction.Disable();
            }
        }
        
        public void SetAttackEnabled(bool attackEnabled)
        {
            if (attackEnabled)
            {
                _attackAction.Enable();
            }
            else
            {
                _attackAction.Disable();
            }
        }

        public void SetInteractEnabled(bool interactEnabled)
        {
            if (interactEnabled)
            {
                _interactAction.Enable();
            }
            else
            {
                _interactAction.Disable();
            }
        }

        public void FireInteractFromUI()
        {
            DoInteract();
        }

        private void OnEnable()
        {
            _characterBrain = GetComponent<CharacterBrain>();
            var actionMap = inputActionAsset.FindActionMap("Player", true);
            if (GameStateManager.Instance)
            {
                GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }

            _moveAction = actionMap.FindAction("Move", true);
            _moveAction.Enable();

            _attackAction = actionMap.FindAction("Attack", true);
            _attackAction.performed += OnAttack;
            _attackAction.Enable();

            _interactAction = actionMap.FindAction("Interact", true);
            _interactAction.performed += OnInteract;
            _interactAction.Enable();

            _pauseAction = actionMap.FindAction("Pause", true);
            _pauseAction.performed += OnPauseAction;
            _pauseAction.Enable();

            _pointerDown = actionMap.FindAction("PointerDown",true);
            _pointerDown.performed += OnPointerDown;
            _pointerDown.canceled += OnPointerDown;
            _pointerDown.Enable();

            _pointerPosition = actionMap.FindAction("PointerPosition",true);
            _pointerPosition.performed += OnPointerPosition;
            _pointerPosition.Enable();
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            SetInputActionsEnabled(newGameState == GameState.Gameplay);
            if (newGameState == GameState.Paused)
            {
                _pauseAction.Enable();
            }
        }
        
        private void SetInputActionsEnabled(bool inputEnabled){
            if (inputEnabled)
            {
                _moveAction.Enable();
                _pointerDown.Enable();
                _pointerPosition.Enable();
                _attackAction.Enable();
                _interactAction.Enable();
                _pauseAction.Enable();
            }
            else
            {
                _moveAction.Disable();
                _pointerDown.Disable();
                _pointerPosition.Disable();
                _attackAction.Disable();
                _interactAction.Disable();
                _pauseAction.Disable();
            }
        }

        private void Start()
        {
            if (playerCamera == null && PlayerCamera.Instance)
            {
                playerCamera = PlayerCamera.Instance.Camera;
            }
            _hasPlayerCamera = playerCamera != null;
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance)
            {
                GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }

            SetInputActionsEnabled(false);
            _pointerDown.performed -= OnPointerDown;
            _pointerDown.canceled -= OnPointerDown;
            _pointerPosition.performed -= OnPointerPosition;
            _attackAction.performed -= OnAttack;
            _interactAction.performed -= OnInteract;
            _pauseAction.performed -= OnPauseAction;
        }

        private void UpdateMovementDirection()
        {
            if (_movementInput.sqrMagnitude > 0)
            {
                Vector3 moveDirection = new Vector3(_movementInput.x, 0, _movementInput.y);
                if (_hasPlayerCamera)
                {
                    Vector3 cameraForward = playerCamera.transform.forward;
                    Vector3 cameraRight = playerCamera.transform.right;
                    cameraForward.y = 0;
                    cameraRight.y = 0;
                    cameraForward.Normalize();
                    cameraRight.Normalize();
                    moveDirection = cameraForward * _movementInput.y + cameraRight * _movementInput.x;
                }
                _characterBrain.MoveInDirection(moveDirection);
            }
        }

        private void OnMove(InputAction.CallbackContext context) { }
        
        private void ProcessMove() {

            Vector2 move = _moveAction.ReadValue<Vector2>();
            _movementInput = Vector2.zero;
            
            if (_inputEnabled == false || _isPointerOverGameObject)
            {
                return;
            }
            if (move.sqrMagnitude <= 0)
            {
                _characterBrain.GoToIdle();
                return;
            }

            _movementInput = move;
            _characterBrain.GoToMove();

            if (!syncMoveDirectionWithCamera)
            {
                UpdateMovementDirection();
            }
        }

        private void OnPointerPosition(InputAction.CallbackContext context) {
            if (_inputEnabled == false)
            {
                return;
            }
            if (_pointerIsDown == false)
            {
                return;
            }
            Vector2 posInput = context.ReadValue<Vector2>();
            Vector3 screenPosOfTransform = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 direction = posInput - (Vector2)screenPosOfTransform;

            // Normalize for joystick-style vector (-1 to 1 range)
            _movementInput = direction.normalized;
            _characterBrain.GoToMove(); 

            // only calling this here will result in the movement directions not updating when the camera rotates
            if (!syncMoveDirectionWithCamera)
            {
                UpdateMovementDirection();
            }
        }

        private void OnPointerDown(InputAction.CallbackContext context) {
            if (context.performed) {
                if (_isPointerOverGameObject)
                {
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

        private void Update()
        {
            ProcessMove();
            if (IsMoving())
            {
                // calling this here will result in movement directions that stay synced with the camera
                if (syncMoveDirectionWithCamera)
                {
                    UpdateMovementDirection();
                }
            }
            _isPointerOverGameObject = EventSystem.current.IsPointerOverGameObject();
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            if (_inputEnabled == false)
            {
                return;
            }
            if (context.performed)
            {
                _characterBrain.Attack();
            }
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                DoInteract();
            }
        }

        private void DoInteract()
        {
            if (_inputEnabled == false)
            {
                return;
            }
            _characterBrain.Interact();
        }

        private void OnPauseAction(InputAction.CallbackContext context)
        {
            if (_inputEnabled == false)
            {
                return;
            }
            if (context.performed)
            {
                GameStateManager.Instance.TogglePause();
            }
        }
    }
}