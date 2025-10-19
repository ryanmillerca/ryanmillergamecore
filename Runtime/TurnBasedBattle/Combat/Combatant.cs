using System.Collections.Generic;
using UnityEngine;

namespace RyanMillerGameCore.TurnBasedCombat
{
    public enum Team
    {
        Player,
        Enemy,
        Neutral
    }

    public class Combatant : MonoBehaviour
    {
        public string m_CombatantName;
        public int m_MaxHp = 100;
        public int m_Attack = 20;
        public int m_Defense = 10;
        public int m_Speed = 10;
        public int m_CurrentHp = 100;
        public List<BattleAction> m_Moves;
        public Team m_Team = Team.Enemy;
        [HideInInspector] public float m_TurnGauge = 0f;
        public Color m_Color;

        [HideInInspector] public MultiTurnActionState currentMultiTurnAction;
        [HideInInspector] public bool isCharging = false;

        public delegate void OnCombatantEvent(CombatantEventData eventData);
        public event OnCombatantEvent CombatantEvent;

        public bool m_IsPlayer
        {
            get { return m_Team == Team.Player; }
            set { m_Team = value ? Team.Player : Team.Enemy; }
        }

        private void Awake()
        {
            m_CurrentHp = Mathf.Clamp(m_CurrentHp, 0, m_MaxHp);
        }

        private void RaiseCombatantEvent(CombatantEventType eventType, string message, int amount = 0)
        {
            CombatantEvent?.Invoke(new CombatantEventData
            {
                EventType = eventType,
                Combatant = this,
                Message = message,
                Amount = amount,
                Timestamp = Time.time
            });
        }

        public void StartMultiTurnAction(BattleAction action, Combatant target)
        {
            currentMultiTurnAction = new MultiTurnActionState(action, this, target);
            isCharging = true;
            ApplyChargeTurnEffects();

            RaiseCombatantEvent(CombatantEventType.ChargeStarted,
                $"{m_CombatantName} begins charging {action.m_ActionName}! ({action.m_TurnCost} turns)");
        }

        public bool AdvanceMultiTurnAction()
        {
            if (currentMultiTurnAction == null) return false;

            bool isReady = currentMultiTurnAction.AdvanceTurn();

            if (isReady)
            {
                RaiseCombatantEvent(CombatantEventType.ChargeComplete,
                    $"{m_CombatantName} completes charging {currentMultiTurnAction.action.m_ActionName}!");
                return true;
            }
            else
            {
                ApplyChargeTurnEffects();
                RaiseCombatantEvent(CombatantEventType.Charging,
                    currentMultiTurnAction.GetChargeMessage(this));
                return false;
            }
        }

        public void CompleteMultiTurnAction()
        {
            if (currentMultiTurnAction != null)
            {
                RemoveChargeTurnEffects();
                currentMultiTurnAction = null;
                isCharging = false;
            }
        }

        public void CancelMultiTurnAction()
        {
            if (currentMultiTurnAction != null)
            {
                RemoveChargeTurnEffects();
                RaiseCombatantEvent(CombatantEventType.ChargeCancelled,
                    $"{m_CombatantName}'s {currentMultiTurnAction.action.m_ActionName} was cancelled!");
                currentMultiTurnAction = null;
                isCharging = false;
            }
        }

        private void ApplyChargeTurnEffects()
        {
            if (currentMultiTurnAction == null) return;

            switch (currentMultiTurnAction.action.m_ChargeTurnBehavior)
            {
                case ChargeTurnBehavior.ApplyDefenseBuff:
                    if (!currentMultiTurnAction.defenseBuffApplied)
                    {
                        int defenseBonus = Mathf.RoundToInt(m_Defense * 0.3f);
                        m_Defense += defenseBonus;
                        currentMultiTurnAction.defenseBuffApplied = true;
                    }
                    break;

                case ChargeTurnBehavior.ApplySpeedDebuff:
                    if (!currentMultiTurnAction.speedDebuffApplied)
                    {
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

        private void RemoveChargeTurnEffects()
        {
            if (currentMultiTurnAction == null) return;

            if (currentMultiTurnAction.defenseBuffApplied)
            {
                m_Defense = currentMultiTurnAction.originalDefense;
            }
            if (currentMultiTurnAction.speedDebuffApplied)
            {
                m_Speed = currentMultiTurnAction.originalSpeed;
            }
        }

        public string ColorAsHex
        {
            get
            {
                return "#" + ColorUtility.ToHtmlStringRGB(m_Color);
            }
        }

        public bool isAlive
        {
            get
            {
                return m_CurrentHp > 0;
            }
        }

        public void TakeDamage(int dmg)
        {
            int previousHp = m_CurrentHp;
            m_CurrentHp -= dmg;
            if (m_CurrentHp < 0)
            {
                m_CurrentHp = 0;
            }

            RaiseCombatantEvent(CombatantEventType.DamageTaken,
                $"{m_CombatantName} takes {dmg} damage. (HP: {m_CurrentHp}/{m_MaxHp})", dmg);

            if (m_CurrentHp == 0 && previousHp > 0)
            {
                // Only cancel multi-turn action if we were actually charging one
                if (isCharging && currentMultiTurnAction != null)
                {
                    CancelMultiTurnAction();
                }
                Die();
            }
        }

        public void Die()
        {
            RaiseCombatantEvent(CombatantEventType.Died, $"{m_CombatantName} died!");
        }

        public void Heal(int amount)
        {
            int previousHp = m_CurrentHp;
            m_CurrentHp += amount;
            if (m_CurrentHp > m_MaxHp)
            {
                m_CurrentHp = m_MaxHp;
            }

            int actualHeal = m_CurrentHp - previousHp;
            RaiseCombatantEvent(CombatantEventType.HealingReceived,
                $"{m_CombatantName} heals {actualHeal}. (HP: {m_CurrentHp}/{m_MaxHp})", actualHeal);

            if (m_CurrentHp == m_MaxHp)
            {
                RaiseCombatantEvent(CombatantEventType.FullHealth, $"{m_CombatantName} is at full health!");
            }
        }
    }

    [System.Serializable]
    public class MultiTurnActionState
    {
        public BattleAction action;
        public Combatant target;
        public int turnsRemaining;
        public int currentTurn;
        public bool isComplete = false;

        public int originalDefense;
        public int originalSpeed;
        public bool defenseBuffApplied = false;
        public bool speedDebuffApplied = false;

        public MultiTurnActionState(BattleAction action, Combatant actor, Combatant target)
        {
            this.action = action;
            this.target = target;
            this.turnsRemaining = action.m_TurnCost;
            this.currentTurn = 0;
            this.originalDefense = actor.m_Defense;
            this.originalSpeed = actor.m_Speed;
        }

        public bool AdvanceTurn()
        {
            currentTurn++;
            turnsRemaining--;

            if (turnsRemaining <= 0)
            {
                isComplete = true;
                return true;
            }
            return false;
        }

        public string GetChargeMessage(Combatant actor)
        {
            return $"{actor.m_CombatantName}{action.m_ChargeMessage} ({turnsRemaining} turn(s) remaining)";
        }
    }

    public enum CombatantEventType
    {
        DamageTaken,
        HealingReceived,
        Died,
        FullHealth,
        ChargeStarted,
        Charging,
        ChargeComplete,
        ChargeCancelled,
        ChargeDamage
    }

    public struct CombatantEventData
    {
        public CombatantEventType EventType;
        public Combatant Combatant;
        public string Message;
        public int Amount;
        public float Timestamp;
    }
}