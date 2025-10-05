namespace RyanMillerGameCore.Performance
{
    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Rendering;
    using UnityEngine.Events;
    
    /// <summary>
    /// Adjusts Render Scale based on frame timings
    /// can prioritize frame rates and send out events based on quality
    /// </summary>
    public class ResolutionScaling : Singleton<ResolutionScaling>
    {
        [Header("Frame Time Polling")]
        [SerializeField, Tooltip("Every x seconds...")] private float sampleInterval = 1;
        [SerializeField, Tooltip("Look at x frames to evaluate performance")] private uint sampleSizeInFrames = 20;

        [Header("Render Scale")]
        [SerializeField, Tooltip("Adjust render scale automatically according to runtime performance")] private bool automaticRenderScale = true;
        [SerializeField, Tooltip("The percentage threshold that justifies an increase/decrease in resolution."), Range(0, 0.99f)] private float increaseDecreaseThreshold = 0.1f;
        [SerializeField, Tooltip("Adjust render scale by up-to this much each sampleInterval")] private float renderScaleIncrement = 0.1f;
        [SerializeField, Tooltip("The lowest allowed render scale (0.3 = 30% of screen resolution)")] private float minimumRenderScale = 0.3f;

        [Header("Framerate Adjustment")]
        [SerializeField, Tooltip("The method of setting Application.targetFramerate")] private FrameRateMethod frameRateMethod = FrameRateMethod.DisplayRefresh;
        
        [SerializeField, ShowIf("frameRateMethod", (int)FrameRateMethod.UseTargetArray)]
        private TargetFrameRates targetFrameRates = new TargetFrameRates();

        public TargetFrameRates TargetFrameRates => targetFrameRates;
        
        [SerializeField, ShowIf("frameRateMethod", (int)FrameRateMethod.Locked)]
        private int fixedTargetFrameRate = 60;
        
        [Header("Unity Events")]
        #pragma warning disable CS0414
        [SerializeField] private bool showEvents = false;

        [SerializeField, ShowIf("showEvents", true)] private UnityEvent maxQualityReached;
        [SerializeField, ShowIf("showEvents", true)] private UnityEvent minQualityReached;

        private bool _resolutionScalingActive;
        
        public void SetFrameMethodAndRate(FrameRateMethod method, int newTargetFrameRate = 60)
        {
            frameRateMethod = method;
            fixedTargetFrameRate = newTargetFrameRate;
            GetRefsAndDefaults();
            SetTargetFrameRateMethod();
        }

        public void SetResolutionScalingActive(bool resolutionScalingEnabled)
        {
            _resolutionScalingActive = resolutionScalingEnabled;
            if (!resolutionScalingEnabled)
            {
                _currentRenderScale = 1;
                _urpAsset.renderScale = _currentRenderScale;
                maxQualityReached.Invoke();
            }
        }

        private int currentTargetFrameRate
        {
            get
            {
                return frameRateMethod switch
                {
                    FrameRateMethod.UseTargetArray => targetFrameRates.frameRates[_currentTargetFrameRateIndex],
                    FrameRateMethod.Locked => fixedTargetFrameRate,
                    FrameRateMethod.Unlocked => int.MaxValue,
                    FrameRateMethod.DisplayRefresh => GetMaxSupportedFramerate(),
                    _ => GetMaxSupportedFramerate()
                };
            }
        }

        // ✅ Key Fix: This returns the MAXIMUM supported framerate (i.e. monitor's refresh rate)
        private int GetMaxSupportedFramerate()
        {
            float ratio = (float)Screen.currentResolution.refreshRateRatio.value;
            int refreshRate = Mathf.RoundToInt(ratio * 60f); // 60Hz base

            // Clamp to realistic values (e.g. 30–120)
            return Mathf.Clamp(refreshRate, 30, 120);
        }

        private int _currentTargetFrameRateIndex = 0;
        private float _originalRenderScale = 1;
        private UniversalRenderPipelineAsset _urpAsset;
        private float _currentRenderScale = 1;
        private float _nextSampleTime = 1;
        private FrameTiming[] _frameTimings;

        protected override void Awake()
        {
            base.Awake();
            GetRefsAndDefaults();
            SetTargetFrameRateMethod();
        }

        private void GetRefsAndDefaults()
        {
            _urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            _frameTimings = new FrameTiming[sampleSizeInFrames];
            _originalRenderScale = _urpAsset.renderScale;
            _currentRenderScale = _originalRenderScale;
        }

        private void SetTargetFrameRateMethod()
        {
            int targetFps = currentTargetFrameRate;

            // ✅ Only set target frame rate if it's valid and within monitor limits
            Application.targetFrameRate = targetFps <= GetMaxSupportedFramerate()
                ? targetFps
                : GetMaxSupportedFramerate();
        }

        private void Update()
        {
            if (!_resolutionScalingActive) return;

            FrameTimingManager.CaptureFrameTimings();

            if (Time.unscaledTime > _nextSampleTime)
            {
                float timingScore = GetFrameTimeScore();
                if (automaticRenderScale)
                {
                    ProcessRenderScale(timingScore);
                }

                _nextSampleTime = Time.unscaledTime + sampleInterval;
            }
        }

        private float GetFrameTimeScore()
        {
            int collected = (int)FrameTimingManager.GetLatestTimings(sampleSizeInFrames, _frameTimings);
            if (collected == 0)
            {
                Debug.Log("No frame timings collected.");
                return 1;
            }

            double totalGpuTime = 0;
            for (int i = 0; i < collected; i++)
            {
                totalGpuTime += _frameTimings[i].gpuFrameTime;
            }

            float gpuAverageFrameTime = (float)(totalGpuTime / collected);
            float frameTimeTarget = 1000f / currentTargetFrameRate;
            return frameTimeTarget / gpuAverageFrameTime;
        }

        private void ProcessRenderScale(float timingScore)
        {
            if (timingScore > 1 + increaseDecreaseThreshold)
            {
                ChangeRenderScale(1);
            }
            else if (timingScore < 1 - increaseDecreaseThreshold)
            {
                ChangeRenderScale(-1);
            }
        }

        private void ChangeRenderScale(float dir)
        {
            float newRenderScale = _currentRenderScale + dir * renderScaleIncrement;
            newRenderScale = Mathf.Clamp(newRenderScale, minimumRenderScale, 1);

            // Early out if no change
            if (Mathf.Approximately(newRenderScale, _currentRenderScale))
                return;

            // Apply new render scale
            _currentRenderScale = newRenderScale;
            _urpAsset.renderScale = _currentRenderScale;

            // Fire events only when we hit min/max quality
            if (Mathf.Approximately(newRenderScale, minimumRenderScale))
            {
                minQualityReached.Invoke();
            }
            else if (Mathf.Approximately(newRenderScale, 1))
            {
                maxQualityReached.Invoke();
            }
        }

        /// <summary>
        /// Only used when frameRateMethod == UseTargetArray
        /// Only changes index if target frame rate is within monitor support
        /// </summary>
        private void ChangeFrameRate(int dir)
        {
            _currentTargetFrameRateIndex += dir;
            _currentTargetFrameRateIndex = Mathf.Clamp(_currentTargetFrameRateIndex, 0, targetFrameRates.frameRates.Length - 1);

            int newFps = targetFrameRates.frameRates[_currentTargetFrameRateIndex];
            if (newFps <= GetMaxSupportedFramerate())
            {
                Application.targetFrameRate = newFps;
            }
        }

        private void OnDisable()
        {
            if (_urpAsset)
            {
                _urpAsset.renderScale = _originalRenderScale;
            }
        }

        // ✅ Prevents invalid frame rates
        private bool IsValidFrameRate(int fps)
        {
            return fps >= 30 && fps <= GetMaxSupportedFramerate();
        }
    }

    [System.Serializable]
    public class TargetFrameRates
    {
        public int[] frameRates = new int[3] { 30, 60, 120 };
    }

    public enum FrameRateMethod
    {
        Unlocked = 0,
        UseTargetArray = 1,
        DisplayRefresh = 2,
        Locked = 3
    }
}