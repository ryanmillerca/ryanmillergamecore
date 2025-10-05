namespace RyanMillerGameCore.Utilities
{
    using UnityEngine;
    using Random = UnityEngine.Random;

    [System.Serializable]
    public class AudioEntry
    {
        [SerializeField] private AudioClip[] clips;
        [SerializeField] private float pitchVariance = 0;
        [SerializeField] private AudioType audioType = AudioType.SFX;

        private bool _disabled = false;
        private AudioSource _lastAudioSource;

        private float SoundEffectsVolume => 1f;
        private float MusicVolume => 1f;
        private float Volume => audioType == AudioType.SFX ? SoundEffectsVolume : MusicVolume;

        private float Pitch
        {
            get
            {
                if (pitchVariance != 0)
                {
                    return 1 + Random.Range(-pitchVariance, pitchVariance);
                }
                return 1;
            }
        }

        public void PlayLooped(AudioSource source)
        {
            if (_disabled)
            {
                return;
            }

            if (source == null)
            {
                return;
            }

            if (clips == null || clips.Length == 0)
            {
                return;
            }

            source.pitch = Pitch;
            source.volume = Volume;
            source.loop = true;
            source.clip = clips[Random.Range(0, clips.Length)];
            source.Play();

            _lastAudioSource = source;
        }

        public void StopLoop(AudioSource source)
        {
            source.Stop();
            source.loop = false;
            source.clip = null;
        }

        public void Stop(AudioSource source)
        {
            source.Stop();
        }

        public void Play(AudioSource source, Vector3 position)
        {
            if (source == null || _disabled)
            {
                return;
            }

            source.volume = Volume;
            source.pitch = Pitch;
            source.transform.position = position;
            source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            _lastAudioSource = source;
        }


        public void Play(AudioSource source)
        {
            if (source == null || _disabled)
            {
                return;
            }

            if (clips == null || clips.Length == 0)
            {
                return;
            }

            source.volume = Volume;
            source.pitch = Pitch;
            source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            _lastAudioSource = source;
        }


        private void Awake()
        {
            if (clips == null || clips.Length == 0)
            {
                _disabled = true;
            }
        }
    }

    public enum AudioType
    {
        SFX,
        Music
    }
}