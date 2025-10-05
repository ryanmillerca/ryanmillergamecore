namespace RyanMillerGameCore.Effects
{
    using UnityEngine;

    public class ParticleOnEnable : MonoBehaviour
    {
        [SerializeField] private ParticleSystem enabledParticleSystem;

        private void OnEnable()
        {
            if (enabledParticleSystem)
            {
                enabledParticleSystem.Play();
            }
        }
    }
}