using System;
using System.Collections.Generic;
using System.Threading;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.ConsoleUI
{
    public static class CustomConsole
    {

        public static readonly String DevPrefix = "[DEV LOG]";

        // Wrapper for Console.WriteLine

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
    }
}
