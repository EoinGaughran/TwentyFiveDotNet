using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Models;
using TwentyFiveDotNet.Core.Game;

namespace TwentyFiveDotNet.ConsoleApp.Builders
{
    public static class ConsoleGameBuilder
    {
        public static readonly string UIPrefix = "[Program] ";
        public static GameState CreateMainGame(RulesEngine rulesEngine, GameConfig config)
        {
            GameState gameState = new GameState();

            Console.WriteLine($"GameTitle: {config.GameTitle}");
            Console.WriteLine("Welcome to the card game 25.");
            Console.WriteLine($"The game is for {config.MinPlayers} - {config.MaxPlayers} players.");

            int totalPlayers = ReadIntInRange(
                $"How many total players? ({config.MinPlayers}-{config.MaxPlayers}): ",
                config.MinPlayers,
                config.MaxPlayers);

            int totalHumans = ReadIntInRange(
                $"How many Human players? (0-1): ",
                0,
                1);

            gameState.Players = new List<Player>();

            for (int i = 0; i < totalHumans; i++)
            {
                Console.Write($"Player {i + 1} enter your name: ");
                var readName = Console.ReadLine();
                gameState.Players.Add(new PlayerHuman(readName));
            }

            for (int i = totalHumans; i < totalPlayers; i++)
            {
                gameState.Players.Add(new PlayerCPU($"CPU Player {i + 1}", rulesEngine));
            }

            for (int i = 0; i < totalPlayers; i++)
            {
                gameState.Players[i].Id = i;
            }

            gameState.PlayedCards = new List<(Player player, Card card)>();
            gameState.CurrentPlayerIndex = 0;
            gameState.Deck = new Deck();
            gameState.Deck.Add52CardsToDeck();
            gameState.Deck.Shuffle();

            return gameState;
        }

        private static int ReadIntInRange(string prompt, int min, int max)
        {
            int value;
            do
            {
                Console.Write(prompt);
            }
            while (!int.TryParse(Console.ReadLine(), out value) || value < min || value > max);

            return value;
        }
    }
}
