using System;
using System.Linq;
using System.Collections.Generic;

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

        public Deck()
        {
            cards = new List<Card>();

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
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Ace) return 0;

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
            return card;
        }
    }

    public class Player
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
    }

    public class cardRanks
    {
        private int[,] scores;

        public cardRanks()
        {

            scores = new int[Enum.GetNames(typeof(Suits)).Length, 4];
        }

        class Program
        {

            static void Main(string[] args)
            {
                Console.WriteLine("Welcome to the card game 25.");
                Console.WriteLine("The game is for 3 - 10 players. 1 human player and between 2 - 9 CPU players.");
                Console.WriteLine("How many total players would you like?");
                int response = int.Parse(Console.ReadLine());

                while (response < 3 || response > 10)
                {
                    Console.WriteLine("Please choose a value between 3 and 10.");
                    response = int.Parse(Console.ReadLine());
                }

                int totalPlayers = response;
                int totalHand = 5;

                Console.WriteLine($"You chose {totalPlayers} players.");

                List<Player> players = new List<Player>();

                for (int i = 0; i < totalPlayers; i++)
                {
                    players.Add(new Player { Name = $"Player {i + 1}", Points = 0 });
                    Console.WriteLine($"Player {players.ElementAt(i).Name} has joined the game.");
                }

                Console.WriteLine($"{totalPlayers} players have been created.");
                Console.WriteLine();

                Console.Write("Selecting dealer: ");
                Random r = new Random();
                int dealerPlayerNumber = r.Next(0, totalPlayers);
                Player dealer = players.ElementAt(dealerPlayerNumber);
                Console.WriteLine(dealer.Name);

                bool somebodyWon = false;

                while (!somebodyWon)
                {
                    Deck deck = new Deck();
                    deck.Shuffle();
                    Console.WriteLine("The deck has been shuffled.");

                    int currentPlayerNumber = dealerPlayerNumber;
                    Player currentPlayer = players.ElementAt(dealerPlayerNumber);

                    for (int i = 0; i < totalPlayers; i++)
                    {
                        currentPlayerNumber = WrapNumbers(++currentPlayerNumber, totalPlayers);
                        currentPlayer = players.ElementAt(currentPlayerNumber);
                        //currentPlayer = nextPlayer(currentPlayer, totalPlayers);
                        currentPlayer.Hand.Add(deck.Draw());
                        currentPlayer.Hand.Add(deck.Draw());
                        Console.WriteLine($"{currentPlayer.Name} has received 2 cards and now has {currentPlayer.Hand.Count} cards.");
                    }

                    for (int i = 0; i < totalPlayers; i++)
                    {
                        currentPlayerNumber = WrapNumbers(++currentPlayerNumber, totalPlayers);
                        currentPlayer = players.ElementAt(currentPlayerNumber);
                        currentPlayer.Hand.Add(deck.Draw());
                        currentPlayer.Hand.Add(deck.Draw());
                        currentPlayer.Hand.Add(deck.Draw());
                        Console.WriteLine($"{currentPlayer.Name} has received 3 cards and now has {currentPlayer.Hand.Count} cards.");
                    }

                    for (int i = 0; i < totalPlayers; i++)
                    {
                        currentPlayerNumber = WrapNumbers(++currentPlayerNumber, totalPlayers);
                        currentPlayer = players.ElementAt(currentPlayerNumber);

                        Console.Write($"{currentPlayer.Name} has:");

                        for (int j = 0; j < currentPlayer.Hand.Count; j++)
                        {
                            Console.Write($" {currentPlayer.Hand.ElementAt(j)},");
                        }
                        Console.WriteLine();
                    }

                    Console.WriteLine();

                    Card trumpCard = deck.Draw();
                    Console.WriteLine($"The trump card is: {trumpCard}");
                    Console.WriteLine($"The trump suit is: {trumpCard.Suit}.");

                    for (int i = 0; i < deck.cards.Count; i++)
                    {
                        deck.cards.ElementAt(i).AdjustForTrump(trumpCard.Suit);
                    }

                    currentPlayerNumber = WrapNumbers(++currentPlayerNumber, totalPlayers);
                    currentPlayer = players.ElementAt(currentPlayerNumber);

                    Console.WriteLine();

                    for (int i = 0; i < totalHand; i++)
                    {
                        Player WinningPlayer = currentPlayer;
                        int WinningPlayerNumber = currentPlayerNumber;
                        Card ChosenCard = currentPlayer.Hand.ElementAt(0);

                        currentPlayer.Hand.RemoveAt(0);
                        Console.WriteLine($"{currentPlayer.Name} plays {ChosenCard}.");

                        Card ledCard = ChosenCard;
                        Card BestCard = ChosenCard;
                        Console.WriteLine($"{ledCard.Suit} is the suit led.");
                        Console.WriteLine($"{currentPlayer.Name} now has {currentPlayer.Hand.Count} cards.");

                        Console.WriteLine();

                        for (int j = 1; j < totalPlayers; j++)
                        {
                            //doesCardMatchLead
                            currentPlayerNumber = WrapNumbers(++currentPlayerNumber, totalPlayers);
                            currentPlayer = players.ElementAt(currentPlayerNumber);

                            //chosenCard = currentPlayer.Hand.ElementAt(0);
                            Console.Write($"{currentPlayer.Name} has in their hand: ");
                            Console.WriteLine(String.Join(", ", currentPlayer.Hand));
                            SetPlayableCards(currentPlayer.Hand, trumpCard, ledCard);

                            ChosenCard = GetBestCard(currentPlayer.Hand, trumpCard);
                            Console.WriteLine($"{currentPlayer.Name} plays {ChosenCard}.");

                            if (ChosenCard.Score > BestCard.Score)
                            {
                                Console.WriteLine($"{currentPlayer.Name}'s {ChosenCard} beats {WinningPlayer.Name}'s {BestCard}.");
                                BestCard = ChosenCard;
                                WinningPlayer = currentPlayer;
                                WinningPlayerNumber = currentPlayerNumber;
                            }
                            else Console.WriteLine($"{WinningPlayer.Name}'s {BestCard} remains as the winning card.");

                            currentPlayer.Hand.Remove(ChosenCard);
                            Console.WriteLine($"{currentPlayer.Name} now has {currentPlayer.Hand.Count} cards.");

                            ResetPlayableCards(currentPlayer.Hand);
                            Console.WriteLine();
                        }

                        Console.WriteLine($"{WinningPlayer.Name} wins the trick with the {BestCard}");
                        WinningPlayer.Points += 5;
                        currentPlayer = WinningPlayer;
                        currentPlayerNumber = WinningPlayerNumber;

                        Console.WriteLine();

                        Console.WriteLine($"Current Scores:");

                        for (int j = 0; j < totalPlayers; j++)
                        {
                            Console.WriteLine($"{players.ElementAt(j).Name} has {players.ElementAt(j).Points} points.");
                        }
                        Console.WriteLine();

                        if (WinningPlayer.Points >= 25)
                        {
                            Console.WriteLine($"{WinningPlayer.Name} wins.");
                            somebodyWon = true;
                        }
                        else
                        {
                            Console.Write("The dealer position rotates clockwise to: ");
                            dealer = players.ElementAt(WrapNumbers(dealerPlayerNumber, totalPlayers));
                            Console.WriteLine(dealer.Name);
                            Console.WriteLine();
                        }
                    }
                }

                /*foreach (var player in players)
                {
                    player.Hand.Add("Ace of Spades");
                    Console.WriteLine($"{player.Name} has been dealt their cards.");
                }*/
            }
            static int WrapNumbers(int current, int total)
            {
                if (current >= total)
                {
                    return current = 0;
                }
                else return current;
            }

            static void SetPlayableCards(List<Card> Hand, Card TrumpCard, Card LedCard)
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

            static void ResetPlayableCards(List<Card> Hand)
            {
                for (int i = 0; i < Hand.Count; i++)
                {
                    Hand.ElementAt((int)i).Playable = true;
                }
            }

            static void PrintPlayableCards(List<Card> Hand)
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

            static Card GetBestCard(List<Card> Set, Card TrumpCard)
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
    }
}