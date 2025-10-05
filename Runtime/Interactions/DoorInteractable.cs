namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;
    using Character;
    
    public class Door : Interactive
    {
        public override void Interact(Character character)
        {
            Debug.Log("Door opened by " + character);
        }
    }
}