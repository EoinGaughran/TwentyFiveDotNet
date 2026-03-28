using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Interfaces;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.ConsoleUI
{
    internal class ConsoleGameInteraction : IGameInteraction
    {
        private readonly ConsoleSettings _settings;
        public static readonly String UIPrefix = "[Game UI] ";

        public ConsoleGameInteraction(ConsoleSettings settings)
        {
            _settings = settings;
        }
        public void ShowMessage(string message)
        {
            Console.WriteLine(UIPrefix + message);
        }

        public string GetInput()
        {
            return Console.ReadLine();
        }

        public void WaitForInput()
        {
            CustomConsole.WriteLine("Enter any key to continue.", _settings);
            Console.ReadKey();
        }
        public void ShowPlayers(List<Player> players)
        {

        }
        public void ShowPlayedCards(Dictionary<Player, Card> cards)
        {

        }

        public void HandleDealingStarted()
        {
            CustomConsole.WriteLine(UIPrefix + "Dealing cards...", _settings);
        }

        public void HandleCardsDealt(Player player, IEnumerable<Card> cards)
        {
            CustomConsole.Write(UIPrefix + $"{player.Name}'s hand:", _settings);
            CustomConsole.WriteLine(string.Join(", ", cards), _settings);
        }
        public void HandleTrumpCard(Card TrumpCard, IEnumerable<Card> deckCards, IEnumerable<Card> dealtCards)
        {
            CustomConsole.Write(UIPrefix + $"Dealer drew the trump card: {TrumpCard}", _settings);
            CustomConsole.WriteLine($"{TrumpCard.GetSuitSymbolUnicoded()} suit is trumps.", _settings);
        }
        public void ShowTrumpCards(Dictionary<Card,int> cards)
        {
            foreach (var kvp in cards)
            {
                Console.WriteLine(UIPrefix + $"{kvp.Key} is worth: {kvp.Value}");
            }
        }
        public void UpdateScores(List<Player> players)
        {
            CustomConsole.PrintPlayersScores(players, _settings, UIPrefix);
        }

        public bool PlayAgainQuestion(String message)
        {
            CustomConsole.WriteLine(UIPrefix + "Would you like to play again? (Y/N)", _settings);
            var charResponse = Console.ReadLine();

            while (true)
            {
                if (charResponse == "y" || charResponse == "Y")
                {
                    CustomConsole.WriteLine(UIPrefix + "You chose to play a new game.", _settings);
                    CustomConsole.WriteLine();
                    return true;
                }
                else if (charResponse == "n" || charResponse == "N")
                {
                    CustomConsole.WriteLine(UIPrefix + "You chose to not play a new game.", _settings);
                    CustomConsole.WriteLine();
                    return false;
                }

                CustomConsole.WriteLine(UIPrefix + "Invalid response, try again.", _settings);
                CustomConsole.WriteLine();
            }

        }
    }
}
