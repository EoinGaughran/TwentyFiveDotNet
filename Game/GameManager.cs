using System;
using System.Collections.Generic;
using System.Linq;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Game
{
    internal class GameManager
    {
        public enum GamePhase
        {
            NotStarted,
            Initialize,
            DealCards,
            HandleTrumps,
            PlayerTurn_LeadStart,
            PlayerTurn_LeadPlayCard, 
            PlayerTurn_Start,
            PlayerTurn_StealDecision,
            PlayerTurn_PlayCard,
            AwaitingPlayerInput,
            Scoring,
            NewRound,
            AwaitingReplayDecision,
            NewGame,
            EndGame
        }

        private readonly RulesEngine _rules;
        private readonly TurnManager _turnManager;
        private readonly GameState _gameState;

        private static readonly Random _rng = new();

        private List<Player> Players => _gameState.Players;
        private List<(Player player, Card card)> PlayedCards { get; set; }
        private Deck Deck => _gameState.Deck;
        private GamePhase CurrentPhase { get; set; }
        private Player RoundWinningPlayer { get; set; }
        private Player PendingPlayer { get; set; }
        private List<Card> PendingOptions { get; set; }
        private PlayerDecisionType PendingDecisionType { get; set; }
        private Card TrumpCard { get; set; }
        private Card LedCard { get; set; }
        private Card RoundWinningCard { get; set; }
        private bool TrumpStolen { get; set; }

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
        public event Action<Dictionary<Card, int>> OnTrumpSuitRevealed;
        public event Action<Player> OnTrumpCardIsAceOTrumps;
        public event Action<Card, Player> OnCardDiscarded;

        //Turn Flow
        public event Action<Player> OnLeadPlayerSelected;
        public event Action<Card, Player> OnPlayerSteal;
        public event Action<Player> OnLeadPlayerTurn;
        public event Action<Player> OnPlayerTurnStarted;
        public event Action<Player, PlayerDecisionType, List<Card>> OnPlayerInputRequest;

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
        public event Action<GamePhase> OnGameStateChange;
        public event Action<Player> OnGameOver;
        public event Action OnNewGame;
        public event Action OnGameEnded;
        public event Action OnProgramClosed;

        public GameManager(
            RulesEngine rules,
            GameState gameState)
        {
            _rules = rules;
            _gameState = gameState;

            _turnManager = new TurnManager(_gameState.Players);

            // Initialize the properties in the constructor
            ChangeGameState(GamePhase.Initialize);
        }

        private void NewDeck()
        {
            _gameState.Deck = new Deck();
            Deck.Add52CardsToDeck();
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
            foreach (var amount in _rules.GetDealPattern())
            {
                for( int i = 0; i < Players.Count; i++ )
                {
                    _turnManager.RotateCurrentPlayer();
                    GivePlayerCards(_turnManager.CurrentPlayer, amount);
                }
            }
        }

        private void GivePlayerCards(Player player, int maxCards)
        {
            for (int i = 0; i < maxCards; i++)
            {
                var card = Deck.Draw();
                player.Hand.Add(card);
                OnCardDealtToPlayer?.Invoke(Deck, card, player);
            }
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

        private bool ArePlayersOutOfCards() => Players.All(p => p.Hand.Count == 0);

        private void ChangeGameState(GamePhase state)
        {
            CurrentPhase = state;
            OnGameStateChange?.Invoke(state);
        }

        private void ClearPlayersHands()
        {
            foreach (var player in Players)
                player.Hand.Clear();
        }

        private void ClearPlayedCards()
        {
            PlayedCards ??= new List<(Player player, Card card)>();
            PlayedCards.Clear();
        }

        private void ClearPlayerPoints()
        {
            foreach (var player in Players)
                player.Points = 0;
        }

        public bool IsGameOver()
        {
            if(CurrentPhase == GamePhase.EndGame) return true;

            return false;
        }

        public void StartGame()
        {
            ChangeGameState(GamePhase.Initialize);
        }
        public void AdvanceGame()
        {
            switch (CurrentPhase)
            {
                case GamePhase.NotStarted:

                case GamePhase.Initialize:

                    Initialize();
                    break;

                case GamePhase.DealCards:

                    HandleDealCards();
                    break;

                case GamePhase.HandleTrumps:

                    HandleTrumps();
                    break;

                case GamePhase.PlayerTurn_LeadStart:

                    PlayerTurn_LeadStart();
                    break;

                case GamePhase.PlayerTurn_StealDecision:

                    PlayerTurn_StealDecision();
                    break;

                case GamePhase.PlayerTurn_LeadPlayCard:

                    PlayerTurn_LeadPlayCard();
                    break;

                case GamePhase.PlayerTurn_Start:

                    PlayerTurn_Start();
                    break;

                case GamePhase.PlayerTurn_PlayCard:

                    PlayerTurn_PlayCard();
                    break;

                case GamePhase.Scoring:

                    Scoring();
                    break;

                case GamePhase.NewRound:

                    NewRound();
                    break;

                case GamePhase.AwaitingReplayDecision:

                    OnGameEnded?.Invoke();
                    break;

                case GamePhase.NewGame:

                    NewGame();
                    break;

                case GamePhase.EndGame:

                    break;

                case GamePhase.AwaitingPlayerInput:

                    break;
            }
        }

        private void RequestPlayerDecision(
            Player player,
            PlayerDecisionType type,
            List<Card> options = null)
        {
            if (player is PlayerCPU cpu)
            {
                PendingDecisionType = type;
                var chosen = cpu.Decide(type, options, TrumpCard, LedCard);
                SubmitPlayerAction(chosen);
            }
            else
            {
                PendingPlayer = player;
                PendingOptions = options;
                PendingDecisionType = type;

                ChangeGameState(GamePhase.AwaitingPlayerInput);
                OnPlayerInputRequest?.Invoke(player, type, options);
            }
        }

        public void SubmitPlayerAction(Card chosenCard)
        {
            switch (PendingDecisionType)
            {
                case PlayerDecisionType.FlipTrump:
                    AcceptTrumpCardFlip();
                    break;

                case PlayerDecisionType.LeadCard:
                    SubmitLeadCard(chosenCard);
                    break;

                case PlayerDecisionType.StealTrump:
                    DiscardCard(chosenCard);
                    break;

                case PlayerDecisionType.PlayCard:
                    SubmitPlayerCard(chosenCard);
                    break;
            }
        }

        private void Initialize()
        {
            if (_gameState.Players.All(p => p.Hand.Count == 0))
            {
                NewDeck();
                OnDeckCreated?.Invoke(Deck);

                Deck.Shuffle();
                OnDeckShuffled?.Invoke(Deck);
            }

            _turnManager.AssignRandomDealer(_rng);
            OnDealerSelected?.Invoke(_turnManager.Dealer);

            _turnManager.SetLeader(_turnManager.NextPlayer(_turnManager.Dealer));

            ResetCardsPlayed();

            ChangeGameState(GamePhase.DealCards);
            return;
        }

        private void HandleDealCards()  
        {
            _turnManager.ChangeToDealer();

            OnDealingStarted?.Invoke();

            if (_gameState.Players.All(p => p.Hand.Count == 0))
            {
                DealCards();
            }

            TrumpCard = Deck.Draw();

            RequestPlayerDecision(
                    _turnManager.CurrentPlayer,
                    PlayerDecisionType.FlipTrump,
                    null
                    );

            return;
        }

        public void AcceptTrumpCardFlip()
        {
            OnPlayerFlipsTrumpCard?.Invoke(TrumpCard, _turnManager.Dealer);

            TrumpStolen = false;

            ChangeGameState(GamePhase.HandleTrumps);
            return;
        }

        public void HandleTrumps()
        {
            var allCards = Deck.Cards.Concat(Deck.DealtCards);
            var trumpCards = _rules.GetTrumpCardsSorted(allCards, TrumpCard);

            var display = trumpCards.ToDictionary(
                card => card,
                card => _rules.GetCardScore(card, TrumpCard));

            OnTrumpSuitRevealed?.Invoke(display);

            ChangeGameState(GamePhase.PlayerTurn_LeadStart);
            return;
        }

        private void PlayerTurn_LeadStart()
        {
            _turnManager.ChangeToLeader();
            OnLeadPlayerTurn?.Invoke(_turnManager.CurrentPlayer);

            if (_rules.CanPlayerSteal(_turnManager.CurrentPlayer.Hand, TrumpCard))
            {
                ChangeGameState(GamePhase.PlayerTurn_StealDecision);
                return;
            }

            ChangeGameState(GamePhase.PlayerTurn_LeadPlayCard);
            return;
        }

        private void PlayerTurn_LeadPlayCard()
        { 
            OnRelayTrumpInfo?.Invoke(TrumpCard.GetSuitSymbolUnicoded());

            RequestPlayerDecision(
                    _turnManager.CurrentPlayer,
                    PlayerDecisionType.LeadCard,
                    null
                    );

            return;
        }

        public void SubmitLeadCard(Card chosenCard)
        { 
            SetLedCard(chosenCard);
            OnCardPlayed?.Invoke(chosenCard, _turnManager.CurrentPlayer);

            SetWinner(LedCard, _turnManager.CurrentPlayer);
            OnRoundNewWinner?.Invoke(chosenCard, _turnManager.CurrentPlayer);

            UpdatePlayedCards(_turnManager.CurrentPlayer, chosenCard);
            _turnManager.CurrentPlayer.Hand.Remove(chosenCard);

            OnRelayTrumpLeadInfo?.Invoke(TrumpCard.GetSuitSymbolUnicoded(), LedCard.GetSuitSymbolUnicoded());

            ChangeGameState(GamePhase.PlayerTurn_Start);
            return;
        }

        private void PlayerTurn_StealDecision()
        {
            RequestPlayerDecision(
                    _turnManager.CurrentPlayer,
                    PlayerDecisionType.StealTrump,
                    null
                    );

            return;
        }

        public void DiscardCard(Card droppedCard)
        {
            OnCardDiscarded?.Invoke(droppedCard, _turnManager.CurrentPlayer);
            _turnManager.CurrentPlayer.Hand.Remove(droppedCard);

            _turnManager.CurrentPlayer.Hand.Add(TrumpCard);
            OnPlayerSteal?.Invoke(TrumpCard, _turnManager.CurrentPlayer);

            TrumpStolen = true;

            if(_turnManager.CurrentPlayer == _turnManager.Leader)
                ChangeGameState(GamePhase.PlayerTurn_LeadPlayCard);
            else
                ChangeGameState(GamePhase.PlayerTurn_PlayCard);

            return;
        }

        private void PlayerTurn_Start()
        {
            _turnManager.RotateCurrentPlayer();
            OnPlayerTurnStarted?.Invoke(_turnManager.CurrentPlayer);

            if (_turnManager.IsCurrentPlayerTheLeader())
            {
                ChangeGameState(GamePhase.Scoring);
                return;
            }

            if (!TrumpStolen)
            {
                bool canPlayerSteal = false;

                if (_rules.CanPlayerSteal(_turnManager.CurrentPlayer.Hand, TrumpCard))
                    canPlayerSteal = true;

                else if (_turnManager.CurrentPlayer.Equals(_turnManager.Dealer) &&
                    _rules.IsTrumpCardStealable(TrumpCard))
                {
                    canPlayerSteal = true;
                    OnTrumpCardIsAceOTrumps?.Invoke(_turnManager.CurrentPlayer);
                }
                    

                if (canPlayerSteal)
                {
                    ChangeGameState(GamePhase.PlayerTurn_StealDecision);
                    return;
                }
            }

            ChangeGameState(GamePhase.PlayerTurn_PlayCard);
            return;
        }

        private void PlayerTurn_PlayCard()
        {
            var playableCards = _rules.GetPlayableCards(_turnManager.CurrentPlayer.Hand, TrumpCard, LedCard);

            RequestPlayerDecision(
                    _turnManager.CurrentPlayer,
                    PlayerDecisionType.PlayCard,
                    playableCards
                    );

            return;
        }

        public void SubmitPlayerCard(Card chosenCard)
        { 
            OnCardPlayed?.Invoke(chosenCard, _turnManager.CurrentPlayer);

            UpdatePlayedCards(_turnManager.CurrentPlayer, chosenCard);

            if (_rules.IsCardBetter(chosenCard, RoundWinningCard, LedCard, TrumpCard))
                SetWinner(chosenCard, _turnManager.CurrentPlayer);

            _turnManager.CurrentPlayer.Hand.Remove(chosenCard);

            OnRoundNewWinner?.Invoke(RoundWinningCard, RoundWinningPlayer);

            ChangeGameState(GamePhase.PlayerTurn_Start);
            return;
        }

        private void Scoring()
        {
            _rules.Scoring(RoundWinningPlayer);
            OnRoundEnded?.Invoke(RoundWinningCard, RoundWinningPlayer);

            OnScoreChanged?.Invoke(Players);

            if (_rules.IsGameOver(RoundWinningPlayer))
            {
                OnGameOver?.Invoke(RoundWinningPlayer);
                ChangeGameState(GamePhase.AwaitingReplayDecision);
                return;
            }

            if (ArePlayersOutOfCards())
            {
                ChangeGameState(GamePhase.NewRound);
                return;
            }

            _turnManager.SetLeader(RoundWinningPlayer); //Winner becomes leader
            OnLeadPlayerSelected?.Invoke(RoundWinningPlayer);

            ChangeGameState(GamePhase.PlayerTurn_LeadStart);
            return;
        }

        private void NewRound()
        {
            ResetCardsPlayed();

            NewDeck();
            OnDeckCreated?.Invoke(Deck);

            Deck.Shuffle();
            OnDeckShuffled?.Invoke(Deck);

            _turnManager.RotateDealer();
            OnDealerSelected?.Invoke(_turnManager.Dealer);

            _turnManager.SetLeader(_turnManager.NextPlayer(_turnManager.Dealer));

            ChangeGameState(GamePhase.DealCards);
            return;
        }

        public void NewGame()
        {
            ClearPlayersHands();
            ClearPlayedCards();
            ClearPlayerPoints();
            ResetCardsPlayed();

            OnNewGame?.Invoke();

            ChangeGameState(GamePhase.NewRound);
            return;
        }

        public void EndGame()
        {
            OnProgramClosed?.Invoke();
            ChangeGameState(GamePhase.EndGame);
            return;
        }
    }
}