namespace RyanMillerGameCore.TurnBasedCombat
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class MoveResolver
    {
        public static List<BattleResult> Resolve(BattleCommand cmd, List<Combatant> allCombatants = null)
        {
            var results = new List<BattleResult>();

            if (!cmd.Actor || !cmd.BattleAction || !cmd.Target)
            {
                results.Add(new BattleResult { Message = "Invalid command." });
                return results;
            }

            if (!cmd.Actor.isAlive)
            {
                results.Add(new BattleResult { Message = $"{cmd.Actor.m_CombatantName} is down and cannot act." });
                return results;
            }

            if (!cmd.Target.isAlive)
            {
                results.Add(new BattleResult { Message = $"{cmd.Target.m_CombatantName} is already down. Action wasted." });
                return results;
            }

            // Accuracy check
            if (Random.value > cmd.BattleAction.m_Accuracy)
            {
                results.Add(new BattleResult
                {
                    Actor = cmd.Actor,
                    Target = cmd.Target,
                    BattleAction = cmd.BattleAction,
                    Missed = true,
                    Message = $"{cmd.Actor.m_CombatantName} used {cmd.BattleAction.m_ActionName} but missed!"
                });
                return results;
            }

            // Determine targets based on ActionTargetType
            var targets = GetTargets(cmd, allCombatants);

            foreach (var target in targets)
            {
                var result = new BattleResult
                {
                    Actor = cmd.Actor,
                    Target = target,
                    BattleAction = cmd.BattleAction
                };

                switch (cmd.BattleAction.m_ActionType)
                {
                    case ActionType.Heal:
                        int healAmount = Mathf.CeilToInt(cmd.Actor.m_MaxHp * 0.15f) +
                                         Mathf.RoundToInt(cmd.Actor.m_Attack * 0.2f);
                        target.Heal(healAmount);
                        result.HealingDone = healAmount;
                        result.Message = cmd.BattleAction.m_TargetSelf
                            ? $"{cmd.Actor.m_CombatantName} used {cmd.BattleAction.m_ActionName} to heal self."
                            : $"{cmd.Actor.m_CombatantName} used {cmd.BattleAction.m_ActionName} to heal {target.m_CombatantName}.";
                        break;

                    case ActionType.Damage:
                        float attack = cmd.Actor.m_Attack * cmd.BattleAction.m_StatMultiplier;
                        float defense = Mathf.Max(1, target.m_Defense);
                        float baseDamage = cmd.BattleAction.m_Power * (attack / defense);
                        float randomFactor = Random.Range(0.85f, 1.0f);
                        int damage = Mathf.Max(1, Mathf.FloorToInt(baseDamage * randomFactor));

                        // Critical hit
                        bool crit = Random.value < cmd.BattleAction.m_CritChance;
                        if (crit)
                        {
                            damage = Mathf.FloorToInt(damage * 1.5f);
                            result.CriticalHit = true;
                        }

                        target.TakeDamage(damage);
                        result.DamageDealt = damage;
                        result.Message = $"{cmd.Actor.m_CombatantName} used {cmd.BattleAction.m_ActionName} on {target.m_CombatantName}" +
                                         (crit ? " (CRITICAL HIT!)" : "") +
                                         $" for {damage} damage.";
                        break;

                    case ActionType.Buff:
                        ApplyBuff(target, cmd.BattleAction);
                        result.Message = $"{cmd.Actor.m_CombatantName} buffed {target.m_CombatantName} with {cmd.BattleAction.m_ActionName}.";
                        break;

                    case ActionType.Debuff:
                        ApplyDebuff(target, cmd.BattleAction);
                        result.Message = $"{cmd.Actor.m_CombatantName} debuffed {target.m_CombatantName} with {cmd.BattleAction.m_ActionName}.";
                        break;

                    case ActionType.Item:
                        // Placeholder: handle items if needed
                        result.Message = $"{cmd.Actor.m_CombatantName} used item {cmd.BattleAction.m_ActionName}.";
                        break;
                }

                results.Add(result);
            }

            return results;
        }

        private static List<Combatant> GetTargets(BattleCommand cmd, List<Combatant> allCombatants)
        {
            if (cmd.BattleAction.m_TargetSelf) return new List<Combatant> { cmd.Actor };

            if (allCombatants == null) return new List<Combatant> { cmd.Target };

            switch (cmd.BattleAction.m_TargetType)
            {
                case ActionTargetType.Self: return new List<Combatant> { cmd.Actor };
                case ActionTargetType.SingleEnemy: return new List<Combatant> { cmd.Target };
                case ActionTargetType.AllEnemies: return allCombatants.FindAll(c => c.isAlive && c != cmd.Actor); // simple enemy/allies logic
                case ActionTargetType.SingleAlly: return new List<Combatant> { cmd.Target };
                case ActionTargetType.AllAllies: return allCombatants.FindAll(c => c.isAlive && c != cmd.Target); // tweak based on your team/allies
                default: return new List<Combatant> { cmd.Target };
            }
        }

        private static void ApplyBuff(Combatant target, BattleAction action)
        {
            if (action.m_AttackModifier != 0) target.m_Attack += Mathf.RoundToInt(target.m_Attack * action.m_AttackModifier);
            if (action.m_DefenseModifier != 0) target.m_Defense += Mathf.RoundToInt(target.m_Defense * action.m_DefenseModifier);
            if (action.m_SpeedModifier != 0) target.m_Speed += Mathf.RoundToInt(target.m_Speed * action.m_SpeedModifier);
            // Duration tracking would be handled by another system
        }

        private static void ApplyDebuff(Combatant target, BattleAction action)
        {
            if (action.m_AttackModifier != 0) target.m_Attack -= Mathf.RoundToInt(target.m_Attack * action.m_AttackModifier);
            if (action.m_DefenseModifier != 0) target.m_Defense -= Mathf.RoundToInt(target.m_Defense * action.m_DefenseModifier);
            if (action.m_SpeedModifier != 0) target.m_Speed -= Mathf.RoundToInt(target.m_Speed * action.m_SpeedModifier);
            // Duration tracking would be handled by another system
        }
    }
}