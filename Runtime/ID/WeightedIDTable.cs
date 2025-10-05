namespace RyanMillerGameCore
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "RyanMillerGameCore/ID/Weighted ID Table", fileName = "NewWeightedIDTable")]
    public class WeightedIDTable : ScriptableObject
    {
        public List<WeightedIDEntry> entries = new List<WeightedIDEntry>();

        public ID GetRandomID()
        {
            if (entries == null || entries.Count == 0)
            {
                return null;
            }

            // Calculate total
            float totalWeight = 0f;
            foreach (var entry in entries)
            {
                totalWeight += entry.weight;
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            // Normalize and roll
            float roll = Random.value;
            float cumulative = 0f;

            foreach (var entry in entries)
            {
                float normalized = entry.weight / totalWeight;
                cumulative += normalized;
                if (roll <= cumulative)
                {
                    return entry.id;
                }
            }

            // Fallback
            return entries[^1].id;
        }
    }
    
    [System.Serializable]
    public class WeightedIDEntry
    {
        public ID id;
        [Range(0f, 100f)]
        public float weight = 1f;
    }
}