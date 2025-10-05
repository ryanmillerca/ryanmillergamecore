namespace RyanMillerGameCore.Effects
{
    using UnityEngine;
    using System.Collections;

    public class EmissionPulse : MonoBehaviour
    {
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField] private Color emissionColor = Color.red;
        [SerializeField] private float pulseDuration = 0.3f;

        private Material[] _materials;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void OnEnable()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                Debug.LogWarning("No target renderers assigned.");
                return;
            }

            // Get all materials from all renderers (shared instances, not instanced copies)
            if (_materials == null)
            {
                _materials = GetAllMaterials(targetRenderers);
                foreach (var mat in _materials)
                {
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor(EmissionColor, emissionColor);
                    }
                }
            }

            StartCoroutine(AnimateEmission());
        }

        private IEnumerator AnimateEmission()
        {
            for (float i = 0; i <= pulseDuration; i += Time.deltaTime)
            {
                float t = i / pulseDuration;
                Color lerpedColor = Color.Lerp(emissionColor, Color.black, t);

                foreach (var mat in _materials)
                {
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.SetColor(EmissionColor, lerpedColor);
                    }
                }

                yield return new WaitForEndOfFrame();
            }

            foreach (var mat in _materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor(EmissionColor, Color.black);
                }
            }
        }

        private Material[] GetAllMaterials(Renderer[] renderers)
        {
            var allMaterials = new System.Collections.Generic.List<Material>();

            foreach (var rend in renderers)
            {
                if (rend != null)
                {
                    allMaterials.AddRange(rend.materials); // ensures we get per-instance materials
                }
            }

            return allMaterials.ToArray();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            foreach (var mat in _materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor(EmissionColor, Color.black);
                }
            }
        }
    }
}