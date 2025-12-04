namespace RyanMillerGameCore.Dialog
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "RyanMillerGameCore/Dialog/Dialog Content")]
    public class DialogContent : ScriptableObject
    {
        public List<DialogLine> Lines => lines;

        [SerializeField] private List<DialogLine> lines = new List<DialogLine>();
        public bool freezeInputs = true;
        public bool freezeTime = true;
        public bool autoAdvance = false;
        public float delay = 0;
        public float autoAdvanceCharTime = 0.03f;
        public float charRevealRate = 0.02f;
    }

    [System.Serializable]
    public class DialogNavigation
    {
        [Tooltip("Enable to have the speaker move before (or instead of) speaking.")]
        public bool enabled = false;

        [Header("Destination")] [Tooltip("ID target in the world (required when enabled).")]
        public ID targetID;

        [Tooltip("World-space offset applied to the resolved Transform position.")]
        public Vector3 offset;

        [Header("Arrival")]
        [Tooltip(
            "How close is 'arrived' (meters). Used for dialog-side waiting; CharacterPathfind has its own radii too.")]
        public float stopDistance = 0.5f;

        [Header("Facing (Optional)")] [Tooltip("Turn to face this ID after arrival (optional).")]
        public ID faceID;

        [Header("Timeout")]
        [Tooltip("Extra safety timeout (seconds). 0 = rely on CharacterPathfind's own timeouts.")]
        public float timeoutSeconds = 0f;
    }

    [System.Serializable]
    public class DialogLine
    {
        [TextArea(3, 3)] public string text;
        public AudioClip voiceOver;
        public bool centered;
        public ID speaker;
        public ID lookAt;
        public bool focusCameraOnSpeaker = true;
        public float cameraOffsetRotation = 30f;
        public AnimationClip speakerAnimation;
        public AnimationClip lookAtAnimation;
        public Sprite portrait;

        [Header("Navigation (Optional)")]
        [Tooltip(
            "If enabled, the speaker will navigate before the text plays. If 'text' is empty, this becomes a movement-only line.")]
        public DialogNavigation navigation = new DialogNavigation();

        // Convenience
        public bool HasText => !string.IsNullOrEmpty(text);
        public bool HasNavigation => navigation != null && navigation.enabled && targetIDValid;

        private bool targetIDValid => navigation != null && navigation.targetID != null;
    }
}