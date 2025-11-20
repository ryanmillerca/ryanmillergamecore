namespace RyanMillerGameCore.Utilities {
    public interface IPoolable {
        void OnAcquire();
        void OnRelease();
    }
}