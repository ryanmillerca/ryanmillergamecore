namespace RyanMillerGameCore.UI
{
    using UnityEngine;
    
    [CreateAssetMenu(menuName = "RyanMillerGameCore/Input/Prompt Graphics")]
    public class PromptGraphics : ScriptableObject
    {
        public Sprite move;
        public Sprite look;
        public Sprite attack;
        public Sprite interact;
        public Sprite dodge;
        public Sprite special;
        public Sprite pause;
    }
}