using RyanMillerGameCore.Utilities;
namespace RyanMillerGameCore.Audio {

	using UnityEngine;

	[RequireComponent(typeof(AudioSource))]
	public class AudioSourcePlayer : MonoBehaviour {

		private AudioSource m_audioSource;
		private AudioSource AudioSource {
			get {
				if (!m_audioSource) {
					m_audioSource = GetComponent<AudioSource>();
				}
				return m_audioSource;
			}
		}

		[MinMaxRange(-1f, 1f), SerializeField] private Vector2 m_PitchRange = new Vector2(0f, 0f);

		public void PlayOneShot(AudioClip clip) {
			AudioSource.pitch = 1 + Random.Range(m_PitchRange.x, m_PitchRange.y);
			AudioSource.PlayOneShot(clip);
		}

		public void ResetPitch() {
			m_PitchRange = new Vector2(0, 0);
		}
	}
}
