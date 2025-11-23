using System.Collections.Generic;
using UnityEngine;

namespace RyanMillerGameCore.TurnBasedCombat {
	public static class MoveResolver {
		private const float BASE_CRIT_CHANCE = 0.05f;
		private const float CRIT_MULTIPLIER = 1.5f;
		private const float PLAYER_CRIT_MODIFIER = 1.0f;
		private const float ENEMY_CRIT_MODIFIER = 1.0f;

		public static List<BattleResult> Resolve(BattleCommand cmd, List<Combatant> allCombatants = null, bool isChargedAction = false, float chargeMultiplier = 1.0f) {
			var results = new List<BattleResult>();

			if (!cmd.Actor || !cmd.BattleAction) {
				results.Add(new BattleResult {
					Message = "Invalid command: missing Actor or Action.",
					Success = false
				});
				return results;
			}

			if (!cmd.Actor.isAlive) {
				results.Add(new BattleResult {
					Message = $"{cmd.Actor.CombatantName} is down and cannot act.",
					Success = false
				});
				return results;
			}

			if (cmd.BattleAction.ActionType == ActionType.Defend) {
				var result = new BattleResult {
					Actor = cmd.Actor,
					Target = cmd.Actor,
					BattleAction = cmd.BattleAction,
					Success = true
				};

				cmd.Actor.StartDefend(cmd.BattleAction);
				result.Message = $"{cmd.Actor.CombatantName} defends!";
				results.Add(result);
				return results;
			}

			if (!cmd.Target) {
				results.Add(new BattleResult {
					Actor = cmd.Actor,
					BattleAction = cmd.BattleAction,
					Message = $"{cmd.Actor.CombatantName}'s {cmd.BattleAction.ActionName} has no valid target!",
					Success = false
				});
				return results;
			}

			if (!cmd.Target.isAlive) {
				results.Add(new BattleResult {
					Message = $"{cmd.Target.CombatantName} is already down. Action wasted.",
					Success = false
				});
				return results;
			}

			if (Random.value > cmd.BattleAction.Accuracy) {
				results.Add(new BattleResult {
					Actor = cmd.Actor,
					Target = cmd.Target,
					BattleAction = cmd.BattleAction,
					Missed = true,
					Success = false,
					Message = $"{cmd.Actor.CombatantName} used {cmd.BattleAction.ActionName} but missed!"
				});
				return results;
			}

			if (cmd.Target != null && cmd.Target.LastAttacker == cmd.Actor && cmd.Target.IsDefending) {
				var counterResult = ResolveCounterAttack(cmd.Target, cmd.Actor);
				if (counterResult != null) {
					results.Add(counterResult);
				}
				cmd.Target.LastAttacker = null;
			}

			var targets = GetTargets(cmd, allCombatants);

			foreach (var target in targets) {
				var result = new BattleResult {
					Actor = cmd.Actor,
					Target = target,
					BattleAction = cmd.BattleAction,
					Success = true,
					IsChargedAction = isChargedAction,
					ChargeMultiplier = chargeMultiplier
				};

				switch (cmd.BattleAction.ActionType) {
					case ActionType.Heal:
						int healAmount = Mathf.CeilToInt(cmd.Actor.MaxHp * 0.15f) +
						                 Mathf.RoundToInt(cmd.Actor.Attack * 0.2f);

						if (isChargedAction) {
							healAmount = Mathf.RoundToInt(healAmount * chargeMultiplier);
						}

						target.Heal(healAmount);
						result.HealingDone = healAmount;

						string healChargeText = isChargedAction ? " (FULLY CHARGED!)" : "";
						result.Message = cmd.BattleAction.TargetSelf
						? $"{cmd.Actor.CombatantName} used {cmd.BattleAction.ActionName} to heal self.{healChargeText}"
						: $"{cmd.Actor.CombatantName} used {cmd.BattleAction.ActionName} to heal {target.CombatantName}.{healChargeText}";
						break;

					case ActionType.Damage:
						result = CalculateDamage(cmd, target, result, isChargedAction, chargeMultiplier);
						break;

					case ActionType.Buff:
						ApplyBuff(target, cmd.BattleAction);
						result.Message = $"{cmd.Actor.CombatantName} buffed {target.CombatantName} with {cmd.BattleAction.ActionName}.";
						break;

					case ActionType.Debuff:
						ApplyDebuff(target, cmd.BattleAction);
						result.Message = $"{cmd.Actor.CombatantName} debuffed {target.CombatantName} with {cmd.BattleAction.ActionName}.";
						break;

					case ActionType.Item:
						result.Message = $"{cmd.Actor.CombatantName} used item {cmd.BattleAction.ActionName}.";
						break;
				}

				results.Add(result);
			}

			return results;
		}

