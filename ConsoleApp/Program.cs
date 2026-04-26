using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Models;
using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Application;
using TwentyFiveDotNet.Core.Game.Builders;
using TwentyFiveDotNet.ConsoleApp.ConsoleUI;
using TwentyFiveDotNet.ConsoleApp.Builders;

namespace TwentyFiveDotNet
{
    class Program
    {
        public static readonly string UIPrefix = "[DotNet Program] ";
        const string filePath = "Config/GameConfig.json";
        static void Main(string[] args)
        {
            GameConfig GameConfig = ConfigLoader.LoadGameConfig(filePath) ??
                throw new InvalidOperationException("Failed to load configuration.");

            RuntimeSettings runtimeSettings = new()
            {
                GameMode = ParseMode(args),
                HidePlayerHands = GameConfig.HidePlayerHands,
                DevMode = GameConfig.DevMode,
                Delay = GameConfig.DelayInMilliseconds
            };

            if (args.Contains("--dev"))
                runtimeSettings.DevMode = true;

            RulesEngine rules = new(GameConfig);

            GameState gameState = runtimeSettings.GameMode switch
            {
                GameMode.Main => ConsoleGameBuilder.CreateMainGame(rules, GameConfig),
                GameMode.Snapshot => TestGameBuilder.CreateBasicGame(rules),
                _ => throw new InvalidOperationException($"Unsupported GameMode: {runtimeSettings.GameMode}")
            };

            GameManager manager = new(rules, gameState);
            var ui = new ConsoleGameInteraction(runtimeSettings, manager);

            GameApp.Start(manager, ui);

            while (!manager.IsGameOver())
            {
                GameApp.Tick(manager);
            }
        }

        static GameMode ParseMode(string[] args)
        {
            if (args.Contains("--test"))
                return GameMode.Snapshot;

            return GameMode.Main;
        }
    }
}