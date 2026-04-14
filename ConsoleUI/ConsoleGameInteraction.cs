using System;
using System.Collections.Generic;
using TwentyFiveDotNet.Game;
using TwentyFiveDotNet.Interfaces;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.ConsoleUI
{
    internal class ConsoleGameInteraction : IGameInteraction
    {
        private readonly ConsoleSettings _settings;
        private readonly GameManager _manager;
        public const String UIPrefix = "[Game UI] ";
        public const String TextUIPrefix = "[TEXT UI] ";
        public const String DEVPrefix = "[DEV UI] ";

        public ConsoleGameInteraction(ConsoleSettings settings, GameManager manager)
        {
            _settings = settings;
            _manager = manager;

            //Messaging
            _manager.OnMessage += (message) =>
            {
                CustomConsole.WriteLine(message, TextUIPrefix, _settings);
            };

            _manager.OnRelayTrumpInfo += (message) =>
            {
                CustomConsole.WriteLine(message + " are trumps.", TextUIPrefix, _settings);
            };

            _manager.OnRelayTrumpLeadInfo += (line1, line2) =>
            {
                CustomConsole.WriteLine($"The trump suit is {line1}. The leading suit is {line2}.", TextUIPrefix, _settings);
            };

            //Setup
            _manager.OnDeckCreated += (deck) =>
            {
                CustomConsole.WriteLine($"{deck} with {deck.Cards.Count} cards was created.", UIPrefix, _settings);
            };
            _manager.OnDeckShuffled += (deck) =>
            {
                CustomConsole.WriteLine($"{deck} was shuffled.", UIPrefix, _settings);
            };

            //Dealing
            _manager.OnDealerSelected += (player) =>
            {
                CustomConsole.WriteLine($"{player.Name} has been selected as the dealer.", UIPrefix, _settings);
            };

            _manager.OnDealingStarted += () =>
            {
                CustomConsole.WriteLine("Dealing cards...", UIPrefix, _settings);
            };

            _manager.OnCardDealtToPlayer += (deck, card, player) =>
            {
                CustomConsole.WriteLine($"The {card} has been dealt from the {deck} to {player.Name}.", UIPrefix, _settings);
            };

            //Trump mechanics
            _manager.OnPlayerFlipsTrumpCard += (trumpCard, dealer) =>
            {
                CustomConsole.WriteLine($"Dealer {dealer.Name} flipped the trump card. It's the {trumpCard}", UIPrefix, _settings);
            };

            _manager.OnTrumpSuitRevealed += (trumps) =>
            {
                foreach (var kvp in trumps)
                {
                    Console.WriteLine(UIPrefix + $"{kvp.Key} is worth: {kvp.Value}");
                }
            };

            _manager.OnTrumpCardIsAceOTrumps += (dealer) =>
            {
                CustomConsole.WriteLine($"Dealer {dealer.Name} has the Ace of Hearts. They may steal it.", UIPrefix, _settings);
            };

            _manager.OnCardDiscarded += (card, player) =>
            {
                CustomConsole.WriteLine($"{player.Name} discarded a card. (It was a {card})", UIPrefix, _settings);
            };

            //Turn Flow
            _manager.OnLeadPlayerSelected += (player) =>
            {
                CustomConsole.WriteLine($"{player.Name} is leading the trick.", UIPrefix, _settings);
            };

            _manager.OnPlayerSteal += (card, player) =>
            {
                CustomConsole.WriteLine($"{player.Name} stole the trump card {card}.", UIPrefix, _settings);
            };

            _manager.OnLeadPlayerTurn += (player) =>
            {
                CustomConsole.WriteLine($"{player.Name} will now lead the trick.", UIPrefix, _settings);
            };

            _manager.OnPlayerTurnStarted += (player) =>
            {
                CustomConsole.WriteLine($"It's {player.Name}'s turn.", UIPrefix, _settings);
            };

            //Card Play
            _manager.OnLeadCardPlayed += (card, player) =>
            {
                CustomConsole.WriteLine($"{player.Name} led with the {card}. Suit {card.Suit} is leading.", UIPrefix, _settings);
            };

            _manager.OnCardPlayed += (card, player) =>
            {
                CustomConsole.WriteLine($"{player.Name} played the {card}", UIPrefix, _settings);
            };

            _manager.OnPlayerInputRequest += (player, decisionType, options) =>
            {
                CustomConsole.WriteLine($"PlayerDecisionType: {decisionType}", UIPrefix, _settings);

                switch (decisionType)
                {
                    case PlayerDecisionType.FlipTrump:

                        CustomConsole.WriteLine($"{player.Name}, please flip over the trump card.", UIPrefix, _settings);
                        CustomConsole.FlipTrumpCard(player.Name, UIPrefix, _settings);

                        _manager.SubmitPlayerAction(null);

                        break;

                    case PlayerDecisionType.LeadCard:

                        CustomConsole.ShowPlayableCards(player.Hand, UIPrefix, _settings);
                        var chosenLeadCard = CustomConsole.RequestCardChoice(player.Hand.Count, UIPrefix, _settings);

                        _manager.SubmitPlayerAction(player.Hand[chosenLeadCard]);

                        break;

                    case PlayerDecisionType.StealTrump:

                        CustomConsole.WriteLine($"{player.Name}, please discard a card to steal the trump card.", UIPrefix, _settings);
                        CustomConsole.ShowPlayableCards(player.Hand, UIPrefix, _settings);

                        var chosenDroppedCard = CustomConsole.RequestCardChoice(player.Hand.Count, UIPrefix, _settings);

                        _manager.SubmitPlayerAction(player.Hand[chosenDroppedCard]);

                        break;

                    case PlayerDecisionType.PlayCard:

                        CustomConsole.ShowCards(player.Hand, UIPrefix, _settings);
                        CustomConsole.ShowPlayableCards(options, UIPrefix, _settings);

                        var chosen = CustomConsole.RequestCardChoice(player.Hand.Count, UIPrefix, _settings);

                        _manager.SubmitPlayerAction(options[chosen]);

                        break;
                }

            };

            //Scoring/Rounds
            _manager.OnScoreChanged += (players) =>
            {
                CustomConsole.PrintPlayersScores(players, UIPrefix, _settings);
            };

            _manager.OnRoundNewWinner += (winningCard, player) =>
            {
                CustomConsole.WriteLine($"{player.Name} is currently winning with the {winningCard}", UIPrefix, _settings);
            };

            _manager.OnDealersTrick += (player) =>
            {
                CustomConsole.WriteLine($"{player.Name} got his dealer's trick.", UIPrefix, _settings);
            };

            _manager.OnRoundEnded += (winningCard, player) =>
            {
                CustomConsole.WriteLine($"{player.Name} won with the {winningCard}", UIPrefix, _settings);
                CustomConsole.WriteLine($"{player.Name} has received the trick worth 5 points.", UIPrefix, _settings);
            };

            _manager.OnNewRound += () =>
            {
                CustomConsole.WriteLine($"New Round!", UIPrefix, _settings);
            };

            //Game State
            _manager.OnGameStateChange += (gameState) =>
            {
                CustomConsole.WriteLine($"The game state has changed to: {gameState}", UIPrefix, _settings);
            };

            _manager.OnGameOver += (winner) =>
            {
                CustomConsole.WriteLine($"Game Over!", UIPrefix, _settings);
            };

            _manager.OnNewGame += () =>
            {
                CustomConsole.WriteLine($"The deck has been removed.", UIPrefix, _settings);
                CustomConsole.WriteLine($"Players hands have been cleared.", UIPrefix, _settings);
                CustomConsole.WriteLine($"Players scores have been cleared.", UIPrefix, _settings);
                CustomConsole.WriteLine($"The list of played cards has been cleared.", UIPrefix, _settings);
            };

            _manager.OnProgramClosed += () =>
            {
                CustomConsole.WriteLine($"The game has ended. The program will now close.", UIPrefix, _settings);
            };
        }

        public string GetInput()
        {
            return Console.ReadLine();
        }

        public void WaitForInput()
        {
            CustomConsole.WriteLine("Enter any key to continue.", UIPrefix, _settings);
            Console.ReadKey();
        }
        public void ShowPlayers(List<Player> players)
        {

        }
        public void ShowPlayedCards(Dictionary<Player, Card> cards)
        {

        }

        public void HandleCardsDealt(Player player)
        {
            CustomConsole.Write($"{player.Name}'s hand: ", DEVPrefix,  _settings);
            CustomConsole.WriteLine(string.Join(", ", player.Hand), UIPrefix, _settings);
        }
        public void HandleTrumpCard(Card TrumpCard, Dictionary<Card,int> Trumps)
        {
            CustomConsole.WriteLine($"Dealer drew the trump card: {TrumpCard}", UIPrefix, _settings);
            CustomConsole.WriteLine($"{TrumpCard.GetSuitSymbolUnicoded()} suit is trumps.", UIPrefix, _settings);

            foreach (var kvp in Trumps)
            {
                Console.WriteLine($"{kvp.Key} is worth: {kvp.Value}", UIPrefix, _settings);
            }
        }

        public bool PlayAgainQuestion(String message)
        {
            while (true)
            {
                CustomConsole.WriteLine("Would you like to play again? (Y/N)", UIPrefix, _settings);
                var charResponse = Console.ReadLine();

                if (charResponse == "y" || charResponse == "Y")
                {
                    CustomConsole.WriteLine("You chose to play a new game.", UIPrefix, _settings);
                    CustomConsole.WriteLine();
                    return true;
                }
                else if (charResponse == "n" || charResponse == "N")
                {
                    CustomConsole.WriteLine("You chose to not play a new game.", UIPrefix, _settings);
                    CustomConsole.WriteLine();
                    return false;
                }

                CustomConsole.WriteLine("Invalid response, try again.", UIPrefix, _settings);
                CustomConsole.WriteLine();
            }

        }
    }
}
