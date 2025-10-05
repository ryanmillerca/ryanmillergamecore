namespace RyanMillerGameCore.Performance
{
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

    /// <summary>
    /// Controls the Render Feature "Screen Space Ambient Occlusion" in the Universal Render Pipeline.
    /// </summary>
    public class SetAmbientOcclusion : MonoBehaviour
    {
        [SerializeField] private UniversalRendererData rendererData;

        public void SetAmbientOcclusionActive(bool enabled)
        {
            if (!enabled)
            {
                return;
            }
            
            if (rendererData == null)
            {
                Debug.LogError("Renderer data not assigned in the Inspector.");
                return;
            }

            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is ScreenSpaceAmbientOcclusion ssaoFeature)
                {
                    ssaoFeature.SetActive(enabled);
//                    Debug.Log($"SSAO has been {(enabled ? "enabled" : "disabled")}.");
                    return;
                }
            }

            Debug.LogWarning("SSAO Renderer Feature not found in the assigned renderer.");
        }
    }
}