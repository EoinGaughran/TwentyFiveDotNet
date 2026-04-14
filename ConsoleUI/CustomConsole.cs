using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.ConsoleUI
{
    public static class CustomConsole
    {

        public static readonly String DevPrefix = "[DEV LOG]";

        // Wrapper for Console.WriteLine

        public static string Readline()
        {
            return Console.ReadLine();
        }

        public static void WriteLine(string message, string sourcePrefix, ConsoleSettings settings)
        {
            Console.WriteLine(sourcePrefix + message);

            if (!settings.DevMode)
            {
                Thread.Sleep(settings.Delay); // Introduce delay
            }
        }

        public static void WriteLineNoDelay(string message)
        {
            Console.WriteLine(message);
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

        public static void Write(string message, string sourcePrefix, ConsoleSettings settings)
        {
            Console.Write(sourcePrefix + message);

            if (!settings.DevMode)
            {
                Thread.Sleep(settings.Delay); // Introduce delay
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

        public static void PrintListOfPlayers(List<Player> players, string sourcePrefix, ConsoleSettings settings)
        {
            foreach (var player in players)
            {
                WriteLine($"{player.Name} has joined the game.", sourcePrefix, settings);
            }
        }

        public static void PrintPlayersHands(List<Player> players, ConsoleSettings settings, String prefix)
        {
            foreach (var player in players)
            {
                DevTagPrint(settings);

                Write(prefix + $"{player.Name} has:", settings);

                foreach (var card in player.Hand)
                {
                    Write(prefix + $" {card},", settings);
                }
                DevWriteLine(settings);
            }
        }

        public static void PrintPlayersScores(List<Player> players, String sourcePrefix, ConsoleSettings settings)
        {
            WriteLine($"Current Scores:", sourcePrefix, settings);

            foreach (var player in players)
            {
                WriteLine($"{player.Name} has {player.Points} points.", sourcePrefix, settings);
            }
            WriteLine();
        }

        public static void PrintCards(List<Card> hand, ConsoleSettings settings)
        {
            foreach (var card in hand)
            {   
                 Write($"{card}, ", settings);
            }
        }

        public static void PrintPlayedCards(Dictionary<Player, Card> PlayedCards)
        {
            foreach (KeyValuePair<Player, Card> card in PlayedCards)
            {
                WriteLineNoDelay($"{card.Key} played {card.Value}");
            }
        }

        public static int RequestCardChoice(int max, string sourcePrefix, ConsoleSettings settings)
        {
            CustomConsole.Write("Enter your choice: ", sourcePrefix, settings);

            int choice;

            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > max)
            {
                CustomConsole.WriteLine("Invalid choice, try again.", sourcePrefix, settings);
            }

            return choice - 1;
        }

        public static void ShowCards(IEnumerable<Card> cards, string sourcePrefix, ConsoleSettings settings)
        {
            CustomConsole.Write(sourcePrefix, settings);
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

        public static void ShowPlayableCards(IEnumerable<Card> legalCards, string sourcePrefix, ConsoleSettings settings)
        {
            CustomConsole.Write($"Playable cards: ", sourcePrefix, settings);
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

        public static void WaitForPlayer(string playerName, string sourcePrefix, ConsoleSettings settings)
        {
            CustomConsole.WriteLine($"{playerName}, press any key when ready.", sourcePrefix, settings);
            Console.ReadKey(true);
        }

        public static void FlipTrumpCard(string playerName, string sourcePrefix, ConsoleSettings settings)
        {
            WaitForPlayer(playerName, sourcePrefix, settings);
        }
    }
}
