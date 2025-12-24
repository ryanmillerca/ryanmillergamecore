namespace RyanMillerGameCore.Character.SMB
{
    using UnityEngine;
    using UnityEngine.Animations;

    /// <summary>
    /// Applies damage to the target at the moment of the attack.
    /// </summary>
    public class MeleeAttackSMB : CharacterSMB
    {
        [SerializeField, Range(0f,1f)] private float attackMoment;
        
        [ExposeFromCharacterReferences("attackDashForce")]
        public float attackDashForce;

        private bool _attacked;
        private float _stateTime;

        protected override void OnCharacterStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnCharacterStateEnter(animator, stateInfo, layerIndex);
            _attacked = false;
            if (referenceProvider.m_AttackDashForce > 0)
            {
                referenceProvider.Movement.PushForward(referenceProvider.m_AttackDashForce);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,
            AnimatorControllerPlayable controller)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex, controller);
            if (_attacked)
            {
                return;
            }
            if (stateInfo.normalizedTime < attackMoment)
            {
                return;
            }
            PerformAttack();
        }

        private void PerformAttack()
        {
            _attacked = true;
            referenceProvider.DamageDealer.DealDamage(referenceProvider.Character.Transform.gameObject);
        }
    }
}