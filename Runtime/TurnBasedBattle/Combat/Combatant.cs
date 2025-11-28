using System.Collections.Generic;
using UnityEngine;

namespace RyanMillerGameCore.TurnBasedCombat {

	public class Combatant : MonoBehaviour, ICombatant {

		// returns the combatant's actions in a List<BattleAction> form for UI compatibility
		public List<BattleAction> MovesAsActions() {
			// Return a copy to avoid accidental modification of the original list by UI code.
			// If you prefer not to copy, return m_Moves directly.
			if (m_Moves == null) {
				return new List<BattleAction>();
			}
			return new List<BattleAction>(m_Moves);
		}

		public string CombatantName {
			get { return m_CombatantName; }
		}

		public int MaxHp {
			get { return m_MaxHp; }
		}

		public int Attack {
			get { return m_Attack; }
			set { m_Attack = value; }
		}

		public int Defense {
			get { return m_Defense; }
			set { m_Defense = value; }
		}

		public int Speed {
			get { return m_Speed; }
			set { m_Speed = value; }
		}

		public int CurrentHp {
			get { return m_CurrentHp; }
		}

		public bool IsAlive {
			get {
				return m_CurrentHp > 0;
			}
		}

		public List<BattleAction> Moves {
			get { return m_Moves; }
		}

		public Team Team {
			get { return m_Team; }
		}

		public Color Color {
			get { return m_Color; }
		}

		public EnemyAIBrain AIBrain {
			get { return m_AIBrain; }
		}

		public Combatant LastAttacker {
			get { return lastAttacker; }
			set { lastAttacker = value; }
		}

		public bool IsDefending {
			get { return isDefending; }
		}

		public float CounterChance {
			get { return counterAttackChance; }
			set { counterAttackChance = value; }
		}

		public float CounterAttackMultiplier {
			get { return counterAttackMultiplier; }
		}

		public float TurnGauge {
			get { return m_TurnGauge; }
		}

		[SerializeField] private string m_CombatantName;
		[SerializeField] private int m_MaxHp = 100;
		[SerializeField] private int m_Attack = 20;
		[SerializeField] private int m_Defense = 10;
		[SerializeField] private int m_Speed = 10;
		[SerializeField] private int m_CurrentHp = 100;
		[SerializeField] private List<BattleAction> m_Moves;
		[SerializeField] private Team m_Team = Team.Enemy;
		[SerializeField] private Color m_Color;
		[SerializeField] private EnemyAIBrain m_AIBrain;

		private float m_TurnGauge = 0f;
		private MultiTurnActionState currentMultiTurnAction;
		private bool isCharging = false;
		private bool isDefending = false;
		private int defendTurnsRemaining = 0;
		private float defendDamageReduction = 1f;
		private float counterAttackChance = 0f;
		private float counterAttackMultiplier = 1f;
		private Combatant lastAttacker = null;
		private bool hasAttackBuff = false;
		private int attackBuffTurnsRemaining = 0;
		private float attackBuffMultiplier = 1f;
		private int originalAttack;

		public delegate void OnCombatantEvent(CombatantEventData eventData);
		public event OnCombatantEvent CombatantEvent;

		public bool m_IsPlayer {
			get { return m_Team == Team.Player; }
			set { m_Team = value ? Team.Player : Team.Enemy; }
		}

		private void Awake() {
			m_CurrentHp = Mathf.Clamp(m_CurrentHp, 0, m_MaxHp);
			originalAttack = m_Attack;
		}

		public (BattleAction action, Combatant target) DecideAIAction(List<Combatant> validTargets) {
			if (m_AIBrain != null) {
				return m_AIBrain.ChooseAction(this, validTargets, m_Moves);
			}
			else {
				if (m_Moves.Count == 0 || validTargets.Count == 0)
					return (null, null);

				var randomAction = m_Moves[Random.Range(0, m_Moves.Count)];
				var randomTarget = validTargets[Random.Range(0, validTargets.Count)];
				return (randomAction, randomTarget);
			}
		}

