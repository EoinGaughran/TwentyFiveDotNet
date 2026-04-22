using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using TwentyFiveDotNet.Game;
using TwentyFiveDotNet.Interfaces;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.ConsoleUI
{
    internal class ConsoleGameInteraction : IGameInteraction
    {
        private readonly CustomConsole customConsole;
        private readonly GameManager _manager;
        public const string UIPrefix = "[Game UI] ";
        public const string TextUIPrefix = "[TEXT UI] ";
        public const string DEVPrefix = "[DEV UI] ";

        public ConsoleGameInteraction(ConsoleSettings settings, GameManager manager)
        {
            _manager = manager;
            customConsole = new CustomConsole(settings, UIPrefix);

            //Messaging
            _manager.OnMessage += (message) =>
            {
                customConsole.WriteLine(message);
            };

            _manager.OnStateSnapshot += (gameState) =>
            {
                customConsole.PrintSnapShot(gameState);
            };
                
            _manager.OnRelayTrumpInfo += (message) =>
            {
                customConsole.WriteLine(message + " are trumps.");
            };

            _manager.OnRelayTrumpLeadInfo += (line1, line2) =>
            {
                customConsole.WriteLine($"The trump suit is {line1}. The leading suit is {line2}.");
            };

            //Setup
            _manager.OnDeckCreated += (deck) =>
            {
                customConsole.WriteLine($"{deck} with {deck.Cards.Count} cards was created.");
            };
            _manager.OnDeckShuffled += (deck) =>
            {
                customConsole.WriteLine($"{deck} was shuffled.");
            };

            //Dealing
            _manager.OnDealerSelected += (player) =>
            {
                customConsole.WriteLine($"{player.Name} has been selected as the dealer.");
            };

            _manager.OnDealingStarted += () =>
            {
                customConsole.WriteLine("Dealing cards...");
            };

            _manager.OnCardDealtToPlayer += (deck, card, player) =>
            {
                customConsole.WriteLine($"The {card} has been dealt from the {deck} to {player.Name}.");
            };

            //Trump mechanics
            _manager.OnPlayerFlipsTrumpCard += (trumpCard, dealer) =>
            {
                customConsole.WriteLine($"Dealer {dealer.Name} flipped the trump card. It's the {trumpCard}");
            };

            _manager.OnTrumpSuitRevealed += (trumps) =>
            {
                foreach (var kvp in trumps)
                {
                    customConsole.WriteLine(UIPrefix + $"{kvp.Key} is worth: {kvp.Value}");
                }
            };

            _manager.OnTrumpCardIsAceOTrumps += (dealer) =>
            {
                customConsole.WriteLine($"Dealer {dealer.Name} has the Ace of Hearts. They may steal it.");
            };

            _manager.OnCardDiscarded += (card, player) =>
            {
                customConsole.WriteLine($"{player.Name} discarded a card. (It was a {card})");
            };

            //Turn Flow
            _manager.OnLeadPlayerSelected += (player) =>
            {
                customConsole.WriteLine($"{player.Name} is leading the trick.");
            };

            _manager.OnPlayerSteal += (card, player) =>
            {
                customConsole.WriteLine($"{player.Name} stole the trump card {card}.");
            };

            _manager.OnLeadPlayerTurn += (player) =>
            {
                customConsole.WriteLine($"{player.Name} will now lead the trick.");
            };

            _manager.OnPlayerTurnStarted += (player) =>
            {
                customConsole.WriteLine($"It's {player.Name}'s turn.");
            };

            //Card Play
            _manager.OnLeadCardPlayed += (card, player) =>
            {
                customConsole.WriteLine($"{player.Name} led with the {card}. Suit {card.Suit} is leading.");
            };

            _manager.OnCardPlayed += (card, player) =>
            {
                customConsole.WriteLine($"{player.Name} played the {card}");
            };

            _manager.OnPlayerInputRequest += (player, decisionType, options) =>
            {
                customConsole.WriteLine($"PlayerDecisionType: {decisionType}");

                switch (decisionType)
                {
                    case PlayerDecisionType.FlipTrump:

                        customConsole.WriteLine($"{player.Name}, please flip over the trump card.");
                        customConsole.FlipTrumpCard(player.Name);

                        _manager.SubmitPlayerAction(null);

                        break;

                    case PlayerDecisionType.LeadCard:

                        customConsole.ShowPlayableCards(player.Hand);
                        var chosenLeadCard = customConsole.RequestCardChoice(player.Hand.Count);

                        _manager.SubmitPlayerAction(player.Hand[chosenLeadCard]);

                        break;

                    case PlayerDecisionType.StealTrump:

                        customConsole.WriteLine($"{player.Name}, please discard a card to steal the trump card.");
                        customConsole.ShowPlayableCards(player.Hand);

                        var chosenDroppedCard = customConsole.RequestCardChoice(player.Hand.Count);

                        _manager.SubmitPlayerAction(player.Hand[chosenDroppedCard]);

                        break;

                    case PlayerDecisionType.PlayCard:

                        customConsole.ShowCards(player.Hand);
                        customConsole.ShowPlayableCards(options);

                        var chosen = customConsole.RequestCardChoice(player.Hand.Count);

                        _manager.SubmitPlayerAction(options[chosen]);

                        break;
                }

            };

            //Scoring/Rounds
            _manager.OnScoreChanged += (players) =>
            {
                customConsole.PrintPlayersScores(players);
            };

            _manager.OnRoundNewWinner += (winningCard, player) =>
            {
                customConsole.WriteLine($"{player.Name} is currently winning with the {winningCard}");
            };

            _manager.OnDealersTrick += (player) =>
            {
                customConsole.WriteLine($"{player.Name} got his dealer's trick.");
            };

            _manager.OnRoundEnded += (winningCard, player) =>
            {
                customConsole.WriteLine($"{player.Name} won with the {winningCard}");
                customConsole.WriteLine($"{player.Name} has received the trick worth 5 points.");
            };

            _manager.OnNewRound += () =>
            {
                customConsole.WriteLine($"New Round!");
            };

            //Game State
            _manager.OnGameStateChange += (gameState) =>
            {
                customConsole.WriteLine($"The game state has changed to: {gameState}");
            };

            _manager.OnGameOver += (winner) =>
            {
                customConsole.WriteLine($"Game Over!");
            };

            _manager.OnNewGame += () =>
            {
                customConsole.WriteLine($"The deck has been removed.");
                customConsole.WriteLine($"Players hands have been cleared.");
                customConsole.WriteLine($"Players scores have been cleared.");
                customConsole.WriteLine($"The list of played cards has been cleared.");
            };

            _manager.OnProgramClosed += () =>
            {
                customConsole.WriteLine($"The game has ended. The program will now close.");
            };
        }

        public string GetInput()
        {
            return Console.ReadLine();
        }

        public void WaitForInput()
        {
            customConsole.WriteLine("Enter any key to continue.");
            customConsole.WaitForKeyPress();
        }
        public void ShowPlayers(List<Player> players)
        {

        }
        public void ShowPlayedCards(Dictionary<Player, Card> cards)
        {

        }

        public void HandleCardsDealt(Player player)
        {
            customConsole.Write($"{player.Name}'s hand: ");
            customConsole.WriteLine(string.Join(", ", player.Hand));
        }
        public void HandleTrumpCard(Card TrumpCard, Dictionary<Card,int> Trumps)
        {
            customConsole.WriteLine($"Dealer drew the trump card: {TrumpCard}");
            customConsole.WriteLine($"{TrumpCard.GetSuitSymbolUnicoded()} suit is trumps.");

            foreach (var kvp in Trumps)
            {
                Console.WriteLine($"{kvp.Key} is worth: {kvp.Value}");
            }
        }

        public bool PlayAgainQuestion(String message)
        {
            while (true)
            {
                customConsole.WriteLine("Would you like to play again? (Y/N)");
                var charResponse = Console.ReadLine();

                if (charResponse == "y" || charResponse == "Y")
                {
                    customConsole.WriteLine("You chose to play a new game.");
                    customConsole.WriteLine();
                    return true;
                }
                else if (charResponse == "n" || charResponse == "N")
                {
                    customConsole.WriteLine("You chose to not play a new game.");
                    customConsole.WriteLine();
                    return false;
                }

                customConsole.WriteLine("Invalid response, try again.");
                customConsole.WriteLine();
            }

        }
    }
}
