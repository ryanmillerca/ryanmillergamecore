namespace RyanMillerGameCore.TurnBasedBattle {
    using UnityEngine;

    public static class MoveResolver {
        // Simple deterministic-ish damage formula with small random factor
        public static void Resolve(BattleCommand cmd) {
            if (cmd.Actor == null || cmd.Move == null || cmd.Target == null) {
                return;
            }

            if (!cmd.Actor.isAlive) {
                Debug.Log($"{cmd.Actor.m_CombatantName} is down and cannot act.");
                return;
            }

            if (!cmd.Target.isAlive) {
                Debug.Log($"{cmd.Target.m_CombatantName} is already down. Move wasted.");
                return;
            }

            // Check accuracy
            float roll = Random.value;
            if (roll > cmd.Move.m_Accuracy) {
                Debug.Log($"{cmd.Actor.m_CombatantName} used {cmd.Move.m_MoveName} but missed!");
                return;
            }

            switch (cmd.Move.m_IsHeal) {
                case true when cmd.Move.m_TargetSelf: {
                    int healAmount = Mathf.CeilToInt(cmd.Actor.m_MaxHp * 0.15f) + Mathf.RoundToInt(cmd.Actor.m_Attack * 0.2f);
                    cmd.Actor.Heal(healAmount);
                    Debug.Log($"{cmd.Actor.m_CombatantName} used {cmd.Move.m_MoveName} (self-heal).");
                    return;
                }
                case true: {
                    int healAmount = Mathf.CeilToInt(cmd.Actor.m_MaxHp * 0.15f) + Mathf.RoundToInt(cmd.Actor.m_Attack * 0.2f);
                    cmd.Target.Heal(healAmount);
                    Debug.Log($"{cmd.Actor.m_CombatantName} used {cmd.Move.m_MoveName} to heal {cmd.Target.m_CombatantName}.");
                    return;
                }
            }

            // Damage formula: power * (attacker.attack / target.defense) * random * statMultiplier
            float attack = cmd.Actor.m_Attack * cmd.Move.m_StatMultiplier;
            float defense = Mathf.Max(1, cmd.Target.m_Defense); // avoid div by zero
            float baseDamage = cmd.Move.m_Power * (attack / defense);
            float randomFactor = Random.Range(0.85f, 1.0f);
            int damage = Mathf.Max(1, Mathf.FloorToInt(baseDamage * randomFactor));

            cmd.Target.TakeDamage(damage);
            Debug.Log($"{cmd.Actor.m_CombatantName} used {cmd.Move.m_MoveName} on {cmd.Target.m_CombatantName} for {damage} damage.");
        }
    }
}