using System.Collections.Generic;
using UnityEngine;

namespace RyanMillerGameCore.TurnBasedCombat
{
    public static class MoveResolver
    {
        private const float BASE_CRIT_CHANCE = 0.05f;
        private const float CRIT_MULTIPLIER = 1.5f;
        private const float PLAYER_CRIT_MODIFIER = 1.0f;
        private const float ENEMY_CRIT_MODIFIER = 1.0f;

        public static List<BattleResult> Resolve(BattleCommand cmd, List<Combatant> allCombatants = null, bool isChargedAction = false, float chargeMultiplier = 1.0f)
        {
            var results = new List<BattleResult>();

            if (!cmd.Actor || !cmd.BattleAction)
            {
                results.Add(new BattleResult
                {
                    Message = "Invalid command: missing Actor or Action.",
                    Success = false
                });
                return results;
            }

            if (!cmd.Actor.isAlive)
            {
                results.Add(new BattleResult
                {
                    Message = $"{cmd.Actor.m_CombatantName} is down and cannot act.",
                    Success = false
                });
                return results;
            }

            if (cmd.BattleAction.m_ActionType == ActionType.Defend)
            {
                var result = new BattleResult
                {
                    Actor = cmd.Actor,
                    Target = cmd.Actor,
                    BattleAction = cmd.BattleAction,
                    Success = true
                };

                cmd.Actor.StartDefend(cmd.BattleAction);
                result.Message = $"{cmd.Actor.m_CombatantName} defends!";
                results.Add(result);
                return results;
            }

            if (!cmd.Target)
            {
                results.Add(new BattleResult
                {
                    Actor = cmd.Actor,
                    BattleAction = cmd.BattleAction,
                    Message = $"{cmd.Actor.m_CombatantName}'s {cmd.BattleAction.m_ActionName} has no valid target!",
                    Success = false
                });
                return results;
            }

            if (!cmd.Target.isAlive)
            {
                results.Add(new BattleResult
                {
                    Message = $"{cmd.Target.m_CombatantName} is already down. Action wasted.",
                    Success = false
                });
                return results;
            }

            if (Random.value > cmd.BattleAction.m_Accuracy)
            {
                results.Add(new BattleResult
                {
                    Actor = cmd.Actor,
                    Target = cmd.Target,
                    BattleAction = cmd.BattleAction,
                    Missed = true,
                    Success = false,
                    Message = $"{cmd.Actor.m_CombatantName} used {cmd.BattleAction.m_ActionName} but missed!"
                });
                return results;
            }

            if (cmd.Target != null && cmd.Target.lastAttacker == cmd.Actor && cmd.Target.isDefending)
            {
                var counterResult = ResolveCounterAttack(cmd.Target, cmd.Actor);
                if (counterResult != null)
                {
                    results.Add(counterResult);
                }
                cmd.Target.lastAttacker = null;
            }

            var targets = GetTargets(cmd, allCombatants);

            foreach (var target in targets)
            {
                var result = new BattleResult
                {
                    Actor = cmd.Actor,
                    Target = target,
                    BattleAction = cmd.BattleAction,
                    Success = true,
                    IsChargedAction = isChargedAction,
                    ChargeMultiplier = chargeMultiplier
                };

                switch (cmd.BattleAction.m_ActionType)
                {
                    case ActionType.Heal:
                        int healAmount = Mathf.CeilToInt(cmd.Actor.m_MaxHp * 0.15f) +
                                         Mathf.RoundToInt(cmd.Actor.m_Attack * 0.2f);

                        if (isChargedAction)
                        {
                            healAmount = Mathf.RoundToInt(healAmount * chargeMultiplier);
                        }

                        target.Heal(healAmount);
                        result.HealingDone = healAmount;

                        string healChargeText = isChargedAction ? " (FULLY CHARGED!)" : "";
                        result.Message = cmd.BattleAction.m_TargetSelf
                            ? $"{cmd.Actor.m_CombatantName} used {cmd.BattleAction.m_ActionName} to heal self.{healChargeText}"
                            : $"{cmd.Actor.m_CombatantName} used {cmd.BattleAction.m_ActionName} to heal {target.m_CombatantName}.{healChargeText}";
                        break;

                    case ActionType.Damage:
                        result = CalculateDamage(cmd, target, result, isChargedAction, chargeMultiplier);
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
                        result.Message = $"{cmd.Actor.m_CombatantName} used item {cmd.BattleAction.m_ActionName}.";
                        break;
                }

                results.Add(result);
            }

            return results;
        }

