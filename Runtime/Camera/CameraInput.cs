namespace RyanMillerGameCore.Camera
{
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class CameraInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        [NonSerialized] private CameraController cameraController;
        [NonSerialized] private InputAction lookAction;
        [NonSerialized] private InputAction zoomAction;

        private Vector2 cachedLookInput = Vector2.zero;
        private float cachedZoomInput;

        private void OnEnable()
        {
            var actionMap = inputActionAsset.FindActionMap("Player", true);
            lookAction = actionMap.FindAction("Look", true);
            lookAction.performed += OnLook;
            lookAction.canceled += OnLook;
            lookAction.Enable();

            zoomAction = actionMap.FindAction("Zoom", true);
            zoomAction.performed += OnZoom;
            zoomAction.canceled += OnZoom;
            zoomAction.Enable();
        }

        private void OnDisable()
        {
            lookAction.performed -= OnLook;
            lookAction.canceled -= OnLook;
            lookAction.Disable();
            
            zoomAction.performed -= OnZoom;
            zoomAction.canceled -= OnZoom;
            zoomAction.Disable();
        }

        private void Start()
        {
            cameraController = GetComponent<CameraController>();
        }

        
        private void Update()
        {
            if (cameraController != null)
            {
                cameraController.SetLookInput(cachedLookInput);
                cameraController.SetZoomInput(cachedZoomInput);
            }
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            cachedLookInput = context.ReadValue<Vector2>();
        }
        
        private void OnZoom(InputAction.CallbackContext context)
        {
            cachedZoomInput = context.ReadValue<float>();
        }
    }
}