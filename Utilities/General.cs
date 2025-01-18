using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Utilities
{
    internal class General
    {
        public static void PrintPlayerDictionary(Dictionary<Player, Card> PlayedCards)
        {
            foreach (var entry in PlayedCards)
            {
                Console.WriteLine($"{entry.Key.Name} has a {entry.Value}");
            }
        }

        public static void PrintListOfPlayers(List<Player> players)
        {
            foreach (var player in players)
            {
                Console.WriteLine($"{player.Name} has joined the game.");
            }
        }

        public static void PrintScoresOfCardListBy(List<Card> cards)
        {
            foreach (var card in cards)
            {
                Console.WriteLine($"{card} is worth: {card.Score}");
            }
        }

        public static void PrintTrumpScores(List<Card> deckCards, List<Card> dealtCards)
        {
            List<Card> TrumpList = new List<Card>();

            foreach (var card in deckCards.Concat(dealtCards))
            {
                if (card.IsTrump)
                    TrumpList.Add(card); 
            }

            TrumpList.Sort((x, y) => y.Score.CompareTo(x.Score)); // desc

            foreach (var card in TrumpList)
                Console.WriteLine($"{card} is worth: {card.Score}");
        }

        public static void PrintPlayersHands(List<Player> players)
        {
            foreach (var player in players)
            {
                Console.Write($"{player.Name} has:");

                foreach (var card in player.Hand)
                {
                    Console.Write($" {card},");
                }
                Console.WriteLine();
            }
        }

        public static void PrintPlayersScores(List<Player> players)
        {
            Console.WriteLine($"Current Scores:");

            foreach (var player in players)
            {
                Console.WriteLine($"{player.Name} has {player.Points} points.");
            }
            Console.WriteLine();
        }

        public static void PrintLegalCards(List<Card> hand)
        {
            foreach (var card in hand)
            {
                if (card.Legal)
                {
                    Console.Write($"{card}, ");
                }
            }
        }
    }
}