		private void RaiseCombatantEvent(CombatantEventType eventType, string message, int amount = 0) {
			CombatantEvent?.Invoke(new CombatantEventData {
				EventType = eventType,
				Combatant = this,
				Message = message,
				Amount = amount,
				Timestamp = Time.time
			});
		}

		public void RaiseCriticalHit(int damage, Combatant attacker = null) {
			string attackerName = attacker != null ? attacker.CombatantName : "unknown";
			RaiseCombatantEvent(CombatantEventType.CriticalDamageTaken,
				$"{m_CombatantName} takes a critical hit from {attackerName} for {damage} damage!", damage);
		}

		public void StartDefend(BattleAction defendAction) {
			isDefending = true;
			defendTurnsRemaining = defendAction.DefendDuration;
			defendDamageReduction = defendAction.DamageReduction;
			counterAttackChance = defendAction.CounterChance;
			counterAttackMultiplier = defendAction.CounterMultiplier;

			if (defendAction.DefendAttackBuff > 1f) {
				ApplyAttackBuff(defendAction.DefendAttackBuff, defendAction.AttackBuffDuration);
			}

			string buffText = defendAction.DefendAttackBuff > 1f ?
				$", attack increased by {((defendAction.DefendAttackBuff - 1f) * 100f):0}% for {defendAction.AttackBuffDuration} turn(s)" : "";

			RaiseCombatantEvent(CombatantEventType.DefendStarted,
				$"{m_CombatantName} takes a defensive stance! Damage reduced by {((1f - defendDamageReduction) * 100f):0}% for {defendTurnsRemaining} turn(s){buffText}.");
		}

		public void ApplyAttackBuff(float multiplier, int duration) {
			hasAttackBuff = true;
			attackBuffTurnsRemaining = duration;
			attackBuffMultiplier = multiplier;

			originalAttack = m_Attack;
			m_Attack = Mathf.RoundToInt(m_Attack * multiplier);

			RaiseCombatantEvent(CombatantEventType.AttackBuffed,
				$"{m_CombatantName}'s attack increases to {m_Attack} for {duration} turn(s)!");
		}

		public void RemoveAttackBuff() {
			if (hasAttackBuff) {
				m_Attack = originalAttack;
				hasAttackBuff = false;
				attackBuffTurnsRemaining = 0;
				attackBuffMultiplier = 1f;

				RaiseCombatantEvent(CombatantEventType.AttackBuffEnded,
					$"{m_CombatantName}'s attack returns to normal.");
			}
		}

		public void AdvanceAttackBuffTurn() {
			if (hasAttackBuff) {
				attackBuffTurnsRemaining--;
				if (attackBuffTurnsRemaining <= 0) {
					RemoveAttackBuff();
				}
			}
		}

		public void EndDefend() {
			if (isDefending) {
				isDefending = false;
				defendTurnsRemaining = 0;
				defendDamageReduction = 1f;
				counterAttackChance = 0f;
				counterAttackMultiplier = 1f;
				lastAttacker = null;

				RaiseCombatantEvent(CombatantEventType.DefendEnded,
					$"{m_CombatantName} drops defensive stance.");
			}
		}

		public void AdvanceDefendTurn() {
			if (isDefending) {
				defendTurnsRemaining--;
				if (defendTurnsRemaining <= 0) {
					EndDefend();
				}
			}
		}

		public bool TryCounterAttack(Combatant attacker) {
			if (!isDefending || !isAlive || !attacker.isAlive)
				return false;

			if (Random.value <= counterAttackChance) {
				lastAttacker = attacker;
				return true;
			}
			return false;
		}
		
		public void TakeDamage(int dmg) {
			TakeDamage(dmg, null, false);
		}
		
