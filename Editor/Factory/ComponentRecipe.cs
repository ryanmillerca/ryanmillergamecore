using UnityEngine;
using System;

namespace RyanMillerGameCore.Factory
{
	[CreateAssetMenu(fileName = "ComponentRecipe", menuName = "RyanMillerGameCore/Factory/Component Recipe")]
	public class ComponentRecipe : ScriptableObject
	{
		[Tooltip("Components (built-in or custom) to add.")]
		public ComponentEntry[] components;

		[Tooltip("Optional name for the generated GameObject.")]
		public string defaultName = "New GameObject";

		[Tooltip("Child recipes to create under this GameObject.")]
		public ComponentRecipe[] children;

		public GameObject CreateInstanceInScene(Transform parent = null)
		{
			GameObject go = new GameObject(string.IsNullOrEmpty(defaultName) ? "New GameObject" : defaultName);

			if (parent != null)
				go.transform.SetParent(parent, worldPositionStays: false);

			// Add components
			if (components != null)
			{
				foreach (var entry in components)
				{
					if (entry == null || !entry.IsValid()) continue;

					Type t = entry.GetTypeSafe();
					Component c = go.GetComponent(t) ?? go.AddComponent(t);

					// Apply property overrides
					entry.ApplyProperties(c);
				}
			}

			// Recursively create children
			if (children != null)
			{
				foreach (var child in children)
				{
					if (child != null)
						child.CreateInstanceInScene(go.transform);
				}
			}

			return go;
		}
	}
}
