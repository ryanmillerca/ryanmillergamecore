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
        private static readonly Dictionary<Animator, ICharacterReferenceProvider> CharacterRefs = new Dictionary<Animator, ICharacterReferenceProvider>();

        public static void SetComponentReference(this Animator animator, ICharacterReferenceProvider referenceProvider)
        {
            CharacterRefs[animator] = referenceProvider;
        }

        public static ICharacterReferenceProvider GetComponentReference(this Animator animator)
        {
            if (animator == null) {
                return null;
            }
            CharacterRefs.TryGetValue(animator, out ICharacterReferenceProvider reference);
            return reference;
        }

        public static void ClearComponentReference(this Animator animator)
        {
            CharacterRefs.Remove(animator);
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

