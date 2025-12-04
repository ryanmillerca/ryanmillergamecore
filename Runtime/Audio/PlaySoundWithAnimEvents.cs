namespace RyanMillerGameCore.Audio {
	using UnityEngine;

	[RequireComponent(typeof(AudioSourcePlayer))]
	public class PlaySoundWithAnimEvents : MonoBehaviour {

		[SerializeField] private AudioClip[] m_Clips;
		private AudioSourcePlayer m_audioSourcePlayer;

		private AudioSourcePlayer AudioSourcePlayer {
			get {
				if (m_audioSourcePlayer == null) {
					m_audioSourcePlayer = GetComponent<AudioSourcePlayer>();
				}
				return m_audioSourcePlayer;
			}
		}

		public void PlaySoundAtIndex(int index) {
			if (index < 0 || index >= m_Clips.Length) {
				Debug.Log($"AudioClip {index} is out of range. {this.name} is skipping playing the sound.", gameObject);
				return;
			}
			AudioSourcePlayer?.PlayOneShot(m_Clips[index]);
		}

		public void PlaySound() {
			if (m_Clips.Length > 0 && m_Clips[0]) {
				AudioSourcePlayer?.PlayOneShot(m_Clips[0]);
			}
			else {
				Debug.Log($"There is no AudioClip at index 0. {this.name} is skipping playing the sound.", gameObject);
			}
		}

		public void PlayRandomSound() {
			int index = UnityEngine.Random.Range(0, m_Clips.Length);
			PlaySoundAtIndex(index);
		}
	}
}
