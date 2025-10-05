namespace RyanMillerGameCore.Performance
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Sets all Lights to cast shadows or not.
    /// </summary>
    public class SetRealtimeCastShadows : MonoBehaviour
    {
        // Stores lights and their original shadow settings
        private readonly Dictionary<Light, LightShadows> _modifiedLights = new();

        [ContextMenu("Set All Shadows Enabled")]
        public void SetAllEnabled()
        {
            SetAllShadowsEnabled(true);
        }

        [ContextMenu("Set All Shadows Disabled")]
        public void SetAllDisabled()
        {
            SetAllShadowsEnabled(false);
        }

        public void SetAllShadowsEnabled(bool enabled)
        {
            if (!enabled)
            {
                return;
            }
            
            Light[] allLights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            if (!enabled)
            {
                foreach (Light eachLight in allLights)
                {
                    if (eachLight != null && eachLight.shadows != LightShadows.None && !_modifiedLights.ContainsKey(eachLight))
                    {
                        _modifiedLights[eachLight] = eachLight.shadows;
                        eachLight.shadows = LightShadows.None;
                    }
                }
            }
            else
            {
                int restored = 0;
                var lightsToRemove = new List<Light>();

                foreach (var kvp in _modifiedLights)
                {
                    Light light = kvp.Key;
                    LightShadows originalShadows = kvp.Value;

                    if (light != null)
                    {
                        light.shadows = originalShadows;
                        restored++;
                    }
                    lightsToRemove.Add(light);
                }

                foreach (var light in lightsToRemove)
                {
                    _modifiedLights.Remove(light);
                }
            }
        }
    }
}