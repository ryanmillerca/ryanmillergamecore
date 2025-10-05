namespace RyanMillerGameCore.Character
{
    using UnityEngine;
    using System;
    using Interactions;
    
    public interface ICharacter
    {
        CharacterID ID();
        void Interact(IInteractive interactive);
        void Reset();
        bool IsPlayer();
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