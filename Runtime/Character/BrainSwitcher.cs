namespace RyanMillerGameCore.Character
{
    using UnityEngine;
    using SMB;

    public class BrainSwitcher : MonoBehaviour
    {
        [SerializeField] private CharacterBrain[] brains;
        [SerializeField] private int currentBrainIndex = 0;
        [SerializeField] private CharacterReferences characterReferences;
        
        private void Start()
        {
            SwitchToBrain(currentBrainIndex);
        }

        public void SwitchToBrain(int index)
        {
            if (index < 0 || index >= brains.Length)
            {
                Debug.LogError("Invalid brain index: " + index);
                return;
            }

            for (int i = 0; i < brains.Length; i++)
            {
                if (i == index)
                {
                    brains[i].enabled = true;
                    characterReferences.characterBrain = brains[i];
                }
                else
                {
                    brains[i].enabled = false;
                }
            }

            currentBrainIndex = index;
        }
    }
}