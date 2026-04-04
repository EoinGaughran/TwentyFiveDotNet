using System;
using System.Collections.Generic;
using System.Linq;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.ConsoleUI;

namespace TwentyFiveDotNet.Game
{
    internal class GameManager
    {
        private readonly GameConfig _config;
        private readonly RulesEngine _rules;

        private static readonly Random _rng = new Random();

        private List<Player> _players;
        private Dictionary<Player, Card> PlayedCards { get; set; }
        private Deck Deck { get; set; }
        private Deck DealtDeck { get; set; }
        private GameState CurrentState { get; set; }
        private int DealerPlayerIndex { get; set; }
        private Player Dealer => _players[DealerPlayerIndex];
        private Player Leader { get; set; }
        private int CurrentPlayerIndex;
        private Player CurrentPlayer => _players[CurrentPlayerIndex];
        private int RoundWinningPlayerIndex { get; set; }
        private Player RoundWinningPlayer => _players[RoundWinningPlayerIndex];
        private Card TrumpCard { get; set; }
        private Card LedCard { get; set; }
        private Card RoundWinningCard { get; set; }
        private bool Steal { get; set; }

        //load from file later
        private readonly int TwoCards = 2;
        private readonly int ThreeCards = 3;

        //Messaging
        public event Action<string> OnMessage;
        public event Action<string> OnRelayTrumpInfo;
        public event Action<string, string> OnRelayTrumpLeadInfo;

        //Setup
        public event Action<Deck> OnDeckCreated;
        public event Action<Deck> OnDeckShuffled;

        //Dealing
        public event Action<Player> OnDealerSelected;
        public event Action OnDealingStarted;
        public event Action<Deck, Card, Player> OnCardDealtToPlayer;

        //Trump mechanics
        public event Action<Card, Player> OnPlayerFlipsTrumpCard;
        public event Action<Dictionary<Card, int>> OnTrumpCardRevealed;
        public event Action<Player> OnTrumpCardIsAceOfHearts;
        public event Action<Card, Player> OnCardDiscarded;

        //Turn Flow
        public event Action<Player> OnLeadPlayerSelected;
        public event Action<Card, Player> OnPlayerSteal;
        public event Action<Player> OnLeadPlayerTurn;
        public event Action<Player> OnNextPlayerTurn;

        //Card Play
        public event Action<Card, Player> OnLeadCardPlayed;
        public event Action<Card, Player> OnCardPlayed;

        //Scoring/Rounds
        public event Action<List<Player>> OnScoreChanged;
        public event Action<Card, Player> OnRoundNewWinner;
        public event Action<Player> OnDealersTrick;
        public event Action<Card, Player> OnRoundEnded;
        public event Action OnNewRound;

        //Game State
        public event Action<GameState> OnGameStateChange;
        public event Action<Player> OnGameOver;
        public event Action OnNewGame;
        public event Action OnGameEnded;

        public GameManager(
            GameConfig config,
            RulesEngine rules,
            List<Player> players)
        {
            _config = config;
            _rules = rules;
            _players = players;

            // Initialize the properties in the constructor
            ChangeGameState(GameState.Initialize);
        }

        private void AssignRandomDealer()
        {
            DealerPlayerIndex = _rng.Next(0, _players.Count);
        }

        private void RotateDealer()
        {
            DealerPlayerIndex = NextPlayerNumber(DealerPlayerIndex);
        }

