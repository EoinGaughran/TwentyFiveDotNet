using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Game
{
    internal class GameManager
    {
        public List<Player> Players { get; private set; }
        public Dictionary<Player, Card> PlayedCards { get; private set; }
        public Deck Deck { get; private set; }
        public Deck DealtCards { get; private set; }
        public GameState CurrentState { get; private set; }
        public Player Dealer { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public Player WinningPlayer { get; private set; }
        public int DealerPlayerNumber { get; private set; }
        public int CurrentPlayerNumber { get; private set; }
        public int WinningPlayerNumber { get; private set; }
        public Card TrumpCard { get; private set; }
        public Card ChosenCard { get; private set; }
        public Card LedCard { get; private set; }
        public Card BestCard { get; private set; }
        public Card WinningCard { get; private set; }

        //load from file later
        private readonly int TwoCards = 2;
        private readonly int ThreeCards = 3;
        private readonly int MaxScore = 25;
        public int TotalHand = 5;
        public int TotalPlayers;
        public int MaxPlayers = 10;
        public int MinPlayers = 3;

        public GameManager()
        {
            // Initialize the properties in the constructor
            CurrentState = GameState.NotStarted;
        }

        public void InitializeGame()
        {
            // You can set the properties inside methods within the class
            Players = new List<Player>();

            // Set up the game (shuffle deck, initialize players, etc.)

            Console.WriteLine("Welcome to the card game 25.");
            Console.WriteLine("The game is for 3 - 10 players. 1 human player and between 2 - 9 CPU players.");
            Console.WriteLine("How many total players would you like?");

            int response;
            while (!int.TryParse(Console.ReadLine(), out response) || response < MinPlayers || response > MaxPlayers)
            {
                Console.WriteLine("Please choose a number between 3 and 10 inclusive.");
            }

            TotalPlayers = response;

            for (int i = 0; i < TotalPlayers; i++)
            {
                Players.Add(new Player ($"Player {i + 1}"));
                Console.WriteLine($"Player {Players[i].Name} has joined the game.");
            }

            Console.WriteLine($"{Players.Count} players have been created.");
            Console.WriteLine();
        }

        public void AssignRandomDealer(int randomNumber)
        {
            Console.Write("Selecting dealer: ");

            DealerPlayerNumber = randomNumber;
            Dealer = Players.ElementAt(DealerPlayerNumber);

            Console.WriteLine(Dealer.Name);

            Deck = new Deck();
            Console.WriteLine($"A deck of {Deck.cards.Count} cards has been created.");
            Deck.Shuffle();
            Console.WriteLine("The deck has been shuffled.");
        }

        public Dictionary<Player, Card> GetPlayedCards()
        {
            var PlayedCards = new Dictionary<Player, Card>();

            foreach (var Player in Players)
            {
                // Example: Calculate score based on cards the player has
                PlayedCards[Player] = Player.TableAreaCard;
            }
            return PlayedCards;
        }

        public void PrintPlayedCards()
        {
            foreach (var entry in PlayedCards)
            {
                Console.WriteLine($"{entry.Key.Name} has a {entry.Value}");
            }
        }

        public void RemoveCardsFromTableArea()
        {
            foreach (var Player in Players)
            {
                Player.RemovePlayerTableArea();
            }
        }

        public void DealCards()
        {
            ChangeToPlayer(DealerPlayerNumber);

            for (int i = 0; i < TotalPlayers; i++)
            {
                NextPlayer();
                GivePlayerCards(TwoCards);
                Console.WriteLine($"{CurrentPlayer.Name} has received {TwoCards} cards and now has {CurrentPlayer.Hand.Count} cards.");
            }

            for (int i = 0; i < TotalPlayers; i++)
            {
                NextPlayer();
                GivePlayerCards(ThreeCards);
                Console.WriteLine($"{CurrentPlayer.Name} has received {ThreeCards} cards and now has {CurrentPlayer.Hand.Count} cards.");
            }

            for (int i = 0; i < TotalPlayers; i++)
            {
                NextPlayer();
                Console.Write($"{CurrentPlayer.Name} has:");

                for (int j = 0; j < CurrentPlayer.Hand.Count; j++)
                {
                    Console.Write($" {CurrentPlayer.Hand[j]},");
                }
                Console.WriteLine();
            }

            Console.WriteLine($"{Deck.cards.Count} cards remain in the deck.");
            Console.WriteLine();

            TrumpCard = Deck.Draw();
            Console.WriteLine($"The trump card is: {TrumpCard}");
            Console.WriteLine($"The trump suit is: {TrumpCard.Suit}.");
            Console.WriteLine($"{Deck.cards.Count} cards remain in the deck.");
            Console.WriteLine();
        }

        public void GivePlayerCards(int cards)
        {
            for (int i = 0; i < cards; i++)
            {
                CurrentPlayer.Hand.Add(Deck.Draw());
            }
        }

        public int NextPlayerNumber(int PlayerNumber, int TotalPlayers)
        {
            return (PlayerNumber + 1) % TotalPlayers; // Move to the indexer to the next player
        }

        public void NextPlayer()
        {
            CurrentPlayerNumber = NextPlayerNumber(CurrentPlayerNumber, TotalPlayers);
            CurrentPlayer = Players[CurrentPlayerNumber];
            //Console.WriteLine($"Changed player to: {CurrentPlayer.Name}");
        }

        public void ChangeToPlayer(int playerNumber)
        {
            CurrentPlayerNumber = playerNumber;
            CurrentPlayer = Players[playerNumber];
        }

        public void LeadTurn()
        {
            Console.WriteLine($"{CurrentPlayer.Name} is now leading the trick.");
            WinningPlayer = CurrentPlayer;
            WinningPlayerNumber = CurrentPlayerNumber;
            ChosenCard = CurrentPlayer.Hand[0];
            CurrentPlayer.Hand.RemoveAt(0);
            Console.WriteLine($"{CurrentPlayer.Name} plays {ChosenCard}.");
            LedCard = ChosenCard;
            BestCard = ChosenCard;
            Console.WriteLine($"{LedCard.Suit} is the suit led.");
            Console.WriteLine($"{CurrentPlayer.Name} now has {CurrentPlayer.Hand.Count} cards.");
            Console.WriteLine();
        }

        public void PlayerTurn()
        {
            Console.Write($"{CurrentPlayer.Name} has in their hand: ");
            Console.WriteLine(String.Join(", ", CurrentPlayer.Hand));
            CurrentPlayer.SetPlayableCards(TrumpCard, LedCard);

            ChosenCard = CardComparer.GetBestCard(CurrentPlayer.Hand);
            Console.WriteLine($"{CurrentPlayer.Name} plays {ChosenCard}.");

            if (ChosenCard.Score > BestCard.Score)
            {
                Console.WriteLine($"{CurrentPlayer.Name}'s {ChosenCard} beats {WinningPlayer.Name}'s {BestCard}.");
                BestCard = ChosenCard;
                WinningPlayer = CurrentPlayer;
                WinningPlayerNumber = CurrentPlayerNumber;
            }
            else Console.WriteLine($"{WinningPlayer.Name}'s {BestCard} remains as the winning card.");

            CurrentPlayer.Hand.Remove(ChosenCard);
            Console.WriteLine($"{CurrentPlayer.Name} now has {CurrentPlayer.Hand.Count} cards.");

            CurrentPlayer.ResetPlayableCards();

            Console.WriteLine();
        }

        public void Scoring()
        {
            Console.WriteLine($"{WinningPlayer.Name} wins the trick with the {BestCard}");
            WinningPlayer.Points += 5;
            CurrentPlayer = WinningPlayer;
            CurrentPlayerNumber = WinningPlayerNumber;

            Console.WriteLine();

            Console.WriteLine($"Current Scores:");

            foreach (var player in Players)
            {
                Console.WriteLine($"{player.Name} has {player.Points} points.");
            }
            Console.WriteLine();
        }

        public bool IsGameOver()
        {
            if (CurrentPlayer.Points >= MaxScore)
            {
                Console.WriteLine($"{WinningPlayer.Name} wins.");
                return true;
            }
            else return false;
        }

        public bool ArePlayersOutOfCards()
        {
            if (Players[0].Hand.Count == 0) return true;
            else return false;
        }

        public void NewRound()
        {
            Console.Write("The dealer position rotates clockwise to: ");
            DealerPlayerNumber = NextPlayerNumber(DealerPlayerNumber, TotalPlayers);
            Dealer = Players[DealerPlayerNumber];

            Console.WriteLine(Dealer.Name);
            Console.WriteLine();

            Deck = new Deck();
            Deck.Shuffle();
        }

        public void ChangeGameState(GameState state)
        {
            CurrentState = state;
        }
    }
}
