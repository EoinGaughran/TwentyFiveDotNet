using System;
using System.Collections.Generic;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.ConsoleUI;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Game.Builders
{
    public static class ConsoleGameBuilder
    {
        public static readonly string UIPrefix = "[Program] ";
        public static GameState CreateMainGame(RulesEngine rulesEngine, GameConfig config, ConsoleSettings consoleSettings)
        {
            GameState gameState = new GameState();

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

            gameState.Players = new List<Player>();

            for (int i = 0; i < totalHumans; i++)
            {
                CustomConsole.Write($"Player {i + 1} enter your name: ", UIPrefix, consoleSettings);
                var readName = CustomConsole.Readline();
                gameState.Players.Add(new PlayerHuman(readName));
            }

            RulesEngine rules = new(config);

            for (int i = totalHumans; i < totalPlayers; i++)
            {
                gameState.Players.Add(new PlayerCPU($"CPU Player {i + 1}", rules));
            }

            gameState.PlayedCards = new List<(Player player, Card card)>();
            gameState.CurrentPlayerIndex = 0;
            gameState.Deck = new Deck();
            gameState.Deck.Add52CardsToDeck();
            gameState.Deck.Shuffle();

            return gameState;
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
