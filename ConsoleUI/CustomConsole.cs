using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Utilities
{
    public static class CustomConsole
    {

        public static readonly String DevPrefix = "[DEV LOG]";

        // Wrapper for Console.WriteLine
        public static void WriteLine(string message, int delay, ConsoleSettings settings)
        {
            Console.WriteLine(message);

            if (!settings.DevMode)
            {
                Thread.Sleep(delay); // Introduce delay
            }
        }

        public static void WriteLine(string message, ConsoleSettings settings)
        {
            Console.WriteLine(message);

            if (!settings.DevMode)
            {
                Thread.Sleep(settings.Delay); // Introduce delay
            }
        }

        public static void WriteLineNoDelay(string message)
        {
            Console.WriteLine(message);
        }

        public static void DevWriteLine(string message, ConsoleSettings settings)
        {
            if (settings.DevMode)
            {
                Console.WriteLine($"{DevPrefix} {message}");
                Thread.Sleep(settings.Delay); // Introduce delay
            }
        }
        public static void DevWriteLineNoDelay(string message, ConsoleSettings settings)
        {
            if (settings.DevMode)
            {
                Console.WriteLine($"{DevPrefix} {message}");
            }
        }
        public static void DevWriteLineNoDelayNoPrefix(string message, ConsoleSettings settings)
        {
            if (settings.DevMode)
            {
                Console.WriteLine(message);
            }
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static void DevWriteLine(ConsoleSettings settings)
        {
            if (settings.DevMode)
            {
                Console.WriteLine();
            }
        }

        // Wrapper for Console.Write
        public static void Write(string message, ConsoleSettings settings)
        {
            Console.Write(message);

            if (!settings.DevMode)
            {
                Thread.Sleep(settings.Delay); // Introduce delay
            }
        }

        public static void DevWriteNoDelay(string message, ConsoleSettings settings)
        {
            if (settings.DevMode)
            {
                Console.Write(message);
            }
            
        }

        public static void WaitForKeyPress()
        {
            Console.ReadKey(true); // Waits for a key press, but doesn't display the key on the console
        }

        public static void Clear()
        {
            Console.Clear();
        }

        public static void DevTagPrint(ConsoleSettings settings)
        {
            if (settings.DevMode) Console.Write($"{DevPrefix} ");
        }

        public static void PrintListOfPlayers(List<Player> players, ConsoleSettings settings)
        {
            foreach (var player in players)
            {
                WriteLine($"{player.Name} has joined the game.", settings);
            }
        }

        public static void PrintTrumpScores(List<Card> deckCards, List<Card> dealtCards, ConsoleSettings settings)
        {
            List<Card> TrumpList = [];

            foreach (var card in deckCards)
            {
                if (card.IsTrump)
                    TrumpList.Add(card);
            }

            foreach (var card in dealtCards)
            {
                if (card.IsTrump)
                    TrumpList.Add(card);
            }

            TrumpList.Sort((x, y) => y.Score.CompareTo(x.Score)); // desc

            foreach (var card in TrumpList)
                DevWriteLineNoDelay($"{card} is worth: {card.Score}", settings);

            DevWriteLine(settings);
        }

        public static void PrintPlayersHands(List<Player> players, ConsoleSettings settings)
        {
            foreach (var player in players)
            {
                DevTagPrint(settings);

                DevWriteNoDelay($"{player.Name} has:", settings);

                foreach (var card in player.Hand)
                {
                    DevWriteNoDelay($" {card},", settings);
                }
                DevWriteLine(settings);
            }
        }

        public static void PrintPlayersScores(List<Player> players, ConsoleSettings settings)
        {
            WriteLine($"Current Scores:", settings);

            foreach (var player in players)
            {
                WriteLine($"{player.Name} has {player.Points} points.", 200, settings);
            }
            WriteLine();
        }

        public static void PrintLegalCards(List<Card> hand, ConsoleSettings settings)
        {
            foreach (var card in hand)
            {
                if (card.Legal)
                {
                    Write($"{card}, ", settings);
                }
            }
        }

        public static void PrintPlayedCards(Dictionary<Player, Card> PlayedCards)
        {
            foreach (KeyValuePair<Player, Card> card in PlayedCards)
            {
                WriteLineNoDelay($"{card.Key} played {card.Value}");
            }
        }
    }
}
