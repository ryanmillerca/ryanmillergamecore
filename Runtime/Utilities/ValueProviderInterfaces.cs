namespace RyanMillerGameCore.Utilities
{
    using System;

    public interface IFloatValueProvider
    {
        float CurrentValue { get; }
        event Action<float> ValueChanged;
    }
}