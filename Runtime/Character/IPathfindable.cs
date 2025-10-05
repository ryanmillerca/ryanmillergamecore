namespace RyanMillerGameCore.Character
{
    using UnityEngine;
    
    public interface IPathfindable
    {
        void SetTarget(Vector3 target);
        void PathUpdate();
        void Stop();
        void CalculatePath();
        bool HasPath { get; }
    }
}