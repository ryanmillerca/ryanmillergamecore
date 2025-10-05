namespace RyanMillerGameCore.Character.SMB
{
    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Animations;
    #endif 
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// This class is used to store references to the character's components in a dictionary.
    /// </summary>
    public static class AnimatorExtensions
    {
        private static readonly Dictionary<Animator, CharacterReferences> _characterRefs = new();

        public static void SetComponentReference(this Animator animator, CharacterReferences references)
        {
            _characterRefs[animator] = references;
        }

        public static CharacterReferences GetComponentReference(this Animator animator)
        {
            _characterRefs.TryGetValue(animator, out var reference);
            return reference;
        }

        public static void ClearComponentReference(this Animator animator)
        {
            _characterRefs.Remove(animator);
        }
        
        #if UNITY_EDITOR
        public static Animator FindAnimatorUsingSerializedObject(SerializedObject serializedObject)
        {
            if (serializedObject?.targetObject is not StateMachineBehaviour smb)
                return null;

            var animators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);

            foreach (var animator in animators)
            {
                var controller = animator.runtimeAnimatorController as AnimatorController;
                if (controller == null)
                    continue;

                foreach (var layer in controller.layers)
                {
                    var stateMachine = layer.stateMachine;
                    if (ContainsBehaviourInstance(stateMachine, smb))
                        return animator;
                }
            }

            return null;
        }
        
        private static bool ContainsBehaviourInstance(AnimatorStateMachine stateMachine, StateMachineBehaviour target)
        {
            foreach (var state in stateMachine.states)
            {
                foreach (var behaviour in state.state.behaviours)
                {
                    if (behaviour == target)
                        return true;
                }
            }

            foreach (var subStateMachine in stateMachine.stateMachines)
            {
                if (ContainsBehaviourInstance(subStateMachine.stateMachine, target))
                    return true;
            }

            return false;
        }
        #endif
    }
}

