using System;
using System.Linq;
using System.Collections.Generic;
using TwentyFiveDotNet.Game;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.Interfaces;
using TwentyFiveDotNet.ConsoleUI;

namespace TwentyFiveDotNet
{
    class Program
    {
        public static readonly string UIPrefix = "[Program] ";
        const string filePath = "Config/GameConfig.json";

        static void Main(string[] args)
        {
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

            CustomConsole.WriteLine($"DevMode: {runtimeSettings.DevMode}.", UIPrefix, consoleSettings);
            CustomConsole.WriteLine($"MaxPlayers: {config.MaxPlayers}, Instance: {config.MaxPlayers}", UIPrefix, consoleSettings);
            CustomConsole.WriteLine($"GameTitle: {config.GameTitle}, Instance: {config.GameTitle}", UIPrefix, consoleSettings);
            CustomConsole.WriteLine("Welcome to the card game 25.", UIPrefix, consoleSettings);
            CustomConsole.WriteLine($"The game is for {config.MinPlayers} - {config.MaxPlayers} players.", UIPrefix, consoleSettings);

            int totalPlayers = ReadIntInRange(
                $"How many total players? ({config.MinPlayers}-{config.MaxPlayers}): ",
                config.MinPlayers,
                config.MaxPlayers,
                UIPrefix,
                consoleSettings);

            int totalHumans = ReadIntInRange(
                $"How many Human players? (0-1): ",
                0,
                1,
                UIPrefix,
                consoleSettings);

            List<Player> Players = [];

            for (int i = 0; i < totalHumans; i++)
            {
                CustomConsole.Write($"Player {i + 1} enter your name: ", UIPrefix, consoleSettings);
                var readName = CustomConsole.Readline();
                Players.Add(new PlayerHuman(readName));
            }

            RulesEngine rules = new(config);

            for (int i = totalHumans; i < totalPlayers; i++)
            {
                Players.Add(new PlayerCPU($"CPU Player {i + 1}", rules));
            }

            GameManager manager = new(rules, Players);
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

        private static int ReadIntInRange(string prompt, int min, int max, string prefix, ConsoleSettings settings)
        {
            int value;
            do
            {
                CustomConsole.Write(prompt, prefix, settings);
            }
            while (!int.TryParse(Console.ReadLine(), out value) || value < min || value > max);

            return value;
        }
    }
}