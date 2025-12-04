namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;
    using Character;

    public class NoAttackZone : ColliderSensor
    {
        protected override void ItemEnteredTrigger(Collider item)
        {
            base.ItemEnteredTrigger(item);
            Character character = TryGetCharacterFromCollider(item);
            if (character) {
                CharacterBrain characterBrain = character.Brain;
                if (characterBrain) {
                    characterBrain.AskToSetAttackEnabled(gameObject.GetInstanceID(), false);
                }
            }
        }
        
        protected override void ItemExitedTrigger(Collider item)
        {
            base.ItemExitedTrigger(item);
            Character character = TryGetCharacterFromCollider(item);
            if (character) {
                CharacterBrain characterBrain = character.Brain;
                characterBrain.AskToSetAttackEnabled(gameObject.GetInstanceID(), true);
            }
        }
    }
}