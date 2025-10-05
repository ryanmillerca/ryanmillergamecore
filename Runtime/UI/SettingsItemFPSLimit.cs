namespace RyanMillerGameCore.UI {
	using Settings;
	using System.Linq;
	using Performance;
	using UnityEngine;

	public class SettingsItemFPSLimit : SettingsItem {
		public int[] _supportedFrameRates;
		private int _currentFrameRateIndex;

		protected override void OnEnable() {
			base.OnEnable();

			int maxRefreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);

			// Filter out frame rates above what the display supports
			_supportedFrameRates = ResolutionScaling.Instance.TargetFrameRates.frameRates
			.Where(rate => rate <= maxRefreshRate)
			.ToArray();

			// Get index of current refresh rate
			int currentFrameRate = maxRefreshRate;
			for (var index = 0; index < _supportedFrameRates.Length; index++) {
				if (_supportedFrameRates[index] == currentFrameRate) {
					_currentFrameRateIndex = index;
					break;
				}
			}

			if (currentFrameRate == 0) {
				Debug.LogWarning("Frame Rate is 0, that can't be right. Setting to 60.", gameObject);
				currentFrameRate = 60;
			}

			SetTargetFPS(currentFrameRate);
		}

		protected override void RefreshSettings(GraphicsSettingsData newSettings) {
			SetTargetFPS(newSettings.DesiredFpsLimit);
		}

		private void SetTargetFPS(int targetFps) {
			GraphicsSettings.Instance.SetDesiredFPSLimit(targetFps);
			if (targetFps == 0) {
				SetLabel("Unlimited FPS");
			}
			else {
				SetLabel(targetFps.ToString() + " FPS");
			}
		}

		public override void WasClicked() {
			base.WasClicked();
			_currentFrameRateIndex++;
			if (_currentFrameRateIndex >= _supportedFrameRates.Length) {
				_currentFrameRateIndex = 0;
			}
			SetTargetFPS(_supportedFrameRates[_currentFrameRateIndex]);
		}
	}
}