        private void NewDeck()
        {
            Deck = new Deck();
            Deck.Add52CardsToDeck();
            DealtDeck = new Deck();
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
            ChangeToPlayer(DealerPlayerIndex);

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

        private void GivePlayerCards(int maxCards)
        {
            for (int i = 0; i < maxCards; i++)
            {
                var card = Deck.Draw();
                DealtDeck.AddCardToDeck(card);
                CurrentPlayer.Hand.Add(card);
                OnCardDealtToPlayer?.Invoke(Deck, card, CurrentPlayer);
            }
        }

        private int NextPlayerNumber(int PlayerNumber)
        {
            return (PlayerNumber + 1) % _players.Count; // Move to the indexer to the next player
        }

        private void NextPlayer()
        {
            CurrentPlayerIndex = NextPlayerNumber(CurrentPlayerIndex);
        }

        private void ChangeToPlayer(int playerNumber)
        {
            CurrentPlayerIndex = playerNumber;
        }

        private void ChangeToPlayer(Player player)
        {
            CurrentPlayerIndex = _players.IndexOf(player);
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
            RoundWinningCard = card;
            RoundWinningPlayerIndex = _players.IndexOf(player);
        }

        private void WinnerBecomesLeader()
        {
            CurrentPlayerIndex = RoundWinningPlayerIndex;
        }

        private void PlayerStoleTheTrump(bool state)
        {
            Steal = state;
        }

        private bool ArePlayersOutOfCards() => _players.All(p => p.Hand.Count == 0);

        private bool HasPlayerStolen() => Steal;

        private void ChangeGameState(GameState state)
        {
            CurrentState = state;
            OnGameStateChange?.Invoke(state);
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

                        OnGameEnded?.Invoke();

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
            NewDeck();
            OnDeckCreated?.Invoke(Deck);

            Deck.Shuffle();
            OnDeckShuffled?.Invoke(Deck);

            AssignRandomDealer();
            OnDealerSelected?.Invoke(Dealer);

            ChangeGameState(GameState.DealCards);
        }

        private void HandleDealCards()
        {
            OnDealingStarted?.Invoke();

            DealCards();

            TrumpCard = Deck.Draw();
            Dealer.PlayerFlipTrumpCard(TrumpCard);

            OnPlayerFlipsTrumpCard?.Invoke(TrumpCard, Dealer);
            
            var allCards = Deck.Cards.Concat(DealtDeck.Cards);
            var trumpCards = _rules.GetTrumpCardsSorted(allCards, TrumpCard);

            var display = trumpCards.ToDictionary(
                card => card,
                card => _rules.GetCardScore(card, TrumpCard));

            OnTrumpCardRevealed?.Invoke(display);

            ChangeGameState(GameState.Stealing);
        }

        private void HandleStealing()
        {
            PlayerStoleTheTrump(false);

            if (_rules.IsTrumpCardStealable(TrumpCard))
            {
                OnTrumpCardIsAceOfHearts?.Invoke(Dealer);
                var droppedCard = Dealer.StealTrump(TrumpCard, LedCard);

                Dealer.Hand.Remove(droppedCard);
                OnCardDiscarded?.Invoke(droppedCard, Dealer);

                Dealer.Hand.Add(TrumpCard);
                OnPlayerSteal(TrumpCard, Dealer);
                PlayerStoleTheTrump(true);

                ChangeGameState(GameState.LeadTurn);
            }
            else
            {
                NextPlayer();

                if (_rules.CanPlayerSteal(CurrentPlayer.Hand, TrumpCard))
                {
                    var droppedCard = CurrentPlayer.StealTrump(TrumpCard, LedCard);             
                    OnCardDiscarded(droppedCard, CurrentPlayer);
                    CurrentPlayer.Hand.Remove(droppedCard);

                    CurrentPlayer.Hand.Add(TrumpCard);
                    OnPlayerSteal?.Invoke(TrumpCard, CurrentPlayer);

                    PlayerStoleTheTrump(true);
                    ChangeToPlayer(Dealer); //This will change when stealing is determined on players first turn
                    ChangeGameState(GameState.LeadTurn);
                }

                if (CurrentPlayer == Dealer)
                {
                    if (!HasPlayerStolen())
                    {
                        OnMessage?.Invoke("Nobody has the Ace of Trumps so nobody steals."); //This is not a real event
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
            OnLeadPlayerTurn(CurrentPlayer);
            OnRelayTrumpInfo?.Invoke(TrumpCard.GetSuitSymbolUnicoded());

            var chosenCard = CurrentPlayer.LeadCard();
            SetLedCard(chosenCard);
            OnCardPlayed(chosenCard, CurrentPlayer);

            SetWinner(LedCard, CurrentPlayer);
            OnRoundNewWinner(chosenCard, CurrentPlayer);

            UpdatePlayedCards(CurrentPlayer, chosenCard);
            CurrentPlayer.Hand.Remove(chosenCard);

            OnRelayTrumpLeadInfo?.Invoke(TrumpCard.GetSuitSymbolUnicoded(), LedCard.GetSuitSymbolUnicoded());

            ChangeGameState(GameState.PlayerTurn);
        }

        private void PlayerTurn()
        {
            NextPlayer();

            if (CurrentPlayer == Leader)
            {
                ChangeGameState(GameState.Scoring);
            }
            else
            {
                OnNextPlayerTurn?.Invoke(CurrentPlayer);

                var playableCards = _rules.GetPlayableCards(CurrentPlayer.Hand, TrumpCard, LedCard);

                var chosenCard = CurrentPlayer.ChooseCard(playableCards, TrumpCard, LedCard);

                OnCardPlayed?.Invoke(chosenCard, CurrentPlayer);

                UpdatePlayedCards(CurrentPlayer, chosenCard);

                if (_rules.IsCardBetter(chosenCard, RoundWinningCard, LedCard, TrumpCard))
                    SetWinner(chosenCard, CurrentPlayer);

                CurrentPlayer.Hand.Remove(chosenCard);

                OnRoundNewWinner?.Invoke(RoundWinningCard, RoundWinningPlayer);
            }
        }

        private void Scoring()
        {
            _rules.Scoring(RoundWinningPlayer);
            OnRoundEnded?.Invoke(RoundWinningCard, RoundWinningPlayer);

            OnScoreChanged?.Invoke(_players);

            if (_rules.IsGameOver(RoundWinningPlayer))
            {
                OnGameOver?.Invoke(RoundWinningPlayer);
                ChangeGameState(GameState.PlayAgain);
            }

            else if (ArePlayersOutOfCards())
            {
                ChangeGameState(GameState.NewRound);
            }
            else
            {
                WinnerBecomesLeader();
                OnLeadPlayerSelected?.Invoke(RoundWinningPlayer);

                ChangeGameState(GameState.LeadTurn);

            }
        }

        private void NewRound()
        {
            NewDeck();
            OnDeckCreated?.Invoke(Deck);

            Deck.Shuffle();
            OnDeckShuffled?.Invoke(Deck);

            RotateDealer();
            OnDealerSelected?.Invoke(Dealer);

            ChangeGameState(GameState.DealCards);
        }

        public void NewGame()
        {
            ClearPlayersHands();
            ClearPlayedCards();
            ClearPlayerPoints();

            OnNewGame?.Invoke();

            ChangeGameState(GameState.NewRound);
        }

        public void EndGame()
        {
            OnGameEnded?.Invoke();
            ChangeGameState(GameState.EndGame);
        }
    }
}
