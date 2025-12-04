using UnityEngine;
[SelectionBase]
public class SelectionBase : MonoBehaviour {

#if UNITY_EDITOR
	[Multiline(10)]
	[Tooltip("Editor Only: Write whatever you want here.")]
	[SerializeField] private string notes;
#endif
}
