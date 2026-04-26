using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Models;

namespace TwentyFiveDotNet.ConsoleApp.ConsoleUI
{
    public class CustomConsole
    {
        private readonly RuntimeSettings _settings;
        private readonly string _prefix;
        private readonly string DevPrefix = "[DevMode] ";

        public CustomConsole(RuntimeSettings runtimeSettings, string prefix)
        {
            _settings = runtimeSettings;
            _prefix = prefix;
        }

        // Wrapper for Console.WriteLine
        private void ApplyDelay()
        {
            if (!_settings.DevMode)
                Thread.Sleep(_settings.Delay);
        }

        public string Readline()
        {
            return Console.ReadLine();
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(_prefix + message);

            ApplyDelay();
        }

        public void WriteLineNoDelay(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        // Wrapper for Console.Write
        public void Write(string message)
        {
            Console.Write(message);

            ApplyDelay();
        }

        public void WriteWithPrefix()
        {
            Console.Write(_prefix);

            ApplyDelay();
        }

        public void WaitForKeyPress()
        {
            Console.ReadKey(true); // Waits for a key press, but doesn't display the key on the console
        }

        public void Clear()
        {
            Console.Clear();
        }

        public void PrintListOfPlayers(List<Player> players)
        {
            foreach (var player in players)
            {
                WriteLine($"{player.Name} has joined the game.");
            }
        }

        public void PrintPlayersHands(List<Player> players)
        {
            foreach (var player in players)
            {
                if (_settings.DevMode)
                    Console.Write($"{DevPrefix}");

                Write($"{player.Name} has:");

                foreach (var card in player.Hand)
                {
                    Write($" {card},");
                }

                WriteLine();
            }
        }

        public void PrintPlayersScores(List<Player> players)
        {
            WriteLine($"Current Scores:");

            foreach (var player in players)
            {
                WriteLine($"{player.Name} has {player.Points} points.");
            }
            WriteLine();
        }

        public void PrintCards(List<Card> hand)
        {
            foreach (var card in hand)
            {   
                 Write($"{card}, ");
            }
        }

        public void PrintPlayedCards(List<(Player player, Card card)> PlayedCards)
        {
            if (PlayedCards.Count == 0)
                WriteLine("No cards have been played yet.");

            else
            {
                foreach (var entry in PlayedCards)
                {
                    WriteLine($"{entry.player.Name} played {entry.card.Rank} of {entry.card.Suit}");
                }
            }
        }

        public int RequestCardChoice(int max)
        {
            Write("Enter your choice: ");

            int choice;

            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > max)
            {
                WriteLine("Invalid choice, try again.");
            }

            return choice - 1;
        }

        public void ShowCards(IEnumerable<Card> cards)
        {
            WriteWithPrefix();
            Console.Write($"Players hand: ");
            int i = 1;
            foreach (var card in cards)
            {
                Console.Write($"{card}");
                if (i < cards.Count())
                    Console.Write(", ");

                else Console.Write(".");
                i++;
            }
            Console.WriteLine();

        }

        public void ShowPlayableCards(IEnumerable<Card> legalCards)
        {
            Write($"Playable cards: ");
            int i = 1;
            foreach (var card in legalCards)
            {
                Console.Write($"{i}: {card}");
                if (i < legalCards.Count())
                    Console.Write(", ");

                else Console.Write(".");
                i++;
            }
            Console.WriteLine();
        }

        public void WaitForPlayer(string playerName)
        {
            WriteLine($"{playerName}, press any key when ready.");
            Console.ReadKey(true);
        }

        public void FlipTrumpCard(string playerName)
        {
            WaitForPlayer(playerName);
        }

        public void PrintSnapShot(GameState gameState)
        {
            WriteLine($"GamePhase: {gameState.CurrentPhase}");
            PrintListOfPlayers(gameState.Players);
            PrintPlayersHands(gameState.Players);
            PrintPlayedCards(gameState.PlayedCards);
            PrintPlayersScores(gameState.Players);
        }
    }
}
