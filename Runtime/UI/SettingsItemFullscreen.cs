namespace RyanMillerGameCore.UI
{
    using Settings;

    public class SettingsItemFullscreen : SettingsItem
    {
        private bool _enabled;
        
        protected override void RefreshSettings(GraphicsSettingsData newSettings)
        {
            _enabled = newSettings.DesiredFullscreen;
            SetFullscreen(_enabled);
        }

        private void SetFullscreen(bool newValue)
        {
            _enabled = newValue;
            SetLabel(newValue ? "Yes" : "No");
            GraphicsSettings.Instance.SetDesiredFullscreen(newValue);
        }
        
        public override void WasClicked()
        {
            SetFullscreen(!_enabled);
        }
    }
}