		public void TakeDamage(int dmg, Combatant attacker, bool isCritical) { 
			int originalDamage = dmg;

			if (isDefending) {
				dmg = Mathf.Max(1, Mathf.RoundToInt(dmg * defendDamageReduction));
			}

			int previousHp = m_CurrentHp;
			m_CurrentHp -= dmg;
			if (m_CurrentHp < 0) {
				m_CurrentHp = 0;
			}

			string defendText = isDefending ? $" (reduced from {originalDamage} due to defense)" : "";

			// Raise appropriate event based on critical hit
			if (isCritical) {
				RaiseCriticalHit(dmg, attacker);
			}
			else {
				RaiseCombatantEvent(CombatantEventType.DamageTaken,
					$"{m_CombatantName} takes {dmg} damage{defendText}. (HP: {m_CurrentHp}/{m_MaxHp})", dmg);
			}

			if (isDefending && attacker != null && dmg > 0) {
				if (TryCounterAttack(attacker)) {
					RaiseCombatantEvent(CombatantEventType.CounterAttack,
						$"{m_CombatantName} prepares a counter attack against {attacker.m_CombatantName}!");
				}
			}

			if (m_CurrentHp == 0 && previousHp > 0) {
				if (isCharging && currentMultiTurnAction != null) {
					CancelMultiTurnAction();
				}
				EndDefend();
				RemoveAttackBuff();
				Die();
			}
		}

		public void RaiseAttackStarted() {
			RaiseCombatantEvent(CombatantEventType.AttackStarted,
				$"{m_CombatantName} begins an attack!");
		}

		public void RaiseAttackHit(Combatant target, int damage) {
			RaiseCombatantEvent(CombatantEventType.AttackHit,
				$"{m_CombatantName}'s attack hits {target.CombatantName} for {damage} damage!", damage);
		}

		public void RaiseAttackMissed(Combatant target) {
			RaiseCombatantEvent(CombatantEventType.AttackMissed,
				$"{m_CombatantName}'s attack misses {target.CombatantName}!");
		}

		public void RaiseSkillUsed(BattleAction skill) {
			RaiseCombatantEvent(CombatantEventType.SkillUsed,
				$"{m_CombatantName} uses {skill.ActionName}!");
		}

		public void RaiseTurnStarted() {
			RaiseCombatantEvent(CombatantEventType.TurnStarted,
				$"{m_CombatantName}'s turn begins!");
		}

		public void RaiseTurnEnded() {
			RaiseCombatantEvent(CombatantEventType.TurnEnded,
				$"{m_CombatantName}'s turn ends!");
		}

		public void StartMultiTurnAction(BattleAction action, Combatant target) {
			currentMultiTurnAction = new MultiTurnActionState(action, this, target);
			isCharging = true;
			ApplyChargeTurnEffects();

			RaiseCombatantEvent(CombatantEventType.ChargeStarted,
				$"{m_CombatantName} begins charging {action.ActionName}! ({action.TurnCost} turns)");
		}

		public bool AdvanceMultiTurnAction() {
			if (currentMultiTurnAction == null) return false;

			bool isReady = currentMultiTurnAction.AdvanceTurn();

			if (isReady) {
				RaiseCombatantEvent(CombatantEventType.ChargeComplete,
					$"{m_CombatantName} completes charging {currentMultiTurnAction.action.ActionName}!");
				return true;
			}
			else {
				ApplyChargeTurnEffects();
				RaiseCombatantEvent(CombatantEventType.Charging,
					currentMultiTurnAction.GetChargeMessage(this));
				return false;
			}
		}

		public void CompleteMultiTurnAction() {
			if (currentMultiTurnAction != null) {
				RemoveChargeTurnEffects();
				currentMultiTurnAction = null;
				isCharging = false;
			}
		}

		public void CancelMultiTurnAction() {
			if (currentMultiTurnAction != null) {
				RemoveChargeTurnEffects();
				RaiseCombatantEvent(CombatantEventType.ChargeCancelled,
					$"{m_CombatantName}'s {currentMultiTurnAction.action.ActionName} was cancelled!");
				currentMultiTurnAction = null;
				isCharging = false;
			}
		}

