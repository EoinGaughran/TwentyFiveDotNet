using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Utilities
{
    public class General
    {
        public static void PrintPlayerDictionary(Dictionary<Player, Card> PlayedCards)
        {
            foreach (var entry in PlayedCards)
            {
                CustomConsole.WriteLine($"{entry.Key.Name} has a {entry.Value}");
            }
        }

        public static void PrintListOfPlayers(List<Player> players)
        {
            foreach (var player in players)
            {
                CustomConsole.WriteLine($"{player.Name} has joined the game.");
            }
        }

        public static void PrintScoresOfCardListBy(List<Card> cards)
        {
            foreach (var card in cards)
            {
                CustomConsole.WriteLine($"{card} is worth: {card.Score}");
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
                CustomConsole.DevWriteLineNoDelay($"{card} is worth: {card.Score}");

            CustomConsole.DevWriteLine();
        }

        public static void PrintPlayersHands(List<Player> players)
        {
            foreach (var player in players)
            {
                CustomConsole.DevTagPrint();

                CustomConsole.DevWriteNoDelay($"{player.Name} has:");

                foreach (var card in player.Hand)
                {
                    CustomConsole.DevWriteNoDelay($" {card},");
                }
                CustomConsole.DevWriteLine();
            }
        }

        public static void PrintPlayersScores(List<Player> players)
        {
            CustomConsole.WriteLine($"Current Scores:");

            foreach (var player in players)
            {
                CustomConsole.WriteLine($"{player.Name} has {player.Points} points.", 200);
            }
            CustomConsole.WriteLine();
        }

        public static void PrintLegalCards(List<Card> hand)
        {
            foreach (var card in hand)
            {
                if (card.Legal)
                {
                    CustomConsole.Write($"{card}, ");
                }
            }
        }

        public static void PrintPlayedCards(Dictionary<Player, Card> PlayedCards)
        {
            foreach (KeyValuePair<Player, Card> card in PlayedCards)
            {
                CustomConsole.WriteLineNoDelay($"{card.Key} played {card.Value}");
            }
        }
    }
}
