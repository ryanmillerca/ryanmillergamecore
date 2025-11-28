namespace RyanMillerGameCore.UI
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using System.Collections.Generic;
    using UnityEngine.InputSystem.Controls;
    using System;
    
    public class PerPlatformPrompt : MonoBehaviour
    {
        [SerializeField] private PromptMappingDatabase _mappingDatabase;
        [SerializeField] private float _inputDebounceTime = 0.2f;
        [SerializeField] private InputActionPromptPair[] m_Pairs = new InputActionPromptPair[] { };

        private PromptGraphics _currentGraphics;
        private InputDevice _activeDevice;
        private float _lastGamepadInputTime;
        private float _lastKeyboardMouseInputTime;
        private readonly List<InputDevice> _eligibleGamepads = new();

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChanged;
            InputSystem.onAfterUpdate += OnAfterInputUpdate;
            RefreshEligibleDevices();
            UpdateActiveDevice(force: true);
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChanged;
            InputSystem.onAfterUpdate -= OnAfterInputUpdate;
        }

        private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added when device is Gamepad:
                    _eligibleGamepads.Add(device);
                    break;

                case InputDeviceChange.Removed:
                    _eligibleGamepads.Remove(device);
                    if (device == _activeDevice)
                        UpdateActiveDevice();
                    break;

                case InputDeviceChange.ConfigurationChanged:
                    if (!_eligibleGamepads.Contains(device) && device is Gamepad)
                        _eligibleGamepads.Add(device);
                    break;
            }
        }

        private void OnAfterInputUpdate()
        {
            if (Time.unscaledTime - Mathf.Max(_lastGamepadInputTime, _lastKeyboardMouseInputTime) < _inputDebounceTime)
                return;

            bool keyboardMouseActive = CheckKeyboardMouseActivity();
            bool gamepadActive = CheckGamepadActivity();

            // Only switch if there's new activity and the other input method hasn't been used more recently
            if (keyboardMouseActive && _lastKeyboardMouseInputTime > _lastGamepadInputTime)
            {
                if (_activeDevice != null)
                    UpdateActiveDevice();
            }
            else if (gamepadActive)
            {
                if (!(_activeDevice is Gamepad))
                    UpdateActiveDevice();
            }
        }

        private bool CheckKeyboardMouseActivity()
        {
            bool hasInput = (Keyboard.current?.anyKey.isPressed == true) ||
                            (Mouse.current?.delta.ReadValue().magnitude > 0.1f);

            if (hasInput)
                _lastKeyboardMouseInputTime = Time.unscaledTime;

            return hasInput;
        }

        private bool CheckGamepadActivity()
        {
            foreach (var device in _eligibleGamepads)
            {
                if (HasMeaningfulGamepadInput(device))
                {
                    _lastGamepadInputTime = Time.unscaledTime;
                    return true;
                }
            }

            return false;
        }

        private void UpdateActiveDevice(bool force = false)
        {
            var newDevice = DetermineActiveDevice();

            if (!force && newDevice == _activeDevice)
                return;

            _activeDevice = newDevice;
            _currentGraphics = _mappingDatabase.GetGraphicsForDevice(_activeDevice);
            OnPromptGraphicsChanged?.Invoke(_currentGraphics);
        }

        private InputDevice DetermineActiveDevice()
        {
            // Keyboard/mouse has priority if used more recently
            if (_lastKeyboardMouseInputTime > _lastGamepadInputTime)
                return null; // null represents keyboard/mouse

            // Otherwise use the most recently active gamepad
            foreach (var device in _eligibleGamepads)
            {
                if (HasMeaningfulGamepadInput(device))
                    return device;
            }

            return _activeDevice is Gamepad ? _activeDevice : null;
        }

        private bool HasMeaningfulGamepadInput(InputDevice device)
        {
            if (device == null) return false;

            foreach (var control in device.allControls)
            {
                if (!control.IsActuated())
                    continue;

                if (control is StickControl stick && stick.ReadValue().magnitude < 0.3f)
                    continue;

                if (control is ButtonControl button && button.ReadValue() < 0.5f)
                    continue;

                return true;
            }

            return false;
        }

        private void RefreshEligibleDevices()
        {
            _eligibleGamepads.Clear();
            foreach (var device in InputSystem.devices)
            {
                if (device is Gamepad)
                    _eligibleGamepads.Add(device);
            }
        }
        
        public Sprite GetSpriteForInputActionReference(InputActionReference inputActionReference) {
            Sprite sprite = null;
            foreach (var inputActionPromptPair in m_Pairs) {
                if (inputActionPromptPair.inputActionReference.Equals(inputActionReference)) {
                    sprite = GetSpriteFor(inputActionPromptPair.promptAction);
                }
            }
            return sprite;
        }
        public Sprite GetSpriteFor(PromptAction action) => action switch
        {
            PromptAction.Attack => _currentGraphics?.attack,
            PromptAction.Dodge => _currentGraphics?.dodge,
            PromptAction.Interact => _currentGraphics?.interact,
            PromptAction.Look => _currentGraphics?.look,
            PromptAction.Move => _currentGraphics?.move,
            PromptAction.Pause => _currentGraphics?.pause,
            PromptAction.Special => _currentGraphics?.special,
            _ => null
        };

        
        public event System.Action<PromptGraphics> OnPromptGraphicsChanged;
    }
    
    [Serializable]
    public class InputActionPromptPair {
        public InputActionReference inputActionReference;
        public PromptAction promptAction;
    }
}