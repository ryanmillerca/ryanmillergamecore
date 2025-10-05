namespace RyanMillerGameCore.Settings
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using Performance;

    public class GraphicsSettings : Singleton<GraphicsSettings>
    {
        [SerializeField] private float minAspectRatio = 4f / 3f;
        [SerializeField] private float maxAspectRatio = 21f / 9f;
        [SerializeField] private int minResolutionY = 360;
        
        #region Get Data

        private void Start()
        {
            LoadGraphicsSettings(true);
        }

        public event System.Action<GraphicsSettingsData> GraphicsSettingsChanged;
        
        public Resolution[] GetSupportedResolutions()
        {
            List<Resolution> filtered = new List<Resolution>();

            foreach (Resolution res in Screen.resolutions)
            {
                float aspectRatio = (float)res.width / res.height;
                
                if (aspectRatio >= minAspectRatio &&
                    aspectRatio <= maxAspectRatio &&
                    res.height >= minResolutionY)
                {
                    filtered.Add(res);
                }
            }

            return filtered.Distinct().ToArray();
        }

        public double[] GetSupportedFrameRateTargets()
        {
            return Screen.resolutions
                .Select(res => res.refreshRateRatio.value)
                .Distinct()
                .OrderBy(fps => fps)
                .ToArray();
        }
        
        #endregion
        
        #region Set Data

        private GraphicsSettingsData _graphicsSettingsDataOld;
        private GraphicsSettingsData _graphicsSettingsDataNew;
        private bool _willRequireConfirmation;

        public bool WillRequireConfirmation
        {
            get { return _willRequireConfirmation; }
        }
        
        public void SetDesiredResolution(Resolution resolution)
        {
            _graphicsSettingsDataNew.DesiredResolution = resolution;
            _willRequireConfirmation = true;
        }

        public void SetDesiredFullscreen(bool isFullscreen)
        {
            _graphicsSettingsDataNew.DesiredFullscreen = isFullscreen;
            _willRequireConfirmation = true;
        }

        public void SetDesiredVsync(bool vsync)
        {
            _graphicsSettingsDataNew.DesiredVsync = vsync;
        }

        public void SetDesiredFPSLimit(int fpsLimit)
        {
            _graphicsSettingsDataNew.DesiredFpsLimit = fpsLimit;
        }

        public void SetDesiredAutoQuality(bool autoQuality)
        {
            _graphicsSettingsDataNew.DesiredAutoQuality = autoQuality;
        }

        public void ApplySettings()
        {
            ApplyGraphicSettingData(_graphicsSettingsDataNew);
        }

        public void ApplyOldSettings()
        {
            _willRequireConfirmation = false;
            ApplyGraphicSettingData(_graphicsSettingsDataOld);
        }

        public void SaveNewSettings()
        {
            _willRequireConfirmation = false;
            _graphicsSettingsDataOld = _graphicsSettingsDataNew;
            PlayerPrefs.SetString("GraphicsSettings", JsonUtility.ToJson(_graphicsSettingsDataNew));
        }

        public void RevertSettings()
        {
            ApplyGraphicSettingData(_graphicsSettingsDataOld);
            GraphicsSettingsChanged?.Invoke(_graphicsSettingsDataOld);
        }

        public void LoadGraphicsSettings(bool loadFromSave)
        {
            if (loadFromSave)
            {
                // Load settings from PlayerPrefs
                string savedSettingsJson = PlayerPrefs.GetString("GraphicsSettings", string.Empty);

                if (!string.IsNullOrEmpty(savedSettingsJson))
                {
                    _graphicsSettingsDataOld = JsonUtility.FromJson<GraphicsSettingsData>(savedSettingsJson);
                }
                else
                {
                    // Fallback to sensible defaults if nothing is saved
                    _graphicsSettingsDataOld = new GraphicsSettingsData
                    {
                        DesiredResolution = Screen.currentResolution,
                        DesiredFullscreen = Screen.fullScreen,
                        DesiredVsync = true,
                        DesiredFpsLimit = 60,
                        DesiredAutoQuality = true
                    };
                }
            }

            // in some cases, "revert" would set resolution to 0,0 which is obviously not good
            if (_graphicsSettingsDataOld.DesiredResolution.width < 1 ||
                _graphicsSettingsDataOld.DesiredResolution.height < 1)
            {
                _graphicsSettingsDataOld.DesiredResolution = Screen.currentResolution;
            }

            _graphicsSettingsDataNew = _graphicsSettingsDataOld;
            GraphicsSettingsChanged?.Invoke(_graphicsSettingsDataNew);
            _willRequireConfirmation = false;
        }

        private void ApplyGraphicSettingData(GraphicsSettingsData settings)
        {
            Screen.SetResolution(settings.DesiredResolution.width, settings.DesiredResolution.height, settings.DesiredFullscreen);
            Screen.fullScreenMode = settings.DesiredFullscreen ?  FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            
            QualitySettings.vSyncCount = settings.DesiredVsync ? 1 : 0;
            
            FrameRateMethod newFrameRateMethod = FrameRateMethod.Locked;
            if (settings.DesiredFpsLimit == 0)
            {
                newFrameRateMethod = FrameRateMethod.Unlocked;
            }
            else if (settings.DesiredVsync)
            {
                newFrameRateMethod = FrameRateMethod.DisplayRefresh;
                settings.DesiredFpsLimit = Mathf.RoundToInt((float)settings.DesiredResolution.refreshRateRatio.value);
            }
            else if (settings.DesiredFpsLimit > 0)
            {
                newFrameRateMethod = FrameRateMethod.Locked;
            }

            if (ResolutionScaling.Instance)
            {
                ResolutionScaling.Instance.SetResolutionScalingActive(settings.DesiredAutoQuality);
                ResolutionScaling.Instance.SetFrameMethodAndRate(newFrameRateMethod, settings.DesiredFpsLimit);
            }
        }
        
        #endregion
    }

    [System.Serializable]
    public struct GraphicsSettingsData
    {
        public Resolution DesiredResolution;
        public bool DesiredFullscreen;
        public bool DesiredVsync;
        public int DesiredFpsLimit;
        public bool DesiredAutoQuality;
    }
}