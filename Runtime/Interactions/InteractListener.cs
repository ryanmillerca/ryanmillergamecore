namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;
    using Character;
    
    public class InteractListener : MonoBehaviour
    {
        private CharacterBrain _characterBrain;
        private Character _character;
        private InteractiveObjectColliderSensor _interactiveObjectColliderSensor;
        
        private void OnEnable()
        {
            if (_interactiveObjectColliderSensor == null)
            {
                _interactiveObjectColliderSensor = transform.root.GetComponentInChildren<InteractiveObjectColliderSensor>();
            }
            if (_characterBrain == null)
            {
                _characterBrain = transform.GetComponentInParent<CharacterBrain>();
            }
            if (_character == null)
            {
                _character = transform.GetComponentInParent<Character>();
            }
            _characterBrain.OnInteractAction += InteractAction;
        }

        private void OnDisable()
        {
            _characterBrain.OnInteractAction -= InteractAction;
        }

        private void InteractAction()
        {
            Interactive interactive = _interactiveObjectColliderSensor.CurrentInteractive;
            if (interactive)
            {
                interactive.Interact(_character);
            }
        }
    }
}