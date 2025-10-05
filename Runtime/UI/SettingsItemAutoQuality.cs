namespace RyanMillerGameCore.UI
{
    using Settings;
    
    public class SettingsItemAutoQuality : SettingsItem
    {
        private bool _enabled;
        
        protected override void RefreshSettings(GraphicsSettingsData newSettings)
        {
            _enabled = newSettings.DesiredAutoQuality;
            SetAutoQuality(_enabled);
        }

        private void SetAutoQuality(bool newValue)
        {
            _enabled = newValue;
            SetLabel(newValue ? "Automatic" : "Highest");
            GraphicsSettings.Instance.SetDesiredAutoQuality(newValue);
        }
        
        public override void WasClicked()
        {
            SetAutoQuality(!_enabled);
        }
    }
}