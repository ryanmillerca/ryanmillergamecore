namespace RyanMillerGameCore.TurnBasedCombat {
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using System;
	using Random = UnityEngine.Random;

	public class BattleManager : MonoBehaviour, ITargetProvider {

		public List<Combatant> Combatants {
			get {
				return m_Combatants;
			}
		}

		[SerializeField] private List<Combatant> m_Combatants = new List<Combatant>();
		[SerializeField] private float m_GaugeThreshold = 100f;
		[SerializeField] private float m_TickRate = 1f;
		[SerializeField] private int m_LookaheadTurns = 5;
		[SerializeField] private float m_TurnDelay = 0.5f;

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
			InitializeCombatants();
			StartCoroutine(BattleLoop());
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

			RaiseTurnEvent(TurnEventType.TurnEnded, c);
			yield return new WaitForSeconds(m_TurnDelay);
		}


		private IEnumerator HandlePlayerTurn(Combatant player) {
			var availableMoves = player.Moves?.Where(m => m != null).ToList() ?? new List<BattleAction>();
			// NOTE: call .ToList() to keep the rest of the code (which uses .Count) intact
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

			// set waiting state and current actor/targets
			m_WaitingForPlayerInput = true;
			m_CurrentPlayerActor = player;
			m_CurrentValidTargets = validTargets;
			m_PendingPlayerCommand = null; // clear any previous pending command

			// Fire both legacy and new style events so UI can subscribe either way
			PlayerInputRequired?.Invoke(inputData);
			OnPlayerActionRequested?.Invoke(player, availableMoves);

			// Wait until the UI submits a command (SubmitPlayerCommand) or uses the legacy SubmitPlayerInput
			yield return new WaitUntil(() => !m_WaitingForPlayerInput);

			// If the UI submitted a BattleCommand, execute it; otherwise fallback to random selection
			if (m_PendingPlayerCommand != null) {
				yield return StartCoroutine(ExecuteSubmittedCommand(m_PendingPlayerCommand));
				m_PendingPlayerCommand = null;
			}
			else {
				yield return StartCoroutine(ExecutePlayerAction(player, availableMoves, validTargets));
			}

			// clear current actor/targets
			m_CurrentPlayerActor = null;
			m_CurrentValidTargets = null;
		}

		private IEnumerator ExecuteSubmittedCommand(BattleCommand cmd) {
			if (cmd == null) yield break;

			// Validate the command a little
			if (cmd.Actor == null || cmd.BattleAction == null || cmd.Target == null) {
				RaiseBattleEvent(BattleEventType.CommandError, $"Submitted command invalid for {cmd.Actor?.CombatantName ?? "null actor"}", cmd.Actor, cmd.Target);
				yield break;
			}

			// Fire action selected
			RaiseTurnEvent(TurnEventType.ActionSelected, cmd.Actor, cmd.Target, cmd.BattleAction);

			List<BattleResult> results = null;
			try {
				results = MoveResolver.Resolve(cmd, m_Combatants);
			}
			catch (System.Exception ex) {
				RaiseBattleEvent(BattleEventType.ResolutionError, $"MoveResolver.Resolve threw for {cmd.Actor.CombatantName}: {ex}", cmd.Actor);
				yield break;
			}

			if (results != null) {
				foreach (var result in results) {
					try {
						MoveResolved?.Invoke(result);
					}
					catch (System.Exception ex) {
						RaiseBattleEvent(BattleEventType.EventHandlerError, $"MoveResolved handler threw for {cmd.Actor.CombatantName}: {ex}", cmd.Actor);
					}
				}
			}

			yield return null;
		}

		private IEnumerator ExecutePlayerAction(Combatant player, List<BattleAction> availableMoves, List<Combatant> validTargets) {
			// fallback/random selection for when no player command was submitted
			var action = availableMoves[Random.Range(0, availableMoves.Count)];
			var target = validTargets[Random.Range(0, validTargets.Count)];

			yield return StartCoroutine(ExecuteAction(player, action, target));
		}

		private IEnumerator HandleAITurn(Combatant ai) {
			// call GetValidTargets and convert to List so rest of code works unchanged
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
