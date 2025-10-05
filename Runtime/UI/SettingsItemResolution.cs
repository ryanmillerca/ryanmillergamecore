namespace RyanMillerGameCore.UI
{
    using UnityEngine;
    using Settings;
    
    public class SettingsItemResolution : SettingsItem
    {
        private Resolution[] _resolutions;
        private Resolution _activeResolution;
        private int _resolutionIndex = -1;
        [SerializeField] private GameObject increaseButton;
        [SerializeField] private GameObject decreaseButton;

        protected override void RefreshSettings(GraphicsSettingsData newSettings)
        {
            _activeResolution = newSettings.DesiredResolution;
            UpdateLabel(_activeResolution);
        }
        
        private void Start()
        {
            _resolutions = GraphicsSettings.Instance.GetSupportedResolutions();
            for (var index = 0; index < _resolutions.Length; index++)
            {
                var resolution = _resolutions[index];
                if (Equals(resolution, Screen.currentResolution))
                {
                    _resolutionIndex = index;
                    break;
                }
            }
            UpdateLabel(Screen.currentResolution);
        }

        public void WasClicked(int direction)
        {
            _resolutionIndex += direction;
            _resolutionIndex = Mathf.Clamp(_resolutionIndex, 0, _resolutions.Length - 1);
            // leave them enabled, was getting issues with active selection getting lost with keyboard/gamepad navigation
            //increaseButton.SetActive(_resolutionIndex < _resolutions.Length -1);
            //decreaseButton.SetActive(_resolutionIndex > 0);
            UpdateLabel(_resolutions[_resolutionIndex]);
            GraphicsSettings.Instance.SetDesiredResolution(_resolutions[_resolutionIndex]);
        }

        public override void WasClicked()
        {
            base.WasClicked();
            _resolutionIndex++;
            if (_resolutionIndex >= _resolutions.Length)
            {
                _resolutionIndex = 0;
            }
            UpdateLabel(_resolutions[_resolutionIndex]);
        }

        private void UpdateLabel(Resolution resolution)
        {
            string labelOut = resolution.width + "x" + resolution.height;
            SetLabel(labelOut);
        }
    }
}