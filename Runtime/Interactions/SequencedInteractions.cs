namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;

    public class SequencedInteractions : MonoBehaviour
    {
        [SerializeField] private Interactive[] interactives;
        [SerializeField] private int currentInteractiveIndex = 0;

        private void Start()
        {
            if (interactives.Length == 0)
            {
                interactives = GetComponentsInChildren<Interactive>();
            }
            SetCurrentInteraction(currentInteractiveIndex);
        }

        private void SetCurrentInteraction(int index)
        {
            if (index >= interactives.Length)
            {
                this.enabled = false;
                return;
            }

            currentInteractiveIndex = index;
            for (int i = 0; i < interactives.Length; i++)
            {
                if (i == currentInteractiveIndex)
                {
                    interactives[i].gameObject.SetActive(true);
                }
                else
                {
                    interactives[i].gameObject.SetActive(false);
                }
            }
            interactives[currentInteractiveIndex].InteractionComplete += OnInteractionComplete;
        }

        private void OnInteractionComplete()
        {
            SetCurrentInteraction(currentInteractiveIndex + 1);
        }

        private void OnDisable()
        {
            interactives[currentInteractiveIndex].InteractionComplete -= OnInteractionComplete;
        }
    }
}