		private void ApplyChargeTurnEffects() {
			if (currentMultiTurnAction == null) return;

			switch (currentMultiTurnAction.action.ChargeTurnBehavior) {
				case ChargeTurnBehavior.ApplyDefenseBuff:
					if (!currentMultiTurnAction.defenseBuffApplied) {
						int defenseBonus = Mathf.RoundToInt(m_Defense * 0.3f);
						m_Defense += defenseBonus;
						currentMultiTurnAction.defenseBuffApplied = true;
					}
					break;

				case ChargeTurnBehavior.ApplySpeedDebuff:
					if (!currentMultiTurnAction.speedDebuffApplied) {
						int speedReduction = Mathf.RoundToInt(m_Speed * 0.2f);
						m_Speed = Mathf.Max(1, m_Speed - speedReduction);
						currentMultiTurnAction.speedDebuffApplied = true;
					}
					break;

				case ChargeTurnBehavior.TakeDamage:
					int chargeDamage = Mathf.Max(1, m_MaxHp / 20);
					TakeDamage(chargeDamage);
					RaiseCombatantEvent(CombatantEventType.ChargeDamage,
						$"{m_CombatantName} takes {chargeDamage} damage from charging!");
					break;
			}
		}

		private void RemoveChargeTurnEffects() {
			if (currentMultiTurnAction == null) return;

			if (currentMultiTurnAction.defenseBuffApplied) {
				m_Defense = currentMultiTurnAction.originalDefense;
			}
			if (currentMultiTurnAction.speedDebuffApplied) {
				m_Speed = currentMultiTurnAction.originalSpeed;
			}
		}

		public string ColorAsHex {
			get {
				return "#" + ColorUtility.ToHtmlStringRGB(m_Color);
			}
		}

		public bool isAlive {
			get {
				return m_CurrentHp > 0;
			}
		}

		public void Die() {
			RaiseCombatantEvent(CombatantEventType.Died, $"{m_CombatantName} died!");
		}

		public void Heal(int amount) {
			int previousHp = m_CurrentHp;
			m_CurrentHp += amount;
			if (m_CurrentHp > m_MaxHp) {
				m_CurrentHp = m_MaxHp;
			}

			int actualHeal = m_CurrentHp - previousHp;
			RaiseCombatantEvent(CombatantEventType.HealingReceived,
				$"{m_CombatantName} heals {actualHeal}. (HP: {m_CurrentHp}/{m_MaxHp})", actualHeal);

			if (m_CurrentHp == m_MaxHp) {
				RaiseCombatantEvent(CombatantEventType.FullHealth, $"{m_CombatantName} is at full health!");
			}
		}
	}

	[System.Serializable]
	public class MultiTurnActionState {
		public BattleAction action;
		public Combatant target;
		public int turnsRemaining;
		public int currentTurn;
		public bool isComplete = false;

		public int originalDefense;
		public int originalSpeed;
		public bool defenseBuffApplied = false;
		public bool speedDebuffApplied = false;

		public MultiTurnActionState(BattleAction action, Combatant actor, Combatant target) {
			this.action = action;
			this.target = target;
			this.turnsRemaining = action.TurnCost;
			this.currentTurn = 0;
			this.originalDefense = actor.Defense;
			this.originalSpeed = actor.Speed;
		}

		public bool AdvanceTurn() {
			currentTurn++;
			turnsRemaining--;

			if (turnsRemaining <= 0) {
				isComplete = true;
				return true;
			}
			return false;
		}

		public string GetChargeMessage(Combatant actor) {
			return $"{actor.CombatantName}{action.ChargeMessage} ({turnsRemaining} turn(s) remaining)";
		}
	}

	public enum CombatantEventType {
		DamageTaken,
		CriticalDamageTaken,
		HealingReceived,
		Died,
		FullHealth,
		ChargeStarted,
		Charging,
		ChargeComplete,
		ChargeCancelled,
		ChargeDamage,
		DefendStarted,
		DefendEnded,
		CounterAttack,
		AttackBuffed,
		AttackBuffEnded,
		AttackStarted,
		AttackHit,
		AttackMissed,
		SkillUsed,
		TurnStarted,
		TurnEnded
	}

	public struct CombatantEventData {
		public CombatantEventType EventType;
		public Combatant Combatant;
		public string Message;
		public int Amount;
		public float Timestamp;
	}

	public enum Team {
		Player,
		Enemy,
		Neutral
	}
}
