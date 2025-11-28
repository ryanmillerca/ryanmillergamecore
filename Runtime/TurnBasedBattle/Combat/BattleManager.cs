namespace RyanMillerGameCore.TurnBasedCombat {
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using System;
	using Random = UnityEngine.Random;
	using UnityEngine.Events;

	public class BattleManager : MonoBehaviour, ITargetProvider {

		public List<Combatant> Combatants {
			get {
				return m_Combatants;
			}
		}

		[SerializeField] private bool autoStart = true;
		[SerializeField] private List<Combatant> m_Combatants = new List<Combatant>();
		[SerializeField] private Combatant[] m_Players;
		[SerializeField] private float m_GaugeThreshold = 100f;
		[SerializeField] private float m_TickRate = 1f;
		[SerializeField] private int m_LookaheadTurns = 5;
		[SerializeField] private float m_TurnDelay = 0.5f;
		[SerializeField] private Transform[] m_PlayerSlots;
		[SerializeField] private Transform[] m_EnemySlots;
		[SerializeField] private UnityEvent BattleDidStart;
		[SerializeField] private UnityEvent BattleDidEnd;
		[SerializeField] private MinigameResolver m_MinigameResolver;

		private bool m_WaitingForPlayerInput = false;
		private Combatant m_CurrentPlayerActor = null;
		private List<Combatant> m_CurrentValidTargets = null;
		private BattleCommand m_PendingPlayerCommand = null;

		public delegate void OnMoveResolved(BattleResult result);
		public event OnMoveResolved MoveResolved;

		public delegate void OnBattleEvent(BattleEventData eventData);
		public event OnBattleEvent BattleEvent;

		public delegate void OnTurnEvent(TurnEventData eventData);
		public event OnTurnEvent TurnEvent;

		public delegate void OnBattleOutcome(BattleOutcome outcome);
		public event OnBattleOutcome BattleEnded;

		public delegate void OnPlayerInputRequired(PlayerInputData inputData);
		public event OnPlayerInputRequired PlayerInputRequired;

		public delegate void OnPlayerInputReceived(PlayerInputResponse response);
		public event OnPlayerInputReceived PlayerInputReceived;
		public event Action<Combatant, List<BattleAction>> OnPlayerActionRequested;
		public event Action<List<Combatant>> OnTurnOrderUpdated;

		public void SetupNewBattle(Combatant[] enemies) {
			ClearCombatants();
			int usedPlayerSlots = 0;
			int usedEnemySlots = 0;
			foreach (var player in m_Players) {
				m_Combatants.Add(NewCombatant(player, m_PlayerSlots[usedPlayerSlots]));
				usedPlayerSlots++;
			}
			foreach (var enemy in enemies) {
				m_Combatants.Add(NewCombatant(enemy, m_EnemySlots[usedEnemySlots]));
				usedEnemySlots++;
			}
			InitializeCombatants();
			StartCoroutine(BattleLoop());
		}

		void ClearCombatants() {
			for (int index = m_Combatants.Count - 1; index >= 0; index--) {
				Combatant combatant = m_Combatants[index];
				Destroy(combatant.gameObject);
			}
			Combatants.Clear();
		}

		Combatant NewCombatant(Combatant combatant, Transform slot) {
			GameObject newObj = Instantiate(combatant.gameObject, slot);
			newObj.transform.localPosition = Vector3.zero;
			return newObj.GetComponent<Combatant>();
		}

		public void SubmitPlayerCommand(BattleCommand cmd) {
			if (!m_WaitingForPlayerInput) {
				return;
			}
			if (cmd == null) {
				return;
			}
			if (m_CurrentPlayerActor == null) {
				return;
			}
			if (cmd.Actor != m_CurrentPlayerActor) {
				return;
			}
			if (!m_CurrentPlayerActor.isAlive) {
				return;
			}

			m_PendingPlayerCommand = cmd;
			m_WaitingForPlayerInput = false;
			PlayerInputReceived?.Invoke(new PlayerInputResponse {
				SelectedAction = cmd.BattleAction,
				SelectedTarget = cmd.Target,
				IsValid = true,
				ValidationMessage = null
			});
		}

		public IEnumerable<Combatant> GetValidTargets(Combatant actor) {
			var validTargets = new List<Combatant>();
			if (!actor) {
				return validTargets;
			}
			foreach (Combatant combatant in m_Combatants) {
				if (!combatant.isAlive || combatant == actor) {
					continue;
				}
				if (actor.Team != combatant.Team) {
					validTargets.Add(combatant);
				}
			}
			return validTargets;
		}

		public void SubmitPlayerInput(PlayerInputResponse response) {
			if (!m_WaitingForPlayerInput || m_CurrentPlayerActor == null) {
				return;
			}
			m_WaitingForPlayerInput = false;
			PlayerInputReceived?.Invoke(response);
		}

		public void CancelPlayerInput() {
			if (!m_WaitingForPlayerInput) {
				return;
			}
			m_WaitingForPlayerInput = false;
			m_CurrentPlayerActor = null;
			m_CurrentValidTargets = null;
			m_PendingPlayerCommand = null;
		}

		private void RaiseBattleEvent(BattleEventType eventType, string message, Combatant combatant = null, Combatant target = null) {
			BattleEvent?.Invoke(new BattleEventData {
				EventType = eventType,
				Message = message,
				Combatant = combatant,
				Target = target,
				Timestamp = Time.time
			});
		}

		private void RaiseTurnEvent(TurnEventType eventType, Combatant combatant, Combatant target = null, BattleAction action = null) {
			TurnEvent?.Invoke(new TurnEventData {
				EventType = eventType,
				Combatant = combatant,
				Target = target,
				Action = action,
				Timestamp = Time.time
			});
		}

		private bool PlayersAreAlive() {
			return m_Combatants.Any(c => c.isAlive && c.Team == Team.Player);
		}

		private bool EnemiesAreAlive() {
			return m_Combatants.Any(c => c.isAlive && c.Team == Team.Enemy);
		}

		private void Start() {
			if (autoStart) {
				InitializeCombatants();
				StartCoroutine(BattleLoop());
			}
		}

		private void InitializeCombatants() {
			foreach (var c in m_Combatants) {
				c.CombatantEvent += OnCombatantEventHandler;
			}
		}

		private void OnCombatantEventHandler(CombatantEventData eventData) {
			if (eventData.EventType == CombatantEventType.CounterAttack) {
				StartCoroutine(HandleCounterAttack(eventData.Combatant));
			}
		}

		private IEnumerator HandleCounterAttack(Combatant counterActor) {
			Combatant target = counterActor.LastAttacker;

			if (target == null || !target.isAlive) {
				counterActor.LastAttacker = null;
				yield break;
			}

			float originalTargetCounterChance = target.CounterChance;
			target.CounterChance = 0f;
			BattleAction counterAction = GetOrCreateCounterAction(counterActor);

			if (counterAction == null) {
				RaiseBattleEvent(BattleEventType.CommandError, $"{counterActor.CombatantName} failed to find a counter action.", counterActor);
				target.CounterChance = originalTargetCounterChance;
				yield break;
			}

			var counterCmd = new BattleCommand(counterActor, counterAction, target);

			// Raise attack started event for counter attack
			counterActor.RaiseAttackStarted();
			RaiseTurnEvent(TurnEventType.ActionSelected, counterActor, target, counterAction);

			List<BattleResult> results = MoveResolver.Resolve(counterCmd, m_Combatants);

			if (results != null) {
				foreach (var result in results) {
					MoveResolved?.Invoke(result);
				}
			}
			target.CounterChance = originalTargetCounterChance;
			counterActor.LastAttacker = null;
			yield return new WaitForSeconds(0.2f);
		}

		private BattleAction GetOrCreateCounterAction(Combatant actor) {
			var counterMove = actor.Moves.FirstOrDefault(m => m.ActionName.ToLower() == "counter");
			if (counterMove == null) {
				counterMove = actor.Moves.FirstOrDefault(m => m.ActionType == ActionType.Damage);
			}
			return counterMove;
		}

		private IEnumerator BattleLoop() {
			RaiseBattleEvent(BattleEventType.BattleStarted, "Battle started!");
			BattleDidStart?.Invoke();
			var turnQueue = new Queue<Combatant>();
			while (PlayersAreAlive() && EnemiesAreAlive()) {
				FillTurnQueueEvenly(turnQueue);
				while (turnQueue.Count > 0 && PlayersAreAlive() && EnemiesAreAlive()) {
					var next = turnQueue.Dequeue();

					if (next.isAlive) {
						yield return StartCoroutine(TakeTurn(next));
					}
				}
				DisplayTurnOrder();
				yield return new WaitForSeconds(m_TickRate);
			}
			BattleOutcome outcome;
			if (PlayersAreAlive() && !EnemiesAreAlive()) {
				outcome = BattleOutcome.Victory;
				RaiseBattleEvent(BattleEventType.BattleEnded, "Victory! All enemies defeated!");
			}
			else if (!PlayersAreAlive() && EnemiesAreAlive()) {
				outcome = BattleOutcome.Defeat;
				RaiseBattleEvent(BattleEventType.BattleEnded, "Defeat! All players are down!");
			}
			else {
				outcome = BattleOutcome.Undefined;
				RaiseBattleEvent(BattleEventType.BattleEnded, "Battle ended unexpectedly!");
			}
			BattleEnded?.Invoke(outcome);
			BattleDidEnd?.Invoke();
		}

		private void FillTurnQueueEvenly(Queue<Combatant> turnQueue) {
			var aliveCombatants = m_Combatants.Where(c => c.isAlive).ToList();
			if (aliveCombatants.Count == 0) { return; }
			int totalSpeed = aliveCombatants.Sum(c => c.Speed);
			var combatantList = new List<CombatantData>();
			foreach (Combatant combatant in aliveCombatants) {
				combatantList.Add(new CombatantData {
					Combatant = combatant,
					Speed = combatant.Speed,
					Step = (double)totalSpeed / combatant.Speed,
					Position = 0.0
				});
			}
			foreach (CombatantData data in combatantList) {
				data.Position = data.Step / 2;
			}
			for (int i = 0; i < totalSpeed; i++) {
				CombatantData nextData = combatantList.OrderBy(d => d.Position).First();
				turnQueue.Enqueue(nextData.Combatant);
				nextData.Position += nextData.Step;
			}
		}

		private IEnumerator TakeTurn(Combatant c) {
			if (c == null) { yield break; }

			if (!c.isAlive) {
				RaiseBattleEvent(BattleEventType.TurnSkipped, $"{c.CombatantName} was dead at turn start — skipping.", c);
				yield break;
			}

			// Raise turn started events
			c.RaiseTurnStarted();
			RaiseTurnEvent(TurnEventType.TurnStarted, c);

			foreach (Combatant combatant in m_Combatants) {
				if (combatant.isAlive) {
					combatant.AdvanceDefendTurn();
					combatant.AdvanceAttackBuffTurn();
				}
			}

			if (c.Team == Team.Player) {
				yield return StartCoroutine(HandlePlayerTurn(c));
			}
			else {
				yield return StartCoroutine(HandleAITurn(c));
			}

			if (!PlayersAreAlive() || !EnemiesAreAlive()) {
				RaiseBattleEvent(BattleEventType.BattleEndConditionMet, $"After {c.CombatantName}'s action, battle end condition met.", c);
				yield break;
			}

			// Raise turn ended events
			c.RaiseTurnEnded();
			RaiseTurnEvent(TurnEventType.TurnEnded, c);
			yield return new WaitForSeconds(m_TurnDelay);
		}

		private IEnumerator HandlePlayerTurn(Combatant player) {
			var availableMoves = player.Moves?.Where(m => m != null).ToList() ?? new List<BattleAction>();
			var validTargets = GetValidTargets(player).ToList();

			if (availableMoves.Count == 0) {
				RaiseBattleEvent(BattleEventType.NoMovesAvailable, $"{player.CombatantName} has no moves. Ending turn.", player);
				yield break;
			}

			if (validTargets.Count == 0) {
				RaiseBattleEvent(BattleEventType.NoValidTargets, $"No valid targets found for {player.CombatantName}. Ending turn.", player);
				yield break;
			}

			var inputData = new PlayerInputData {
				Actor = player,
				AvailableMoves = availableMoves,
				ValidTargets = validTargets,
				Timestamp = Time.time
			};

			m_WaitingForPlayerInput = true;
			m_CurrentPlayerActor = player;
			m_CurrentValidTargets = validTargets;
			m_PendingPlayerCommand = null;

			PlayerInputRequired?.Invoke(inputData);
			OnPlayerActionRequested?.Invoke(player, availableMoves);

			yield return new WaitUntil(() => !m_WaitingForPlayerInput);

			if (m_PendingPlayerCommand != null) {

				// start mini game
				if (m_PendingPlayerCommand.BattleAction.ActionType == ActionType.Minigame) {
					AbstractMinigame abstractMinigame = m_MinigameResolver.GetMinigameWithName(m_PendingPlayerCommand.BattleAction.ActionName);
					if (abstractMinigame) {
						abstractMinigame.BattleManager = this;
						abstractMinigame.BattleCommand = m_PendingPlayerCommand;
						yield return m_MinigameResolver.StartMinigame(abstractMinigame);
					}
				}
				ExecuteBattleCommand(m_PendingPlayerCommand);
				m_PendingPlayerCommand = null;
			}
			else {
				yield return StartCoroutine(ExecutePlayerAction(player, availableMoves, validTargets));
			}

			m_CurrentPlayerActor = null;
			m_CurrentValidTargets = null;
		}

		public void ExecuteBattleCommand(BattleCommand cmd) {
			if (cmd == null) {
				return;
			}

			if (cmd.Actor == null || cmd.BattleAction == null || cmd.Target == null) {
				RaiseBattleEvent(BattleEventType.CommandError, $"Submitted command invalid for {cmd.Actor?.CombatantName ?? "null actor"}", cmd.Actor, cmd.Target);
				return;
			}

			// Raise skill used event for non-basic attacks
			if (cmd.BattleAction.ActionType != ActionType.Damage || cmd.BattleAction.ActionName.ToLower() != "attack") {
				cmd.Actor.RaiseSkillUsed(cmd.BattleAction);
			}

			// Raise attack started event for damage actions
			if (cmd.BattleAction.ActionType == ActionType.Damage) {
				cmd.Actor.RaiseAttackStarted();
			}

			RaiseTurnEvent(TurnEventType.ActionSelected, cmd.Actor, cmd.Target, cmd.BattleAction);

			List<BattleResult> results = null;
			try {
				results = MoveResolver.Resolve(cmd, m_Combatants);
			}
			catch (System.Exception ex) {
				RaiseBattleEvent(BattleEventType.ResolutionError, $"MoveResolver.Resolve threw for {cmd.Actor.CombatantName}: {ex}", cmd.Actor);
				return;
			}

			if (results != null) {
				foreach (var result in results) {
					// Raise hit/miss events for damage actions
					if (cmd.BattleAction.ActionType == ActionType.Damage && result.Target == cmd.Target) {
						if (result.Missed) {
							cmd.Actor.RaiseAttackMissed(cmd.Target);
						}
						else if (result.DamageDealt > 0) {
							cmd.Actor.RaiseAttackHit(cmd.Target, result.DamageDealt);
						}
					}

					try {
						MoveResolved?.Invoke(result);
					}
					catch (System.Exception ex) {
						RaiseBattleEvent(BattleEventType.EventHandlerError, $"MoveResolved handler threw for {cmd.Actor.CombatantName}: {ex}", cmd.Actor);
					}
				}
			}
		}

		private IEnumerator ExecutePlayerAction(Combatant player, List<BattleAction> availableMoves, List<Combatant> validTargets) {
			var action = availableMoves[Random.Range(0, availableMoves.Count)];
			var target = validTargets[Random.Range(0, validTargets.Count)];

			yield return StartCoroutine(ExecuteAction(player, action, target));
		}

		private IEnumerator HandleAITurn(Combatant ai) {
			var validTargets = GetValidTargets(ai).ToList();
			if (validTargets.Count == 0) {
				RaiseBattleEvent(BattleEventType.NoValidTargets, $"No valid targets found for {ai.CombatantName}. Ending turn.", ai);
				yield break;
			}

			if (ai.Moves == null || ai.Moves.Count == 0) {
				RaiseBattleEvent(BattleEventType.NoMovesAvailable, $"{ai.CombatantName} has no moves. Ending turn.", ai);
				yield break;
			}

			var (action, target) = ai.DecideAIAction(validTargets);

			if (action == null || target == null) {
				RaiseBattleEvent(BattleEventType.NoValidTargets, $"AI could not decide action for {ai.CombatantName}. Ending turn.", ai);
				yield break;
			}

			yield return StartCoroutine(ExecuteAction(ai, action, target));
		}

		private IEnumerator ExecuteAction(Combatant actor, BattleAction action, Combatant target) {
			if (action.IsMultiTurn && action.TurnCost > 1) {
				actor.StartMultiTurnAction(action, target);
				RaiseTurnEvent(TurnEventType.MultiTurnStarted, actor, target, action);
			}
			else {
				BattleCommand cmd;
				try {
					cmd = new BattleCommand(actor, action, target);
				}
				catch (System.Exception ex) {
					RaiseBattleEvent(BattleEventType.CommandError, $"Exception constructing BattleCommand for {actor.CombatantName}: {ex}", actor);
					yield break;
				}

				// Raise skill used event for non-basic attacks
				if (action.ActionType != ActionType.Damage || action.ActionName.ToLower() != "attack") {
					actor.RaiseSkillUsed(action);
				}

				// Raise attack started event for damage actions
				if (action.ActionType == ActionType.Damage) {
					actor.RaiseAttackStarted();
					yield return new WaitForSeconds(0.1f); // Small delay for windup animation
				}

				RaiseTurnEvent(TurnEventType.ActionSelected, actor, target, action);

				List<BattleResult> results = null;
				try {
					results = MoveResolver.Resolve(cmd, m_Combatants);
				}
				catch (System.Exception ex) {
					RaiseBattleEvent(BattleEventType.ResolutionError, $"MoveResolver.Resolve threw for {actor.CombatantName}: {ex}", actor);
					yield break;
				}

				if (results != null) {
					foreach (var result in results) {
						// Raise hit/miss events for damage actions
						if (action.ActionType == ActionType.Damage && result.Target == target) {
							if (result.Missed) {
								actor.RaiseAttackMissed(target);
							}
							else if (result.DamageDealt > 0) {
								actor.RaiseAttackHit(target, result.DamageDealt);
							}
						}

						try {
							MoveResolved?.Invoke(result);
						}
						catch (System.Exception ex) {
							RaiseBattleEvent(BattleEventType.EventHandlerError, $"MoveResolved handler threw for {actor.CombatantName}: {ex}", actor);
						}
					}
				}
			}
		}

		private void DisplayTurnOrder() {
			var upcoming = GetUpcomingTurns();
			string queue = string.Join(" -> ", upcoming.Select(c => $"{c.CombatantName} ({c.TurnGauge:0})"));
			RaiseBattleEvent(BattleEventType.TurnOrderUpdated, $"Upcoming Turns: {queue}");
			OnTurnOrderUpdated?.Invoke(upcoming);
		}

		private List<Combatant> GetUpcomingTurns() {
			var tempList = m_Combatants
				.Where(c => c.isAlive)
				.Select(c => new { combatant = c, gauge = c.TurnGauge })
				.ToList();

			List<Combatant> upcoming = new List<Combatant>();

			for (int i = 0; i < m_LookaheadTurns; i++) {
				if (tempList.Count == 0) break;

				var next = tempList.OrderBy(c => (m_GaugeThreshold - c.gauge) / c.combatant.Speed).First();
				upcoming.Add(next.combatant);

				float timeToAct = (m_GaugeThreshold - next.gauge) / next.combatant.Speed;

				for (int j = 0; j < tempList.Count; j++)
					tempList[j] = new { combatant = tempList[j].combatant, gauge = tempList[j].gauge + tempList[j].combatant.Speed * timeToAct };

				int idx = tempList.FindIndex(t => t.combatant == next.combatant);
				tempList[idx] = new { combatant = next.combatant, gauge = tempList[idx].gauge - m_GaugeThreshold };
			}

			return upcoming;
		}
	}

	public class CombatantData {
		public Combatant Combatant { get; set; }
		public int Speed { get; set; }
		public double Step { get; set; }
		public double Position { get; set; }
	}

	public struct PlayerInputData {
		public Combatant Actor;
		public List<BattleAction> AvailableMoves;
		public List<Combatant> ValidTargets;
		public float Timestamp;
	}

	public struct PlayerInputResponse {
		public BattleAction SelectedAction;
		public Combatant SelectedTarget;
		public bool IsValid;
		public string ValidationMessage;
	}

	public enum BattleEventType {
		BattleStarted,
		BattleEnded,
		TurnSkipped,
		NoValidTargets,
		NoMovesAvailable,
		CommandError,
		ResolutionError,
		EventHandlerError,
		BattleEndConditionMet,
		TurnOrderUpdated,
		TargetChanged
	}

	public enum TurnEventType {
		TurnStarted,
		ActionSelected,
		MultiTurnStarted,
		TurnEnded
	}

	public enum BattleOutcome {
		Victory,
		Defeat,
		Undefined
	}

	public struct BattleEventData {
		public BattleEventType EventType;
		public string Message;
		public Combatant Combatant;
		public Combatant Target;
		public float Timestamp;
	}

	public struct TurnEventData {
		public TurnEventType EventType;
		public Combatant Combatant;
		public Combatant Target;
		public BattleAction Action;
		public float Timestamp;
	}

	public interface ITargetProvider {
		IEnumerable<Combatant> GetValidTargets(Combatant actor);
	}
}
