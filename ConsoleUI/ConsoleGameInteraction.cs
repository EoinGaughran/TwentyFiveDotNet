using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
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
            _manager.OnMessage += DisplayMessage;
            _manager.OnRelayTrumpInfo += DisplayTrumpInfo;
            _manager.OnRelayTrumpLeadInfo += DisplayTrumpLeadInfo;

            //Setup
            _manager.OnDeckCreated += UpdateDeckCreated;
            _manager.OnDeckShuffled += UpdateDeckShuffled;

            //Dealing
            _manager.OnDealerSelected += UpdateDealerSelected;
            _manager.OnDealingStarted += ShowDealingStarted;
            _manager.OnCardDealtToPlayer += DealCardToPlayer;

            //Trump mechanics
            _manager.OnPlayerFlipsTrumpCard += ShowTrumpCardFlip;
            _manager.OnTrumpCardRevealed += DisplayTrumpCards;
            _manager.OnTrumpCardIsAceOTrumps += HandleTrumpAceOfHeartsSpecial;
            _manager.OnCardDiscarded += DiscardCard;

            //Turn Flow
            _manager.OnLeadPlayerSelected += HighlightLeadPlayer;
            _manager.OnPlayerSteal += AnimatePlayerSteal;
            _manager.OnLeadPlayerTurn += ShowLeadTurn;
            _manager.OnNextPlayerTurn += ShowNextTurn;

            //Card Play
            _manager.OnLeadCardPlayed += PlayLeadCard;
            _manager.OnCardPlayed += PlayCard;

            //Scoring/Rounds
            _manager.OnScoreChanged += UpdateScore;
            _manager.OnRoundNewWinner += ShowRoundWinner;
            _manager.OnDealersTrick += ShowDealersTrick;
            _manager.OnRoundEnded += ShowRoundEnd;
            _manager.OnNewRound += PrepareNewRound;

            //Game State
            _manager.OnGameStateChange += UpdateGameState;
            _manager.OnGameOver += ShowGameOver;
            _manager.OnNewGame += ResetForNewGame;
            _manager.OnProgramClosed += ShowClosingResponse;
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

        public void HandleDealingStarted()
        {
            CustomConsole.WriteLine("Dealing cards...", UIPrefix, _settings);
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

        /*********
         * EVENTS
         *********/

        //Messaging
        void DisplayMessage(string message)
        {
            CustomConsole.WriteLine(message, TextUIPrefix, _settings);
        }
        void DisplayTrumpInfo(string message)
        {
            CustomConsole.WriteLine(message + " are trumps.", TextUIPrefix, _settings);
        }
        void DisplayTrumpLeadInfo(string line1, string line2)
        {
            CustomConsole.WriteLine($"The trump suit is {line1}. The leading suit is {line2}.", TextUIPrefix, _settings);
        }

        //Setup
        void UpdateDeckCreated(Deck deck)
        {
            CustomConsole.WriteLine($"{deck} with {deck.Cards.Count} cards was created.", UIPrefix, _settings);
        }
        void UpdateDeckShuffled(Deck deck)
        {
            CustomConsole.WriteLine($"{deck} was shuffled.", UIPrefix, _settings);
        }

        //Dealing
        void UpdateDealerSelected(Player player)
        {
            CustomConsole.WriteLine($"{player.Name} has been selected as the dealer.", UIPrefix, _settings);
        }
        void ShowDealingStarted()
        {
            //May not be required
        }
        void DealCardToPlayer(Deck deck, Card card, Player player)
        {
            CustomConsole.WriteLine($"The {card} has been dealt from the {deck} to {player.Name}.", UIPrefix, _settings);
        }

        //Trump mechanics
        void ShowTrumpCardFlip(Card TrumpCard, Player dealer)
        {
            CustomConsole.WriteLine($"Dealer {dealer.Name} flipped the trump card. It's the {TrumpCard}", UIPrefix, _settings);

        }
        void HandleTrumpAceOfHeartsSpecial(Player dealer)
        {
            CustomConsole.WriteLine($"Dealer {dealer.Name} has the Ace of Hearts. They may steal it.", UIPrefix, _settings);
        }
        void DisplayTrumpCards(Dictionary<Card, int> Trumps)
        {
            foreach (var kvp in Trumps)
            {
                Console.WriteLine(UIPrefix + $"{kvp.Key} is worth: {kvp.Value}");
            }
        }

        //Turn Flow
        void HighlightLeadPlayer(Player player)
        {
            CustomConsole.WriteLine($"{player.Name} is leading the trick.", UIPrefix, _settings);
        }

        void AnimatePlayerSteal(Card card, Player player)
        {
            CustomConsole.WriteLine($"{player.Name} stole the trump card {card}.", UIPrefix, _settings);
        }
        void DiscardCard(Card card, Player player)
        {
            CustomConsole.WriteLine($"{player.Name} discarded a card. (It was a {card})", UIPrefix, _settings);
        }
        void ShowLeadTurn(Player player)
        {
            CustomConsole.WriteLine($"{player.Name} will now lead the trick.", UIPrefix, _settings);
        }
        void ShowNextTurn(Player player)
        {
            CustomConsole.WriteLine($"It's {player.Name}'s turn.", UIPrefix, _settings);
        }
        
        //Card Play
        void PlayLeadCard(Card card, Player player)
        {
            CustomConsole.WriteLine($"{player.Name} led with the {card}. Suit {card.Suit} is leading.", UIPrefix, _settings);
        }
        void PlayCard(Card card, Player player)
        {
            CustomConsole.WriteLine($"{player.Name} played the {card}", UIPrefix, _settings);
        }
        
        //Scoring/Rounds
        void UpdateScore(List<Player> players)
        {
            CustomConsole.PrintPlayersScores(players, UIPrefix, _settings);
        }
        void ShowRoundWinner(Card winningCard, Player player)
        {
            CustomConsole.WriteLine($"{player.Name} is currently winning with the {winningCard}", UIPrefix, _settings);
        }
        void ShowDealersTrick(Player player)
        {
            CustomConsole.WriteLine($"{player.Name} got his dealer's trick.", UIPrefix, _settings);
        }
        void ShowRoundEnd(Card winningCard, Player player)
        {
            CustomConsole.WriteLine($"{player.Name} won with the {winningCard}", UIPrefix, _settings);
            CustomConsole.WriteLine($"{player.Name} has received the trick worth 5 points.", UIPrefix, _settings);
        }
        void PrepareNewRound()
        {
            CustomConsole.WriteLine($"New Round!", UIPrefix, _settings);
        }
        
        //Game State
        void UpdateGameState(GameState state)
        {
            CustomConsole.WriteLine($"The game state has changed to: {state}", UIPrefix, _settings);
        }
        void ShowGameOver(Player winner)
        {
            CustomConsole.WriteLine($"Game Over!", UIPrefix, _settings);
        }
        void ResetForNewGame()
        {
            CustomConsole.WriteLine($"The deck has been removed.", UIPrefix, _settings);
            CustomConsole.WriteLine($"Players hands have been cleared.", UIPrefix, _settings);
            CustomConsole.WriteLine($"Players scores have been cleared.", UIPrefix, _settings);
            CustomConsole.WriteLine($"The list of played cards has been cleared.", UIPrefix, _settings);
        }

        void ShowClosingResponse()
        {
            CustomConsole.WriteLine($"The game has ended. The program will now close.", UIPrefix, _settings);
        }
    }
}
