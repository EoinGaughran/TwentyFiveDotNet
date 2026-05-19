using System.Numerics;
using Core.Models;
using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Interfaces;
using TwentyFiveDotNet.Core.Models;

namespace TwentyFiveDotNet.ConsoleApp.ConsoleUI
{
    public class ConsoleGameInteraction : IGameInteraction
    {
        private readonly CustomConsole customConsole;
        private readonly GameManager _manager;
        public const string UIPrefix = "[Game UI] ";
        public const string TextUIPrefix = "[TEXT UI] ";
        public const string DEVPrefix = "[DEV UI] ";

        public ConsoleGameInteraction(RuntimeSettings runtimeSettings, GameManager manager)
        {
            _manager = manager;
            customConsole = new CustomConsole(runtimeSettings, UIPrefix);

            //Messaging

            _manager.OnStateSnapshot += (gameState) =>
            {
                customConsole.PrintSnapShot(gameState);
            };

            _manager.OnDealingCompleted += (gameState) =>
            {
                customConsole.WriteLine($"{gameState.Deck} was created.");
                customConsole.WriteLine($"{gameState.Deck} was shuffled.");
                customConsole.WriteLine("Dealing cards...");

                customConsole.PrintPlayersHands(gameState.GetPlayersOrThrow());

                customConsole.WriteLine($"{gameState.Deck.Cards.Count} cards remain in {gameState.Deck}");
            };

            _manager.OnTrumpResolved += (trumpData, dealer, deck) =>
            {
                customConsole.WriteLine($"Dealer {dealer.Name} flipped the trump card. It's the {trumpData._trumpCard}");

                foreach (var kvp in trumpData._trumpCards)
                {
                    customConsole.WriteLine($"{kvp.Key} is worth: {kvp.Value}");
                }

                customConsole.WriteLine($"{deck} has {deck.Cards.Count} cards remaining.");

                //customConsole.WriteLine($"Dealer {dealer.Name} has the Ace of Trumps. They may steal it.");
            };

            //Dealing
            _manager.OnRolesSelected += (dealer, leader) =>
            {
                customConsole.WriteLine($"{dealer} has been selected as the dealer.");
                customConsole.WriteLine($"{leader} is leading the trick.");
            };

            _manager.OnCardDiscarded += (card, player) =>
            {
                customConsole.WriteLine($"{player.Name} discarded a card. (It was a {card})");
            };

            //Turn Flow

            _manager.OnPlayerSteal += (card, player) =>
            {
                customConsole.WriteLine($"{player.Name} stole the trump card {card}.");
            };

            _manager.OnPlayerTurnStarted += (player) =>
            {
                customConsole.WriteLine($"It's {player.Name}'s turn.");
            };

            //Card Play
            _manager.OnCardPlayed += (cardPlayedEvent) =>
            {
                if(cardPlayedEvent.IsLeader)
                    customConsole.WriteLine($"{cardPlayedEvent.Player} led with the {cardPlayedEvent.PlayedCard}. Suit {cardPlayedEvent.PlayedCard.GetSuitSymbolUnicoded()} is leading.");
                else
                    customConsole.WriteLine($"{cardPlayedEvent.Player.Name} played the {cardPlayedEvent.PlayedCard}");
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

                        _manager.SubmitPlayerAction(player.Hand[chosenLeadCard].Id);

                        break;

                    case PlayerDecisionType.StealTrump:

                        customConsole.WriteLine($"{player.Name}, please discard a card to steal the trump card.");
                        customConsole.ShowPlayableCards(player.Hand);

                        var chosenDroppedCard = customConsole.RequestCardChoice(player.Hand.Count);

                        _manager.SubmitPlayerAction(player.Hand[chosenDroppedCard].Id);

                        break;

                    case PlayerDecisionType.PlayCard:

                        if (options == null)
                            throw new InvalidOperationException("Options was null");

                        customConsole.ShowCards(player.Hand);
                        customConsole.ShowPlayableCards(options);

                        var chosen = customConsole.RequestCardChoice(player.Hand.Count);

                        _manager.SubmitPlayerAction(options[chosen].Id);

                        break;
                }

            };

            //Scoring
            _manager.OnScoreChanged += (players) =>
            {
                customConsole.PrintPlayersScores(players);
            };

            _manager.OnTrickNewWinner += (winningCard, player, isDealer) =>
            {
                if (isDealer)
                {
                    customConsole.WriteLine($"{player.Name} got their dealer's trick.");
                }
                customConsole.WriteLine($"{player.Name} is currently winning with the {winningCard}");
            };

            _manager.OnTrickScored += (winningCard, player) =>
            {
                customConsole.WriteLine($"{player.Name} won with the {winningCard}");
                customConsole.WriteLine($"{player.Name} has received the trick worth 5 points.");
            };

            _manager.OnNewTrick += (leader, trickNumber) =>
            {
                customConsole.WriteLine($"Trick {trickNumber} begins.");
                customConsole.WriteLine($"{leader} will lead the trick.");
            };

            //Game State 
            _manager.OnGamePhaseChange += (gamePhase) =>
            {
                customConsole.WriteLine($"Game phase changed to: {gamePhase}");
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

        public bool PlayAgain()
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
