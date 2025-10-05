namespace RyanMillerGameCore.Character.SMB
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Collections;
    using Utilities;

    /// <summary>
    /// Uses Emission channel to create a pulsing effect on the character's materials.
    /// </summary>
    public class EmissionPulseSMB : CharacterSMB
    {
        [SerializeField] private Color emissionColor = Color.red;
        [SerializeField, Range(0f,1f)] private float pulseDuration = 1f;
        
        private Coroutine _pulseCoroutine;
        private float _pulseDuration;
        private Material[] _materials;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            
            _pulseDuration = stateInfo.length * pulseDuration;
            
            // Get all materials from all renderers (shared instances, not instanced copies)
            if (_materials == null)
            {
                _materials = GetAllMaterials(References.renderers);
                foreach (var mat in _materials)
                {
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor(EmissionColor, emissionColor);
                    }
                }
            }
            _pulseCoroutine = CoroutineRunner.Instance.StartCoroutine(AnimateEmission());
        }

        private IEnumerator AnimateEmission()
        {
            for (float i = 0; i <= _pulseDuration; i += Time.deltaTime)
            {
                float t = i / _pulseDuration;
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
            var allMaterials = new List<Material>();
            foreach (var rend in renderers)
            {
                if (rend != null)
                {
                    allMaterials.AddRange(rend.materials);
                }
            }
            return allMaterials.ToArray();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            CoroutineRunner.Instance.StopCoroutine(_pulseCoroutine);
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