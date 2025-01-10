using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace TwentyFiveDotNet
{
    public enum Suits
    {
        Hearts = 0,
        Diamonds = 1,
        Clubs = 2,
        Spades = 3,
    }

    public enum Ranks
    {
        Two = 0,
        Three = 1,
        Four = 2,
        Five = 3,
        Six = 4,
        Seven = 5,
        Eight = 6,
        Nine = 7,
        Ten = 8,
        Jack = 9,
        Queen = 10,
        King = 11,
        Ace = 12
    }

    public enum GameState
    {
        NotStarted,
        Initialize,
        DealCards,
        LeadTurn,
        PlayerTurn,
        NewRound,
        PlayAgain,
        EndGame
    }

    public class Card
    {
        public Suits Suit { get; set; }
        public Ranks Rank { get; set; }
        public int Score { get; set; }
        public bool Playable { get; set; }

        public void AdjustForTrump(Suits trumpSuit)
        {
            if (Suit == trumpSuit)
            {
                Score += 15;
                if (Rank == Ranks.Ace) Score += 50;
                else if (Rank == Ranks.Jack) Score += 200;
                else if (Rank == Ranks.Five) Score += 300;
            }
            else if (Suit == Suits.Hearts && Rank == Ranks.Ace)
            {
                Score += 100;
            }
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }

    public class Deck
    {
        public List<Card> cards;
        public List<Card> DealtCards;

        public Deck()
        {
            cards = new List<Card>();
            DealtCards = new List<Card>();

            foreach (Suits suit in Enum.GetValues(typeof(Suits)))
            {
                foreach (Ranks rank in Enum.GetValues(typeof(Ranks)))
                {
                    cards.Add(new Card
                    {
                        Suit = suit,
                        Rank = rank,
                        Score = GetBaseScore(suit, rank),
                        Playable = true
                    });
                }
            }

            int GetBaseScore(Suits suit, Ranks rank)
            {
                // Implement the base scoring logic here
                if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.King) return 12;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Queen) return 11;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Jack) return 10;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Ten) return 9;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Nine) return 8;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Eight) return 7;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Seven) return 6;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Six) return 5;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Five) return 4;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Four) return 3;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Three) return 2;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Two) return 1;
                else if ((suit == Suits.Diamonds) && rank == Ranks.Ace) return 0;

                else if ((suit == Suits.Hearts) && rank == Ranks.Ace) return 100;

                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.King) return 12;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Queen) return 11;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Jack) return 10;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Ten) return 0;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Nine) return 1;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Eight) return 2;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Seven) return 3;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Six) return 4;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Five) return 5;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Four) return 6;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Three) return 7;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Two) return 8;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Ace) return 9;

                return 0;
            }
        }

        public void Shuffle()
        {
            Random rng = new Random();
            cards = cards.OrderBy(x => rng.Next()).ToList();
        }

        public Card Draw()
        {
            Card card = cards[0];
            cards.RemoveAt(0);
            DealtCards.Add(card);
            return card;
        }

        public void AdjustForTrump(Card TrumpCard)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                ScanTrumpList(cards, TrumpCard, i);
            }

            for (int i = 0; i < DealtCards.Count; i++)
            {
                ScanTrumpList(DealtCards, TrumpCard, i);
            }

            Console.WriteLine();
        }

        private void ScanTrumpList(List<Card> CardList, Card TrumpCard, int index)
        {
            if (CardList.ElementAt(index).Suit == TrumpCard.Suit)
            {

                if (CardList.ElementAt(index).Rank == Ranks.Ace)
                {
                    CardList.ElementAt(index).Score += 50;
                }
                else if (CardList.ElementAt(index).Rank == Ranks.Jack)
                {
                    CardList.ElementAt(index).Score += 200;
                }
                else if (CardList.ElementAt(index).Rank == Ranks.Five)
                {
                    CardList.ElementAt(index).Score += 300;
                }
                else
                {
                    CardList.ElementAt(index).Score += 15;
                }

                Console.WriteLine($"{CardList.ElementAt(index)} is now worth {CardList.ElementAt(index).Score}");
            } 
        }
    }

    public class Player
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();

        public void PlayCard(int index)
        {
            // Where the logic for playing a card should be
        }

        public void SetPlayableCards(Card TrumpCard, Card LedCard)
        {
            bool isThereLedSuit = false;
            List<Card> LegalCards = new List<Card>();

            //remove illegal cards
            if (LedCard.Suit.Equals(TrumpCard.Suit))
            {
                for (int i = 0; i < Hand.Count; i++)
                {
                    if (Hand.ElementAt(i).Suit.Equals(TrumpCard.Suit))
                    {
                        isThereLedSuit = true;
                    }
                }

                if (isThereLedSuit)
                {
                    for (int i = 0; i < Hand.Count; i++)
                    {
                        if (!(Hand.ElementAt(i).Suit.Equals(TrumpCard.Suit)))
                        {
                            Hand.ElementAt(i).Playable = false;
                        }
                        else LegalCards.Add(Hand.ElementAt(i));
                    }
                }
                else LegalCards = Hand;
            }

            else
            {
                for (int i = 0; i < Hand.Count; i++)
                {
                    if (Hand.ElementAt(i).Suit.Equals(LedCard.Suit))
                    {
                        isThereLedSuit = true;
                    }
                }

                if (isThereLedSuit)
                {
                    for (int i = 0; i < Hand.Count; i++)
                    {
                        if (!(Hand.ElementAt(i).Suit.Equals(LedCard.Suit) || Hand.ElementAt(i).Suit.Equals(TrumpCard.Suit)))
                        {
                            Hand.ElementAt(i).Playable = false;
                        }
                        else LegalCards.Add(Hand.ElementAt(i));
                    }
                }
                else LegalCards = Hand;
            }

            Console.Write("The legal cards to play are: ");
            Console.WriteLine(String.Join(", ", LegalCards));
        }

        public void ResetPlayableCards()
        {
            for (int i = 0; i < Hand.Count; i++)
            {
                Hand.ElementAt((int)i).Playable = true;
            }
        }

        public void PrintPlayableCards()
        {
            Console.Write("The playable cards are: ");

            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand.ElementAt((int)i).Playable)
                {
                    Console.Write(Hand.ElementAt(i));
                }
            }

            Console.WriteLine();
        }

        public bool HasWon()
        {
            // Logic to determine if this player has won
            return false;
        }
    }

    public class GameManager
    {
        public List<Player> Players { get; private set; }
        public Deck Deck { get; private set; }
        public Deck DealtCards { get; private set; }
        public GameState CurrentState { get; private set; }
        public Player Dealer { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public Player WinningPlayer { get; private set; }
        public int TotalPlayers { get; private set; }
        public int TotalHand { get; private set; }
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

            TotalHand = 5;

            int response = int.Parse(Console.ReadLine());

            while (response < 3 || response > 10)
            {
                Console.WriteLine("Please choose a value between 3 and 10.");
                response = int.Parse(Console.ReadLine());
            }

            TotalPlayers = response;
            
            for (int i = 0; i < TotalPlayers; i++)
            {
                Players.Add(new Player { Name = $"Player {i + 1}", Points = 0 });
                Console.WriteLine($"Player {Players.ElementAt(i).Name} has joined the game.");
            }

            Console.WriteLine($"{Players.Count} players have been created.");
            Console.WriteLine();
        }

        public void AssignDealer(int playerNumber)
        {
            Console.Write("Selecting dealer: ");

            DealerPlayerNumber = playerNumber;
            Dealer = Players.ElementAt(DealerPlayerNumber);

            Console.WriteLine(Dealer.Name);

            Deck = new Deck();
            Console.WriteLine($"A deck of {Deck.cards.Count} cards has been created.");
            Deck.Shuffle();
            Console.WriteLine("The deck has been shuffled.");
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
                    Console.Write($" {CurrentPlayer.Hand.ElementAt(j)},");
                }
                Console.WriteLine();
            }

            Console.WriteLine();

            TrumpCard = Deck.Draw();
            Console.WriteLine($"The trump card is: {TrumpCard}");
            Console.WriteLine($"The trump suit is: {TrumpCard.Suit}.");
        }

        public void GivePlayerCards(int cards)
        {
            for (int i = 0; i < cards; i++)
            {
                CurrentPlayer.Hand.Add(Deck.Draw());
            }
        }

        public void NextPlayer()
        {
            CurrentPlayerNumber = WrapNumbers(++CurrentPlayerNumber, TotalPlayers);
            CurrentPlayer = Players.ElementAt(CurrentPlayerNumber);
        }
        public void ChangeToPlayer(int playerNumber)
        {
            CurrentPlayerNumber = playerNumber;
            CurrentPlayer = Players.ElementAt(playerNumber);
        }

        public void LeadTurn()
        {
            WinningPlayer = CurrentPlayer;
            WinningPlayerNumber = CurrentPlayerNumber;
            ChosenCard = CurrentPlayer.Hand.ElementAt(0);
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

            ChosenCard = GetBestCard(CurrentPlayer.Hand);
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

            for (int j = 0; j < TotalPlayers; j++)
            {
                Console.WriteLine($"{Players.ElementAt(j).Name} has {Players.ElementAt(j).Points} points.");
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

        public void NewRound()
        {
            Console.Write("The dealer position rotates clockwise to: ");
            Dealer = Players.ElementAt(WrapNumbers(DealerPlayerNumber, TotalPlayers));
            Console.WriteLine(Dealer.Name);
            Console.WriteLine();
        }

        public void ChangeGameState(GameState state)
        {
            CurrentState = state;
        }

        public int WrapNumbers(int current, int total)
        {
            if (current >= total)
            {
                return current = 0;
            }
            else return current;
        }
        static Card GetBestCard(List<Card> Set)
        {
            //pick best card
            Card bestCard = Set.ElementAt(0);

            if (Set.Count > 1)
            {
                for (int i = 1; i < Set.Count; i++)
                {
                    if (Set.ElementAt(i).Playable)
                    {
                        if (Set.ElementAt(i).Score > bestCard.Score) bestCard = Set.ElementAt(i);
                    }
                }
            }
            return bestCard;
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            GameManager manager = new GameManager();
            Random r = new Random();

            while (manager.CurrentState != GameState.EndGame)
            {

                switch (manager.CurrentState)
                {
                    case GameState.NotStarted:


                    case GameState.Initialize:

                        manager.InitializeGame();
                        manager.AssignDealer(r.Next(0, manager.TotalPlayers));
                        manager.ChangeGameState(GameState.DealCards);

                        break;

                    case GameState.DealCards:

                        manager.DealCards();
                        manager.Deck.AdjustForTrump(manager.TrumpCard);
                        manager.NextPlayer();
                        manager.ChangeGameState(GameState.LeadTurn);

                        break;
                        
                    case GameState.LeadTurn:

                        manager.LeadTurn();
                        manager.ChangeGameState(GameState.PlayerTurn);
                        break;

                    case GameState.PlayerTurn:
                        // Handle the player's turn logic
                        // Example: Check if the player exceeded the score limit

                        for (int i = 1; i < manager.TotalPlayers; i++)
                        {
                            manager.NextPlayer();
                            manager.PlayerTurn();
                        }

                        manager.Scoring();

                        if (manager.IsGameOver())
                        {
                            Console.WriteLine("Game Over!");
                            Console.WriteLine();
                            manager.ChangeGameState(GameState.PlayAgain);
                        }

                        else if (manager.Players.ElementAt(0).Hand.Count == 0)
                        {
                            manager.ChangeGameState(GameState.NewRound);
                        }
                        else manager.ChangeGameState(GameState.LeadTurn);

                        break;

                    case GameState.NewRound:

                        manager.AssignDealer(r.Next(0, manager.TotalPlayers));
                        manager.ChangeGameState(GameState.DealCards);

                        break;

                    case GameState.PlayAgain:

                        Console.WriteLine("Would you like to play again? (Y/N)");
                        var response = Console.ReadLine();

                        if (response == "y" || response == "Y")
                        {
                            manager.ChangeGameState(GameState.Initialize);
                            Console.WriteLine("You chose to play a new game.");
                            Console.WriteLine();
                        }
                        else if (response == "n" || response == "N")
                        {
                            Console.WriteLine("You chose to not play a new game.");
                            Console.WriteLine();
                            manager.ChangeGameState(GameState.EndGame);
                        }
                        else
                        {
                            Console.WriteLine("Invalid response, try again.");
                            Console.WriteLine();
                        }

                        break;

                    case GameState.EndGame:
                        // Display results and end the loop
                        
                        break;
                }
            }

            /*foreach (var player in players)
            {
                player.Hand.Add("Ace of Spades");
                Console.WriteLine($"{player.Name} has been dealt their cards.");
            }*/
        }
    }
}
