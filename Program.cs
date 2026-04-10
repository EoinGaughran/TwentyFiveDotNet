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
    public enum Suits
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades,
    }

    public enum Ranks
    {
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    public enum GameState
    {
        NotStarted,
        Initialize,
        DealCards,
        LeadTurn,
        PlayerTurn,
        Scoring,
        NewRound,
        AwaitingReplayDecision,
        NewGame,
        EndGame
    }

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
                $"How many Human players? (0-{totalPlayers}): ",
                0,
                totalPlayers,
                UIPrefix,
                consoleSettings);

            if (totalHumans > 1)
            {
                runtimeSettings.HidePlayerHands = true;
                CustomConsole.WriteLine($"Amount of humans is greater than 1, enabling hidden player hands mode.", UIPrefix, consoleSettings);
            }
            else runtimeSettings.HidePlayerHands = false;

            List<Player> Players = [];
            IPlayerInteraction iPlayerInteraction = new ConsoleInteraction(consoleSettings);

            for (int i = 0; i < totalHumans; i++)
            {
                CustomConsole.Write($"Player {i + 1} enter your name: ", UIPrefix, consoleSettings);
                var readName = CustomConsole.Readline();
                Players.Add(new PlayerHuman(readName, iPlayerInteraction));
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