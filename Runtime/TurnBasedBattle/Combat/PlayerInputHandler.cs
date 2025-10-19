using System.Linq;
using UnityEngine;
using RyanMillerGameCore.TurnBasedCombat.UI;

namespace RyanMillerGameCore.TurnBasedCombat
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("References")]
        public BattleManager battleManager;
        public UIBattleMenu uiBattleMenu; // You'll create this

        private void OnEnable()
        {
            if (battleManager != null)
            {
                battleManager.PlayerInputRequired += OnPlayerInputRequired;
                battleManager.PlayerInputReceived += OnPlayerInputReceived;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.PlayerInputRequired -= OnPlayerInputRequired;
                battleManager.PlayerInputReceived -= OnPlayerInputReceived;
            }
        }

        private void OnPlayerInputRequired(PlayerInputData inputData)
        {
            Debug.Log($"Waiting on Player input for {inputData.Actor.m_CombatantName}");
            Debug.Log($"Available moves: {string.Join(", ", inputData.AvailableMoves.Select(m => m.m_ActionName))}");
            Debug.Log($"Valid targets: {string.Join(", ", inputData.ValidTargets.Select(t => t.m_CombatantName))}");

            // Here you would:
            // 1. Show your battle UI menu
            // 2. Populate it with available moves and targets
            // 3. Wait for player selection
            // 4. Call SubmitPlayerInput when ready

            // Example of how you might handle this:
            // uiBattleMenu.Show(inputData, OnPlayerSelection);
            
            // For now, we'll simulate a player selection after a delay
            StartCoroutine(SimulatePlayerInput(inputData));
        }

        private System.Collections.IEnumerator SimulatePlayerInput(PlayerInputData inputData)
        {
            // Simulate player thinking time
            yield return new WaitForSeconds(1.5f);

            // Create a simulated player response
            var response = new PlayerInputResponse
            {
                SelectedAction = inputData.AvailableMoves[0], // First move
                SelectedTarget = inputData.ValidTargets[0],   // First target
                IsValid = true,
                ValidationMessage = "Valid selection"
            };

            // Submit the response to the battle manager
            battleManager.SubmitPlayerInput(response);
        }

        private void OnPlayerInputReceived(PlayerInputResponse response)
        {
            Debug.Log($"🎮 Player input received: {response.SelectedAction.m_ActionName} on {response.SelectedTarget.m_CombatantName}");
        }

        // Public methods for your UI to call
        public void SubmitActionSelection(BattleAction action, Combatant target)
        {
            var response = new PlayerInputResponse
            {
                SelectedAction = action,
                SelectedTarget = target,
                IsValid = true,
                ValidationMessage = "Valid selection"
            };

            battleManager.SubmitPlayerInput(response);
        }

        public void CancelActionSelection()
        {
            battleManager.CancelPlayerInput();
        }
    }
}