namespace RyanMillerGameCore.TurnBasedBattle {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BattleManager : MonoBehaviour {
        [SerializeField] private Combatant m_Player;
        [SerializeField] private Combatant m_Enemy;
        [SerializeField] private bool m_AutoChoosePlayerMove = true;
        [SerializeField] private int m_PlayerMoveIndex = 0;

        private void Start() {
            if (m_Player == null || m_Enemy == null) {
                Debug.LogError("Assign player and enemy Combatant prefabs in the inspector.");
                return;
            }

            StartCoroutine(BattleLoop());
        }

        private IEnumerator BattleLoop() {
            Debug.Log("Battle start!");
            while (m_Player.isAlive && m_Enemy.isAlive) {
                // Choose commands
                BattleCommand playerCmd = ChoosePlayerCommand();
                BattleCommand enemyCmd = ChooseEnemyCommand();

                // Order by speed (higher speed acts first). Ties: random.
                List<BattleCommand> ordered = new List<BattleCommand> { playerCmd, enemyCmd };
                ordered.Sort((a, b) => {
                    int sa = a.Actor != null ? a.Actor.m_Speed : 0;
                    int sb = b.Actor != null ? b.Actor.m_Speed : 0;
                    if (sa == sb) return Random.value > 0.5f ? 1 : -1;
                    return sb.CompareTo(sa); // descending: higher speed first
                });

                // Resolve sequentially
                foreach (var cmd in ordered) {
                    if (m_Player.isAlive == false || m_Enemy.isAlive == false)
                        break;

                    MoveResolver.Resolve(cmd);
                    yield return new WaitForSeconds(0.6f);
                }

                yield return new WaitForSeconds(0.2f);
            }

            Debug.Log(m_Player.isAlive ? "Player wins!" : "Enemy wins!");
        }

        BattleCommand ChoosePlayerCommand() {
            if (m_Player == null) return null;
            if (m_Player.m_Moves == null || m_Player.m_Moves.Count == 0)
                return null;

            MoveDefSo chosen;
            if (m_AutoChoosePlayerMove) {
                chosen = m_Player.m_Moves[Mathf.Clamp(0, 0, m_Player.m_Moves.Count - 1)];
            }
            else {
                int idx = Mathf.Clamp(m_PlayerMoveIndex, 0, m_Player.m_Moves.Count - 1);
                chosen = m_Player.m_Moves[idx];
            }

            Combatant target = chosen.m_TargetSelf ? m_Player : m_Enemy;
            return new BattleCommand(m_Player, chosen, target);
        }

        BattleCommand ChooseEnemyCommand() {
            if (m_Enemy == null) return null;
            if (m_Enemy.m_Moves == null || m_Enemy.m_Moves.Count == 0)
                return null;

            // Very simple AI: prefer damaging moves, pick random move
            MoveDefSo chosen = m_Enemy.m_Moves[Random.Range(0, m_Enemy.m_Moves.Count)];
            Combatant target = chosen.m_TargetSelf ? m_Enemy : m_Player;
            return new BattleCommand(m_Enemy, chosen, target);
        }
    }
}