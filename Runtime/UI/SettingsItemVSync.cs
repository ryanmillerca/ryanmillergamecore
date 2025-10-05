namespace RyanMillerGameCore.UI
{
    using Settings;
    
    public class SettingsItemVSync : SettingsItem
    {
        private bool _enabled;

        protected override void RefreshSettings(GraphicsSettingsData newSettings)
        {
            _enabled = newSettings.DesiredVsync;
            SetVsync(_enabled);
        }

        private void SetVsync(bool newValue)
        {
            _enabled = newValue;
            SetLabel(newValue ? "Yes" : "No");
            GraphicsSettings.Instance.SetDesiredVsync(newValue);
        }
        
        public override void WasClicked()
        {
            SetVsync(!_enabled);
        }
    }
}