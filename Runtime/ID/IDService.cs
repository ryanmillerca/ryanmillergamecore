namespace RyanMillerGameCore
{
    using UnityEngine;
    using System.Collections.Generic;

    public class IDService : Singleton<IDService>
    {
        private readonly Dictionary<ID, GameObject> idEntries = new();

        public GameObject GetGameObjectWithID(ID id)
        {
            if (!id)
            {
                return null;
            }

            idEntries.TryGetValue(id, out var go);
            return go;
        }
        
        public Transform GetTransformWithID(ID id)
        {
            if (!id)
            {
                return null;
            }

            idEntries.TryGetValue(id, out var go);
            return go.transform;
        }

        public void AddGameObjectWithID(IHasID hasIDObject)
        {
            if (hasIDObject == null)
            {
                return;
            }

            var mono = hasIDObject as MonoBehaviour;
            if (mono == null)
            {
                return;
            }

            ID newID = hasIDObject.GetID();
            if (newID == null)
            {
                return;
            }

            if (!idEntries.ContainsKey(newID))
            {
                idEntries[newID] = mono.gameObject;
            }
        }

        public void RemoveGameObjectWithID(IHasID hasIDObject)
        {
            if (hasIDObject == null)
            {
                return;
            }

            ID id = hasIDObject.GetID();
            if (id == null)
            {
                return;
            }

            idEntries.Remove(id);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Print Registered IDs")]
        private void DebugPrintRegisteredIDs()
        {
            if (idEntries == null || idEntries.Count == 0)
            {
                Debug.Log("[IDService] No registered IDs.");
                return;
            }

            Debug.Log($"[IDService] {idEntries.Count} registered entries:");
            foreach (var kvp in idEntries)
            {
                string idName = kvp.Key != null
                    ? (!string.IsNullOrEmpty(kvp.Key.prettyName) ? kvp.Key.prettyName : kvp.Key.name)
                    : "(null ID)";

                string goName = kvp.Value != null ? kvp.Value.name : "(null GameObject)";
                Debug.Log($"    ID: {idName} → GameObject: {goName}");
            }
        }
#endif
    }
}