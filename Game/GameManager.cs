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
        public GameState CurrentState { get; private set; }
        public Player Dealer { get; private set; }
        public Player Leader { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public Player WinningPlayer { get; private set; }
        public int DealerPlayerNumber { get; private set; }
        public int CurrentPlayerNumber { get; private set; }
        public int WinningPlayerNumber { get; private set; }
        public Card TrumpCard { get; private set; }
        public Card LedCard { get; private set; }
        public Card WinningCard { get; private set; }
        public bool Steal {  get; private set; }

        //load from file later
        private readonly int TwoCards = 2;
        private readonly int ThreeCards = 3;
        private readonly int MaxScore = 25;
        public int TotalHand = 5;
        public int TotalPlayers;
        public int TotalHumans;
        public int TotalCPUs;
        public int MaxPlayers = 10;
        public int MinPlayers = 3;

        public GameManager()
        {
            // Initialize the properties in the constructor
            CurrentState = GameState.NotStarted;
        }

        public void InitializeGame(int totalPlayers, int totalHumans)
        {
            Players = new List<Player>();
            TotalPlayers = totalPlayers;
            TotalHumans = totalHumans;
            TotalCPUs = TotalPlayers - TotalHumans;
        }

        public void InitializeHumanPlayer(Player human)
        {
            Players.Add(human);
        }

        public void InitializeCPUs()
        {
            for (int i = TotalHumans; i < TotalPlayers ; i++)
            {
                Players.Add(new PlayerCPU ($"Player {i + 1}"));
            }
        }
        public void AssignDealer(int NewDealer)
        {
            DealerPlayerNumber = NewDealer;
            Dealer = Players[DealerPlayerNumber];
        }

        public void AssignRandomDealer()
        {
            Random r = new Random();           
            DealerPlayerNumber = r.Next(0, TotalPlayers);
            Dealer = Players[DealerPlayerNumber];     
        }

        public void RotateDealer()
        {
            DealerPlayerNumber = NextPlayerNumber(DealerPlayerNumber,TotalPlayers);
            Dealer = Players[DealerPlayerNumber];
        }

        public void NewDeck()
        {
            Deck = new Deck();
            Deck.Shuffle();
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
            }

            for (int i = 0; i < TotalPlayers; i++)
            {
                NextPlayer();
                GivePlayerCards(ThreeCards);
            }      
        }

        public void AssignTrumpSuit()
        {
            TrumpCard = Deck.Draw();
            Steal = false;
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
        }

        public void ChangeToPlayer(int playerNumber)
        {
            CurrentPlayerNumber = playerNumber;
            CurrentPlayer = Players[playerNumber];
        }

        public void ChangeToPlayer(Player player)
        {
            CurrentPlayerNumber = Players.IndexOf(player);
            CurrentPlayer = player;
        }

        public void SetWinner(Player player)
        {
            WinningCard = player.ChosenCard;
            WinningPlayer = player;
            WinningPlayerNumber = Players.IndexOf(player);
        }

        public void SetLeader(Player player)
        {
            Leader = player;
        }

        public void SetLedCard(Card card)
        {
            LedCard = card;
        }

        public void UpdateWinner()
        {
            if(CurrentPlayer.ChosenCard.Suit == LedCard.Suit || CurrentPlayer.ChosenCard.IsTrump)
            {
                if (CurrentPlayer.ChosenCard.Score > WinningCard.Score)
                {
                    SetWinner(CurrentPlayer);
                }
            }
        }

        public void Scoring()
        {
            WinningPlayer.Points += 5;
        }

        public void WinnerBecomesLeader()
        {
            CurrentPlayer = WinningPlayer;
            CurrentPlayerNumber = WinningPlayerNumber;
        }

        public void PlayerStoleTheTrump()
        {
            Steal = true;
        }

        public void TheTrumpIsNotStolen()
        {
            Steal = false;
        }

        public bool IsGameOver()
        {
            if (WinningPlayer.Points >= MaxScore)
            {
                return true;
            }
            else return false;
        }

        public bool ArePlayersOutOfCards()
        {
            if (Players[0].Hand.Count == 0) return true;
            else return false;
        }

        public bool HasPlayerStolen()
        {
            if (Steal)
            {
                return true;
            }
            else return false;
        }

        public void ChangeGameState(GameState state)
        {
            CurrentState = state;
        }
    }
}
