namespace RyanMillerGameCore.Character
{
    using UnityEngine;
    using System;
    using Interactions;
    
    public interface ICharacter {
        Transform Transform { get; }
        CharacterID ID();
        void Interact(IInteractive interactive);
        void Reset();
        bool IsPlayer();
        public event Action<float> OnReceiveDamage;
        public event Action<float> OnReceiveHeal;
        public event Action<Character> OnDied;
        public event Action<Vector3> OnKnockedBack;
        public event Action Died;
        public event Action Spawned;
    }
    
    public interface IMovable
    {
        void Move(Vector3 input);
        void Teleport(Vector3 position);
        void CanMove(bool canMove);
        void LookAt(Vector3 point);
        void PushForward(float amount, bool resetForceFirst = true);
        Vector3 Position();
        void ApplyMovement();
    }

    public interface ITakesDamage
    {
        bool CanReceiveDamage();
        bool ReceiveDamage(float damageAmount, Component attacker = null);
        bool ReceiveKnockback(Vector3 direction);
        event Action Died;
        event Action Spawned;
    }
}