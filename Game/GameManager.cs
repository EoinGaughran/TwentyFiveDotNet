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
        private List<(Player player, Card card)> PlayedCards { get; set; }
        private Deck Deck { get; set; }
        private Deck DealtDeck { get; set; }
        private GameState CurrentState { get; set; }
        private Player Dealer { get; set; }
        private Player Leader { get; set; }
        private Player CurrentPlayer { get; set; }
        private Player RoundWinningPlayer { get; set; }
        private Card TrumpCard { get; set; }
        private Card LedCard { get; set; }
        private Card RoundWinningCard { get; set; }
        private bool TrumpStolen { get; set; }

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
        public event Action<Player> OnTrumpCardIsAceOTrumps;
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
        public event Action OnProgramClosed;

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
            Dealer = _players[_rng.Next(0, _players.Count)];
        }

        private void RotateDealer()
        {
            Dealer = NextPlayer(Dealer);
        }

        private void NewDeck()
        {
            Deck = new Deck();
            Deck.Add52CardsToDeck();
            DealtDeck = new Deck();
        }

        private void ResetCardsPlayed()
        {
            PlayedCards = new List<(Player player, Card card)>();
        }

        private void UpdatePlayedCards(Player player, Card chosenCard)
        {
            PlayedCards.Add((player, chosenCard));
        }

        private void DealCards()
        {
            ChangeToPlayer(Dealer);

            foreach (var amount in _config.DealPattern)
            {
                for( int i = 0; i < _players.Count; i++ )
                {
                    RotateCurrentPlayer();
                    GivePlayerCards(CurrentPlayer, amount);
                }
            }
        }

        private void GivePlayerCards(Player player, int maxCards)
        {
            for (int i = 0; i < maxCards; i++)
            {
                var card = Deck.Draw();
                DealtDeck.AddCardToDeck(card);
                player.Hand.Add(card);
                OnCardDealtToPlayer?.Invoke(Deck, card, player);
            }
        }

        private void RotateCurrentPlayer()
        {
            CurrentPlayer = NextPlayer(CurrentPlayer);
        }

        private Player NextPlayer(Player player)
        {
            int index = _players.IndexOf(player);

            if (index == -1)
                throw new InvalidOperationException("Player not found in player list.");

            int nextIndex = (index + 1) % _players.Count;
            return _players[nextIndex];
        }

        private void ChangeToPlayer(Player player)
        {
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
            RoundWinningCard = card;
            RoundWinningPlayer = player;
        }

        private void HandleStealing()
        {
            var droppedCard = CurrentPlayer.StealTrump(TrumpCard, LedCard);
            OnCardDiscarded?.Invoke(droppedCard, CurrentPlayer);
            CurrentPlayer.Hand.Remove(droppedCard);

            CurrentPlayer.Hand.Add(TrumpCard);
            OnPlayerSteal?.Invoke(TrumpCard, CurrentPlayer);

            PlayerStoleTheTrump(true);

        }

        private void PlayerStoleTheTrump(bool state)
        {
            TrumpStolen = state;
        }

        private bool ArePlayersOutOfCards() => _players.All(p => p.Hand.Count == 0);

        private bool HasPlayerStolen() => TrumpStolen;

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
            PlayedCards ??= new List<(Player player, Card card)>();
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

            SetLeader(NextPlayer(Dealer));

            ResetCardsPlayed();

            ChangeGameState(GameState.DealCards);
            return;
        }

        private void HandleDealCards()
        {
            OnDealingStarted?.Invoke();

            DealCards();

            TrumpCard = Deck.Draw();
            Dealer.PlayerFlipTrumpCard(TrumpCard);

            OnPlayerFlipsTrumpCard?.Invoke(TrumpCard, Dealer);

            DealtDeck.AddCardToDeck(TrumpCard);
            var allCards = Deck.Cards.Concat(DealtDeck.Cards);
            var trumpCards = _rules.GetTrumpCardsSorted(allCards, TrumpCard);

            var display = trumpCards.ToDictionary(
                card => card,
                card => _rules.GetCardScore(card, TrumpCard));

            OnTrumpCardRevealed?.Invoke(display);

            PlayerStoleTheTrump(false);

            ChangeGameState(GameState.LeadTurn);
            return;
        }

        private void LeadTurn()
        {
            ChangeToPlayer(Leader);

            if (_rules.CanPlayerSteal(CurrentPlayer.Hand, TrumpCard))
            {
                HandleStealing();
            }

            SetLeader(CurrentPlayer);
            OnLeadPlayerTurn?.Invoke(CurrentPlayer);
            OnRelayTrumpInfo?.Invoke(TrumpCard.GetSuitSymbolUnicoded());

            var chosenCard = CurrentPlayer.LeadCard();
            SetLedCard(chosenCard);
            OnCardPlayed?.Invoke(chosenCard, CurrentPlayer);

            SetWinner(LedCard, CurrentPlayer);
            OnRoundNewWinner?.Invoke(chosenCard, CurrentPlayer);

            UpdatePlayedCards(CurrentPlayer, chosenCard);
            CurrentPlayer.Hand.Remove(chosenCard);

            OnRelayTrumpLeadInfo?.Invoke(TrumpCard.GetSuitSymbolUnicoded(), LedCard.GetSuitSymbolUnicoded());

            ChangeGameState(GameState.PlayerTurn);
            return;
        }

        private void PlayerTurn()
        {
            RotateCurrentPlayer();

            if (CurrentPlayer.Equals(Leader))
            {
                ChangeGameState(GameState.Scoring);
                return;
            }

            if (!HasPlayerStolen())
            {
                if(_rules.CanPlayerSteal(CurrentPlayer.Hand, TrumpCard))
                    HandleStealing();

                else if (CurrentPlayer.Equals(Dealer) &&
                    _rules.IsTrumpCardStealable(TrumpCard))
                    HandleStealing();
            }

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

        private void Scoring()
        {
            _rules.Scoring(RoundWinningPlayer);
            OnRoundEnded?.Invoke(RoundWinningCard, RoundWinningPlayer);

            OnScoreChanged?.Invoke(_players);

            if (_rules.IsGameOver(RoundWinningPlayer))
            {
                OnGameOver?.Invoke(RoundWinningPlayer);
                ChangeGameState(GameState.PlayAgain);
                return;
            }

            if (ArePlayersOutOfCards())
            {
                ChangeGameState(GameState.NewRound);
                return;
            }

            SetLeader(RoundWinningPlayer); //Winner becomes leader
            OnLeadPlayerSelected?.Invoke(RoundWinningPlayer);

            ChangeGameState(GameState.LeadTurn);
            return;
        }

        private void NewRound()
        {
            ResetCardsPlayed();

            NewDeck();
            OnDeckCreated?.Invoke(Deck);

            Deck.Shuffle();
            OnDeckShuffled?.Invoke(Deck);

            RotateDealer();
            OnDealerSelected?.Invoke(Dealer);

            ChangeGameState(GameState.DealCards);
            return;
        }

        public void NewGame()
        {
            ClearPlayersHands();
            ClearPlayedCards();
            ClearPlayerPoints();
            ResetCardsPlayed();

            OnNewGame?.Invoke();

            ChangeGameState(GameState.NewRound);
            return;
        }

        public void EndGame()
        {
            OnProgramClosed?.Invoke();
            ChangeGameState(GameState.EndGame);
            return;
        }
    }
}
