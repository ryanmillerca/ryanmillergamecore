namespace RyanMillerGameCore.TurnBasedBattle {

    public class BattleCommand {
        public readonly Combatant Actor;
        public readonly MoveDefSo Move;
        public readonly Combatant Target;

        public BattleCommand(Combatant actor, MoveDefSo move, Combatant target) {
            this.Actor = actor;
            this.Move = move;
            this.Target = target;
        }
    }
}