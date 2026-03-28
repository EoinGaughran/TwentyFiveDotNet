using System;
using System.Collections.Generic;
using System.Linq;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.Interfaces;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Game
{
    internal class GameManager
    {
        private readonly GameConfig _config;
        private readonly RulesEngine _rules;
        private readonly IGameInteraction _ui;

        private List<Player> _players { get; set; }
        private Dictionary<Player, Card> PlayedCards { get; set; }
        private Deck Deck { get; set; }
        private GameState CurrentState { get; set; }
        private Player Dealer { get; set; }
        private Player Leader { get; set; }
        private Player CurrentPlayer { get; set; }
        private Player WinningPlayer { get; set; }
        private int DealerPlayerNumber { get; set; }
        private int CurrentPlayerNumber { get; set; }
        private int WinningPlayerNumber { get; set; }
        private Card TrumpCard { get; set; }
        private Card LedCard { get; set; }
        private Card WinningCard { get; set; }
        private bool Steal { get; set; }

        //load from file later
        private readonly int TwoCards = 2;
        private readonly int ThreeCards = 3;

        public event Action OnDealingStarted;
        public event Action<Player> OnCardsDealtToPlayer;
        public event Action<Card, IEnumerable<Card>, IEnumerable<Card>> OnTrumpCardRevealed;
        public event Action<string> OnMessage;
        public event Action<List<Player>> ScoreChanged;

        public GameManager(
            GameConfig config,
            RulesEngine rules,
            List<Player> players,
            IGameInteraction ui)
        {
            _config = config;
            _rules = rules;
            _players = players;
            _ui = ui;

            // Initialize the properties in the constructor
            ChangeGameState(GameState.Initialize);
        }

        private void AssignRandomDealer()
        {
            Random r = new Random();
            DealerPlayerNumber = r.Next(0, _players.Count);
            Dealer = _players[DealerPlayerNumber];
        }

        private void RotateDealer()
        {
            DealerPlayerNumber = NextPlayerNumber(DealerPlayerNumber);
            Dealer = _players[DealerPlayerNumber];
        }

        private void NewDeck()
        {
            Deck = new Deck();
        }

        private void ResetCardsPlayed()
        {
            PlayedCards = new Dictionary<Player, Card>();
        }

        private void UpdatePlayedCards(Player player, Card chosenCard)
        {
            PlayedCards.Add(player, chosenCard);
        }

        private void DealCards()
        {
            ChangeToPlayer(DealerPlayerNumber);

            for (int i = 0; i < _players.Count; i++)
            {
                NextPlayer();
                GivePlayerCards(TwoCards);
            }

            for (int i = 0; i < _players.Count; i++)
            {
                NextPlayer();
                GivePlayerCards(ThreeCards);
            }
        }

        private void AssignTrumpSuit()
        {
            TrumpCard = Deck.Draw();
        }

        private void GivePlayerCards(int cards)
        {
            for (int i = 0; i < cards; i++)
            {
                CurrentPlayer.Hand.Add(Deck.Draw());
            }
        }

        private int NextPlayerNumber(int PlayerNumber)
        {
            return (PlayerNumber + 1) % _players.Count; // Move to the indexer to the next player
        }

        private void NextPlayer()
        {
            CurrentPlayerNumber = NextPlayerNumber(CurrentPlayerNumber);
            CurrentPlayer = _players[CurrentPlayerNumber];
        }

        private void ChangeToPlayer(int playerNumber)
        {
            CurrentPlayerNumber = playerNumber;
            CurrentPlayer = _players[playerNumber];
        }

        private void ChangeToPlayer(Player player)
        {
            CurrentPlayerNumber = _players.IndexOf(player);
            CurrentPlayer = player;
        }
        private void SetLeader(Player player)
        {
            Leader = player;
        }

        private void SetLedCard(Card card)
        {
            LedCard = card;
        }
        private void SetWinner(Card card, Player player)
        {
            WinningCard = card;
            WinningPlayer = player;
            WinningPlayerNumber = _players.IndexOf(player);
        }

        private void WinnerBecomesLeader()
        {
            CurrentPlayer = WinningPlayer;
            CurrentPlayerNumber = WinningPlayerNumber;
        }

        private void PlayerStoleTheTrump(bool state)
        {
            Steal = state;
        }

        private bool ArePlayersOutOfCards()
        {
            if (_players[0].Hand.Count == 0) return true;
            else return false;
        }

        private bool HasPlayerStolen()
        {
            if (Steal)
            {
                return true;
            }
            else return false;
        }

        private void ChangeGameState(GameState state)
        {
            CurrentState = state;
            OnMessage?.Invoke("Game State changed to: " + state);
        }

        private void ClearPlayersHands()
        {
            foreach (var player in _players)
                player.Hand.Clear();
        }

        private void ClearPlayedCards()
        {
            PlayedCards.Clear();
        }

        private void ClearPlayerPoints()
        {
            foreach (var player in _players)
                player.Points = 0;
        }

        public void StartGame()
        {
            ConsoleSettings consoleSettings = new ConsoleSettings();
            consoleSettings.DevMode = _config.DevMode;
            consoleSettings.Delay = _config.DelayInMilliseconds;

            while (CurrentState != GameState.EndGame)
            {
                switch (CurrentState)
                {
                    case GameState.NotStarted:

                    case GameState.Initialize:

                        Initialize();

                        break;

                    case GameState.DealCards:

                        HandleDealCards();

                        break;

                    case GameState.Stealing:

                        HandleStealing();

                        break;

                    case GameState.LeadTurn:

                        LeadTurn();

                        break;

                    case GameState.PlayerTurn:

                        PlayerTurn();

                        break;

                    case GameState.Scoring:

                        Scoring();

                        break;

                    case GameState.NewRound:

                        NewRound();

                        break;

                    case GameState.PlayAgain:

                        CheckForNewGame();

                        break;

                    case GameState.NewGame:

                        NewGame();

                        break;

                    case GameState.EndGame:

                        break;
                }
            }
        }

        private void Initialize()
        {
            AssignRandomDealer();
            OnMessage?.Invoke($"Selected dealer: {Dealer.Name}");

            NewDeck();
            OnMessage?.Invoke("The deck has been created.");

            Deck.Shuffle();
            OnMessage?.Invoke("The deck has been shuffled.");

            ChangeGameState(GameState.DealCards);
        }

        private void HandleDealCards()
        {
            OnDealingStarted?.Invoke();

            DealCards();

            foreach (var player in _players)
            {
                OnCardsDealtToPlayer?.Invoke(player);
            }

            AssignTrumpSuit();

            OnTrumpCardRevealed?.Invoke(TrumpCard, Deck.cards, Deck.DealtCards);

            var allCards = Deck.cards.Concat(Deck.DealtCards);
            var trumpCards = _rules.GetTrumpCardsSorted(allCards, TrumpCard);

            var display = trumpCards.ToDictionary(
                card => card,
                card => _rules.GetCardScore(card, TrumpCard));

            _ui.ShowTrumpCards(display);

            ChangeGameState(GameState.Stealing);
        }

        private void HandleStealing()
        {
            PlayerStoleTheTrump(false);

            if (_rules.IsTrumpCardStealable(TrumpCard))
            {
                OnMessage?.Invoke("The Trump card is the Ace of Hearts. The Dealer can steal it.");
                var droppedCard = Dealer.StealTrump(TrumpCard, LedCard);
                OnMessage?.Invoke($"Dealer Dropped a card and took the Trump Card: {TrumpCard}");

                Dealer.Hand.Remove(droppedCard);
                Dealer.Hand.Add(TrumpCard);
                PlayerStoleTheTrump(true);

                ChangeGameState(GameState.LeadTurn);
            }
            else
            {
                NextPlayer();

                if (_rules.CanPlayerSteal(CurrentPlayer.Hand, TrumpCard))
                {
                    OnMessage?.Invoke($"{CurrentPlayer} has the Ace of Trumps and so gets to steal.");

                    CurrentPlayer.Hand.Remove(CurrentPlayer.StealTrump(TrumpCard, LedCard));
                    OnMessage?.Invoke($"{CurrentPlayer} placed down their worst card");

                    CurrentPlayer.Hand.Add(TrumpCard);
                    OnMessage?.Invoke($"{CurrentPlayer} stole the trump card {TrumpCard}.");

                    PlayerStoleTheTrump(true);
                    ChangeToPlayer(Dealer);
                    ChangeGameState(GameState.LeadTurn);
                }

                if (CurrentPlayer == Dealer)
                {
                    if (!HasPlayerStolen())
                    {
                        OnMessage?.Invoke("Nobody has the Ace of Trumps so nobody steals.");
                    }

                    NextPlayer();
                    ChangeGameState(GameState.LeadTurn);
                }
            }
        }

        private void LeadTurn()
        {
            ResetCardsPlayed();
            SetLeader(CurrentPlayer);
            OnMessage?.Invoke($"{Leader.Name} is leading the trick.");
            OnMessage?.Invoke(($"{TrumpCard.GetSuitSymbolUnicoded()} are trumps."));

            var chosenCard = CurrentPlayer.LeadCard();
            SetLedCard(chosenCard);
            OnMessage?.Invoke($"{CurrentPlayer} played {chosenCard}");

            SetWinner(LedCard, CurrentPlayer);
            OnMessage?.Invoke($"{CurrentPlayer} is winning with {chosenCard}");

            CurrentPlayer.Hand.Remove(chosenCard);

            UpdatePlayedCards(CurrentPlayer, chosenCard);

            OnMessage?.Invoke($"{TrumpCard.GetSuitSymbolUnicoded()} are trumps, {LedCard.GetSuitSymbolUnicoded()} were led.");

            ChangeGameState(GameState.PlayerTurn);
        }

        private void PlayerTurn()
        {
            NextPlayer();
            OnMessage?.Invoke($"It's {CurrentPlayer}'s turn.");

            if (CurrentPlayer == Leader)
            {
                ChangeGameState(GameState.Scoring);
            }
            else
            {
                var playableCards = _rules.GetPlayableCards(CurrentPlayer.Hand, TrumpCard, LedCard);

                var chosenCard = CurrentPlayer.ChooseCard(playableCards, TrumpCard, LedCard);

                OnMessage?.Invoke($"{CurrentPlayer.Name} played {chosenCard}.");

                UpdatePlayedCards(CurrentPlayer, chosenCard);

                if (_rules.IsCardBetter(chosenCard, WinningCard, LedCard, TrumpCard))
                    SetWinner(chosenCard, CurrentPlayer);

                CurrentPlayer.Hand.Remove(chosenCard);

                OnMessage?.Invoke($"{WinningPlayer.Name} is currently winning with the {WinningCard}.");
            }
        }

        private void Scoring()
        {
            _rules.Scoring(WinningPlayer);
            OnMessage?.Invoke($"{WinningPlayer.Name} wins the trick with the {WinningCard} and receives 5 points.");

            ScoreChanged?.Invoke(_players);

            if (_rules.IsGameOver(WinningPlayer))
            {
                OnMessage?.Invoke("Game Over!");
                OnMessage?.Invoke($"{WinningPlayer.Name} wins.");

                ChangeGameState(GameState.PlayAgain);
            }

            else if (ArePlayersOutOfCards())
            {
                ChangeGameState(GameState.NewRound);
            }
            else
            {
                WinnerBecomesLeader();
                OnMessage?.Invoke($"{WinningPlayer.Name} leads the next trick.");

                ChangeGameState(GameState.LeadTurn);

            }
        }

        private void NewRound()
        {
            RotateDealer();
            NewDeck();

            Deck.Shuffle();
            OnMessage?.Invoke($"Deck has been shuffled.");

            OnMessage?.Invoke($"The dealer position has rotated clockwise to: {Dealer.Name}");

            ChangeGameState(GameState.DealCards);
        }

        private void CheckForNewGame()
        {
            bool playAgain = _ui.PlayAgainQuestion("Play again?");

            if (playAgain) ChangeGameState(GameState.NewGame);
            else ChangeGameState(GameState.EndGame);
        }

        private void NewGame()
        {
            ClearPlayersHands();
            ClearPlayedCards();
            ClearPlayerPoints();

            ChangeGameState(GameState.NewRound);
        }
    }
}
