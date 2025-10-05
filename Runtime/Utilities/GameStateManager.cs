namespace RyanMillerGameCore
{
    using UnityEngine;

    public class GameStateManager : Singleton<GameStateManager>
    {
        [SerializeField] private GameState currentGameState;
        public event System.Action<GameState> OnGameStateChanged;
        
        public GameState CurrentGameState { get { return currentGameState; } }
        
        public void ChangeGameState(GameState newState)
        {
            if (newState == currentGameState)
            {
                return;
            }
            
            if (currentGameState == GameState.Gameplay && newState == GameState.Paused)
            {
                WasPaused(true);
            }
            if (currentGameState == GameState.Paused && newState == GameState.Gameplay)
            {
                WasPaused(false);
            }
            
            currentGameState = newState;
            OnGameStateChanged?.Invoke(currentGameState);
        }
        
        public void TogglePause()
        {
            switch (currentGameState)
            {
                case GameState.Gameplay:
                    ChangeGameState(GameState.Paused);
                    break;
                case GameState.Paused:
                    ChangeGameState(GameState.Gameplay);
                    break;
            }
        }

        private void WasPaused(bool isPaused)
        {
            Time.timeScale = isPaused ? 0 : 1;
        }
    }

    public enum GameState
    {
        None = 0,
        Loading = 1,
        Menus = 2,
        Gameplay = 3,
        Paused = 4
    }
}