        private static BattleResult ResolveCounterAttack(Combatant defender, Combatant attacker)
        {
            if (!defender.isAlive || !attacker.isAlive)
                return null;

            var counterAction = ScriptableObject.CreateInstance<BattleAction>();
            counterAction.m_ActionName = "Counter Attack";
            counterAction.m_ActionType = ActionType.Damage;
            counterAction.m_Power = Mathf.RoundToInt(defender.m_Attack * defender.counterAttackMultiplier);
            counterAction.m_Accuracy = 1f;

            var counterCmd = new BattleCommand(defender, counterAction, attacker);
            var counterResult = CalculateDamage(counterCmd, attacker, new BattleResult
            {
                Actor = defender,
                Target = attacker,
                BattleAction = counterAction,
                Success = true
            }, false, 1f);

            counterResult.Message = $"{defender.m_CombatantName} counter attacks {attacker.m_CombatantName} for {counterResult.DamageDealt} damage!";

            return counterResult;
        }

        private static BattleResult CalculateDamage(BattleCommand cmd, Combatant target, BattleResult result, bool isChargedAction, float chargeMultiplier)
        {
            float attack = cmd.Actor.m_Attack * cmd.BattleAction.m_StatMultiplier;
            float defense = Mathf.Max(1, target.m_Defense);
            float baseDamage = cmd.BattleAction.m_Power * (attack / defense);
            float randomFactor = Random.Range(0.85f, 1.0f);
            int damage = Mathf.Max(1, Mathf.FloorToInt(baseDamage * randomFactor));

            if (isChargedAction)
            {
                damage = Mathf.RoundToInt(damage * chargeMultiplier);
            }

            float critChance = CalculateCriticalHitChance(cmd.Actor, cmd.BattleAction);
            bool isCritical = Random.value < critChance;

            if (isCritical)
            {
                damage = Mathf.FloorToInt(damage * CRIT_MULTIPLIER);
                result.CriticalHit = true;
                result.CriticalChance = critChance;
            }

            target.TakeDamage(damage, cmd.Actor);
            result.DamageDealt = damage;

            string critText = isCritical ? " (CRITICAL HIT!)" : "";
            string chargeText = isChargedAction ? " (FULLY CHARGED!)" : "";
            result.Message = $"{cmd.Actor.m_CombatantName} used {cmd.BattleAction.m_ActionName} on {target.m_CombatantName}{critText}{chargeText} for {damage} damage.";

            return result;
        }

        private static float CalculateCriticalHitChance(Combatant attacker, BattleAction action)
        {
            float critChance = BASE_CRIT_CHANCE;
            critChance += action.m_CritChance;

            if (attacker.m_Team == Team.Player)
            {
                critChance *= PLAYER_CRIT_MODIFIER;
            }
            else
            {
                critChance *= ENEMY_CRIT_MODIFIER;
            }

            return Mathf.Clamp01(critChance);
        }

        private static List<Combatant> GetTargets(BattleCommand cmd, List<Combatant> allCombatants)
        {
            if (cmd.BattleAction.m_TargetSelf) return new List<Combatant> { cmd.Actor };

            if (allCombatants == null) return new List<Combatant> { cmd.Target };

            switch (cmd.BattleAction.m_TargetType)
            {
                case ActionTargetType.Self:
                    return new List<Combatant> { cmd.Actor };

                case ActionTargetType.SingleEnemy:
                    return new List<Combatant> { cmd.Target }.FindAll(t => t.m_Team != cmd.Actor.m_Team && t.isAlive);

                case ActionTargetType.AllEnemies:
                    return allCombatants.FindAll(c => c.isAlive && c.m_Team != cmd.Actor.m_Team);

                case ActionTargetType.SingleAlly:
                    return new List<Combatant> { cmd.Target }.FindAll(t => t.m_Team == cmd.Actor.m_Team && t.isAlive);

                case ActionTargetType.AllAllies:
                    return allCombatants.FindAll(c => c.isAlive && c.m_Team == cmd.Actor.m_Team && c != cmd.Actor);

                default:
                    return new List<Combatant> { cmd.Target };
            }
        }

        private static void ApplyBuff(Combatant target, BattleAction action)
        {
            if (action.m_AttackModifier != 0) target.m_Attack += Mathf.RoundToInt(target.m_Attack * action.m_AttackModifier);
            if (action.m_DefenseModifier != 0) target.m_Defense += Mathf.RoundToInt(target.m_Defense * action.m_DefenseModifier);
            if (action.m_SpeedModifier != 0) target.m_Speed += Mathf.RoundToInt(target.m_Speed * action.m_SpeedModifier);
        }

        private static void ApplyDebuff(Combatant target, BattleAction action)
        {
            if (action.m_AttackModifier != 0) target.m_Attack -= Mathf.RoundToInt(target.m_Attack * action.m_AttackModifier);
            if (action.m_DefenseModifier != 0) target.m_Defense -= Mathf.RoundToInt(target.m_Defense * action.m_DefenseModifier);
            if (action.m_SpeedModifier != 0) target.m_Speed -= Mathf.RoundToInt(target.m_Speed * action.m_SpeedModifier);
        }
    }
}