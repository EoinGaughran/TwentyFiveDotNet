using System;
using System.Linq;
using System.Collections.Generic;
using TwentyFiveDotNet.Game;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.ConsoleUI;
using TwentyFiveDotNet.Game.Builders;
using System.Data;

namespace TwentyFiveDotNet
{
    class Program
    {
        public static readonly string UIPrefix = "[Program] ";
        const string filePath = "Config/GameConfig.json";
        static void Main(string[] args)
        {
            GameMode mode = ParseMode(args);

            GameConfig config = ConfigLoader.LoadGameConfig(filePath) ??
                throw new InvalidOperationException("Failed to load configuration.");

            RuntimeSettings runtimeSettings = new()
            {
                HidePlayerHands = config.HidePlayerHands,
                DevMode = config.DevMode,
            };

            if (args.Contains("--dev"))
                runtimeSettings.DevMode = true;

            ConsoleSettings consoleSettings = new()
            {
                DevMode = runtimeSettings.DevMode,
                Delay = config.DelayInMilliseconds
            };

            Console.WriteLine($"DevMode: {runtimeSettings.DevMode}.");

            if (mode != GameMode.Real)
            {
                RunTestMode(mode, config, consoleSettings);
                return;
            }

            RunMainGame(config, consoleSettings);
        }

        static void RunMainGame(GameConfig config, ConsoleSettings consoleSettings)
        {
            Console.WriteLine($"MaxPlayers: {config.MaxPlayers}, Instance: {config.MaxPlayers}");
            Console.WriteLine($"GameTitle: {config.GameTitle}, Instance: {config.GameTitle}");
            Console.WriteLine("Welcome to the card game 25.");
            Console.WriteLine($"The game is for {config.MinPlayers} - {config.MaxPlayers} players.");

            RulesEngine rules = new(config);

            GameManager manager = new(rules, ConsoleGameBuilder.CreateMainGame(rules, config, consoleSettings));
            Run(manager, consoleSettings);
        }

        static void RunTestMode(GameMode mode, GameConfig config, ConsoleSettings consoleSettings)
        {
            Console.WriteLine("Running TEST MODE");

            RulesEngine rules = new(config);

            GameManager manager = new(rules, TestGameBuilder.CreateBasicGame(rules));

            var gameUI = new ConsoleGameInteraction(consoleSettings, manager);

            manager.PublishState();
        }

        static GameMode ParseMode(string[] args)
        {
            if (args.Contains("--test"))
                return GameMode.TestBasic;

            return GameMode.Real;
        }
        static void Run(GameManager manager, ConsoleSettings consoleSettings)
        {
            var gameUI = new ConsoleGameInteraction(consoleSettings, manager);

            manager.OnGameEnded += () =>
            {
                if (gameUI.PlayAgainQuestion("Play again?"))
                    manager.NewGame();
                else
                    manager.EndGame();
            };

            manager.StartGame();

            while (true)
            {
                manager.AdvanceGame();

                if (manager.IsGameOver()) break;
            }
        }
    }
}