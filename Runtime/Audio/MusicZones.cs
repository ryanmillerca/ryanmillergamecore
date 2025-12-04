namespace RyanMillerGameCore.Audio {

	using UnityEngine;

	public class MusicZones : MonoBehaviour {

		[SerializeField] private AudioSource[] audioSources;
		private int m_CurrentAudioSource;
		private float m_MusicTransitionSpeed = 2;

		public void SetActiveAudioSource(int index) {
			if (index >= 0 && index < audioSources.Length) {
				m_CurrentAudioSource = index;
			}
			else if (index == -1) {
				m_CurrentAudioSource = -1;
			}
			else {
				Debug.LogWarning($"Invalid audio source index: {index}");
			}
		}

		private void Update() {
			for (int i = 0; i < audioSources.Length; i++) {
				AudioSource source = audioSources[i];

				if (!source) {
					continue;
				}

				if (i == m_CurrentAudioSource) {
					source.volume = Mathf.Lerp(source.volume, 1f, Time.deltaTime * m_MusicTransitionSpeed);
				}
				else if (m_CurrentAudioSource == -1) {
					source.volume = Mathf.Lerp(source.volume, 0f, Time.deltaTime * m_MusicTransitionSpeed);
				}
				else {
					source.volume = Mathf.Lerp(source.volume, 0f, Time.deltaTime * m_MusicTransitionSpeed);
				}
			}
		}
	}
}
