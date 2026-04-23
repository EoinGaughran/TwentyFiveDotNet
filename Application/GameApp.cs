using TwentyFiveDotNet.Game;
using TwentyFiveDotNet.Interfaces;

namespace TwentyFiveDotNet.Application
{
    public static class GameApp
    {
        public static void Start(GameManager manager, IGameInteraction ui)
        {
            manager.OnGameEnded += () =>
            {
                if (ui.PlayAgain())
                    manager.NewGame();
                else
                    manager.EndGame();
            };

            manager.StartGame();
        }

        public static void Tick(GameManager manager)
        {
            if (!manager.IsGameOver())
            {
                manager.AdvanceGame();
            }
        }
    }
}
