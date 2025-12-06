namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;
    using Character;
    
    public class Door : Interactive
    {
        public override void Interact(ICharacter character)
        {
            Debug.Log("Door opened by " + character);
        }
    }
}