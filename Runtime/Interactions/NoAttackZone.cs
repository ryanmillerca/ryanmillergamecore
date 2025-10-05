namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;
    using Character;

    public class NoAttackZone : ColliderSensor
    {
        protected override void ItemEnteredTrigger(Collider item)
        {
            base.ItemEnteredTrigger(item);
            var characterObject = item.transform.root.gameObject;
            CharacterBrain character = characterObject.GetComponent<CharacterBrain>();
            if (character)
            {
                character.AskToSetAttackEnabled(gameObject.GetInstanceID(), false);
            }
        }
        
        protected override void ItemExitedTrigger(Collider item)
        {
            base.ItemExitedTrigger(item);
            var characterObject = item.transform.root.gameObject;
            CharacterBrain character = characterObject.GetComponent<CharacterBrain>();
            if (character)
            {
                character.AskToSetAttackEnabled(gameObject.GetInstanceID(), true);
            }
        }
    }
}