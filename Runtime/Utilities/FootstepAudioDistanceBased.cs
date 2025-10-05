namespace RyanMillerGameCore.Utilities
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class FootstepAudioDistanceBased : MonoBehaviour
    {
        [Header("Audio Clips")] 
        [SerializeField] private AudioClip[] footstepClips;

        [Header("Settings")] 
        [SerializeField] private float stepDistance = 2f; // Distance in meters between footstep sounds
        [SerializeField] private float volume = 1f;

        private Rigidbody rb;
        private AudioSource audioSource;
        private Vector3 lastPosition;
        private float distanceTravelled;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            lastPosition = transform.position;

            // Create and configure AudioSource in code
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.volume = volume;
        }

        void Update()
        {
            Vector3 delta = transform.position - lastPosition;
            delta.y = 0f; // Ignore vertical movement if desired

            float distance = delta.magnitude;
            distanceTravelled += distance;
            lastPosition = transform.position;

            if (footstepClips.Length > 0 && distanceTravelled >= stepDistance)
            {
                PlayFootstep();
                distanceTravelled = 0f;
            }
        }

        void PlayFootstep()
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            audioSource.pitch = Random.Range(0.95f, 1.05f); // Slight pitch variation
            audioSource.PlayOneShot(clip);
        }
    }
}