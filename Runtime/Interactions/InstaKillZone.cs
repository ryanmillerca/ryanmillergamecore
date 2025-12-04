namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;
    using Utilities;
    using Character;
    
    public class InstaKillZone : ColliderSensor
    {
        protected override void ItemEnteredTrigger(Collider item) {
            Character character = TryGetCharacterFromCollider(item);
            if (character)
            {
                character.ReceiveDamage(float.MaxValue);
            }
        }
    }
}