		private static BattleResult ResolveCounterAttack(Combatant defender, Combatant attacker) {
			if (!defender.isAlive || !attacker.isAlive)
				return null;

			var counterAction = ScriptableObject.CreateInstance<BattleAction>();
			counterAction.ActionName = "Counter Attack";
			counterAction.ActionType = ActionType.Damage;
			counterAction.Power = Mathf.RoundToInt(defender.Attack * defender.CounterAttackMultiplier);
			counterAction.Accuracy = 1f;

			var counterCmd = new BattleCommand(defender, counterAction, attacker);
    
			// Calculate crit chance for counter attack too
			float critChance = CalculateCriticalHitChance(defender, counterAction);
			bool isCritical = Random.value < critChance;
    
			var counterResult = CalculateDamage(counterCmd, attacker, new BattleResult {
				Actor = defender,
				Target = attacker,
				BattleAction = counterAction,
				Success = true,
				CriticalHit = isCritical
			}, false, 1f);

			counterResult.Message = $"{defender.CombatantName} counter attacks {attacker.CombatantName} for {counterResult.DamageDealt} damage!";

			return counterResult;
		}

		private static BattleResult CalculateDamage(BattleCommand cmd, Combatant target, BattleResult result, bool isChargedAction, float chargeMultiplier) {
			float attack = cmd.Actor.Attack * cmd.BattleAction.StatMultiplier;
			float defense = Mathf.Max(1, target.Defense);
			float baseDamage = cmd.BattleAction.Power * (attack / defense);
			float randomFactor = Random.Range(0.85f, 1.0f);
			int damage = Mathf.Max(1, Mathf.FloorToInt(baseDamage * randomFactor));

			if (isChargedAction) {
				damage = Mathf.RoundToInt(damage * chargeMultiplier);
			}

			float critChance = CalculateCriticalHitChance(cmd.Actor, cmd.BattleAction);
			bool isCritical = Random.value < critChance;

			if (isCritical) {
				damage = Mathf.FloorToInt(damage * CRIT_MULTIPLIER);
				result.CriticalHit = true;
				result.CriticalChance = critChance;
        
				// Force critical hit visual
				Debug.Log($"CRITICAL HIT! {cmd.Actor.CombatantName} -> {target.CombatantName}: {damage} damage");
			}

			// CRITICAL: Make sure isCritical is passed through
			target.TakeDamage(damage, cmd.Actor, isCritical);
			result.DamageDealt = damage;

			string critText = isCritical ? " (CRITICAL HIT!)" : "";
			string chargeText = isChargedAction ? " (FULLY CHARGED!)" : "";
			result.Message = $"{cmd.Actor.CombatantName} used {cmd.BattleAction.ActionName} on {target.CombatantName}{critText}{chargeText} for {damage} damage.";

			return result;
		}

		private static float CalculateCriticalHitChance(Combatant attacker, BattleAction action) {
			float critChance = BASE_CRIT_CHANCE;
			critChance += action.CritChance;

			if (attacker.Team == Team.Player) {
				critChance *= PLAYER_CRIT_MODIFIER;
			}
			else {
				critChance *= ENEMY_CRIT_MODIFIER;
			}

			return Mathf.Clamp01(critChance);
		}

		private static List<Combatant> GetTargets(BattleCommand cmd, List<Combatant> allCombatants) {
			if (cmd.BattleAction.TargetSelf) return new List<Combatant> { cmd.Actor };

			if (allCombatants == null) return new List<Combatant> { cmd.Target };

			switch (cmd.BattleAction.TargetType) {
				case ActionTargetType.Self:
					return new List<Combatant> { cmd.Actor };

				case ActionTargetType.SingleEnemy:
					return new List<Combatant> { cmd.Target }.FindAll(t => t.Team != cmd.Actor.Team && t.isAlive);

				case ActionTargetType.AllEnemies:
					return allCombatants.FindAll(c => c.isAlive && c.Team != cmd.Actor.Team);

				case ActionTargetType.SingleAlly:
					return new List<Combatant> { cmd.Target }.FindAll(t => t.Team == cmd.Actor.Team && t.isAlive);

				case ActionTargetType.AllAllies:
					return allCombatants.FindAll(c => c.isAlive && c.Team == cmd.Actor.Team && c != cmd.Actor);

				default:
					return new List<Combatant> { cmd.Target };
			}
		}

		private static void ApplyBuff(Combatant target, BattleAction action) {
			if (action.AttackModifier != 0) target.Attack += Mathf.RoundToInt(target.Attack * action.AttackModifier);
			if (action.DefenseModifier != 0) target.Defense += Mathf.RoundToInt(target.Defense * action.DefenseModifier);
			if (action.SpeedModifier != 0) target.Speed += Mathf.RoundToInt(target.Speed * action.SpeedModifier);
		}

		private static void ApplyDebuff(Combatant target, BattleAction action) {
			if (action.AttackModifier != 0) target.Attack -= Mathf.RoundToInt(target.Attack * action.AttackModifier);
			if (action.DefenseModifier != 0) target.Defense -= Mathf.RoundToInt(target.Defense * action.DefenseModifier);
			if (action.SpeedModifier != 0) target.Speed -= Mathf.RoundToInt(target.Speed * action.SpeedModifier);
		}
	}
}
