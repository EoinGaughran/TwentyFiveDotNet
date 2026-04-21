using System;
using System.Collections.Generic;
using System.Linq;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Game
{
    internal class GameManager
    {
        private readonly RulesEngine _rules;
        private readonly TurnManager _turnManager;
        private readonly GameState _gameState;

        private static readonly Random _rng = new();

        //Messaging
        public event Action<string> OnMessage;
        public event Action<string> OnRelayTrumpInfo;
        public event Action<string, string> OnRelayTrumpLeadInfo;
        public event Action<GameState> OnStateSnapshot;

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

            ValidateState();

            _turnManager = new TurnManager(_gameState.Players);
        }
        private void ValidateState()
        {
            if (_gameState.Players == null || _gameState.Players.Count == 0)
                throw new InvalidOperationException("GameState is missing players");
        }

        public void PublishState()
        {
            OnStateSnapshot?.Invoke(_gameState);
        }

        private void NewDeck()
        {
            _gameState.Deck = new Deck();
            _gameState.Deck.Add52CardsToDeck();
        }

        private void ResetCardsPlayed()
        {
            _gameState.PlayedCards.Clear();
        }

        private void UpdatePlayedCards(Player player, Card chosenCard)
        {
            _gameState.PlayedCards.Add((player, chosenCard));
        }

        private void DealCards()
        {
            foreach (var amount in _rules.GetDealPattern())
            {
                for( int i = 0; i < _gameState.Players.Count; i++ )
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
                var card = _gameState.Deck.Draw();
                player.Hand.Add(card);

                OnCardDealtToPlayer?.Invoke(
                    _gameState.Deck,
                    card,
                    player);
            }
        }

        private void SetLedCard(Card card)
        {
            _gameState.LedCard = card;
        }
        private void SetWinner(Card card, Player player)
        {
            _gameState.RoundWinningCard = card;
            _gameState.RoundWinningPlayer = player;
        }

        private bool ArePlayersOutOfCards()
            => _gameState.Players.All(p => p.Hand.Count == 0);

        private void ChangeGameState(GamePhase state)
        {
            _gameState.CurrentPhase = state;
            OnGameStateChange?.Invoke(state);
        }

        private void ClearPlayersHands()
        {
            foreach (var player in _gameState.Players)
                player.Hand.Clear();
        }

        private void ClearPlayedCards()
        {
            _gameState.PlayedCards.Clear();
        }

        private void ClearPlayerPoints()
        {
            foreach (var player in _gameState.Players)
                player.Points = 0;
        }

        public bool IsGameOver()
        {
            if(_gameState.CurrentPhase == GamePhase.EndGame)
                return true;

            return false;
        }

        public void StartGame()
        {
            ChangeGameState(GamePhase.Initialize);
        }
        public void AdvanceGame()
        {
            switch (_gameState.CurrentPhase)
            {
                case GamePhase.NotStarted:

                    //Do nothing
                    break;

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
                _gameState.PendingDecisionType = type;
                var chosen = cpu.Decide(
                    type,
                    options,
                    _gameState.TrumpCard,
                    _gameState.LedCard);

                SubmitPlayerAction(chosen);
            }
            else
            {
                _gameState.PendingPlayer = player;
                _gameState.PendingOptions = options;
                _gameState.PendingDecisionType = type;

                ChangeGameState(GamePhase.AwaitingPlayerInput);

                OnPlayerInputRequest?.Invoke(
                    player,
                    type,
                    options);
            }
        }

        public void SubmitPlayerAction(Card chosenCard)
        {
            switch (_gameState.PendingDecisionType)
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
                OnDeckCreated?.Invoke(_gameState.Deck);

                _gameState.Deck.Shuffle();
                OnDeckShuffled?.Invoke(_gameState.Deck);
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

            _gameState.TrumpCard = _gameState.Deck.Draw();

            RequestPlayerDecision(
                    _turnManager.CurrentPlayer,
                    PlayerDecisionType.FlipTrump,
                    null);

            return;
        }

        public void AcceptTrumpCardFlip()
        {
            OnPlayerFlipsTrumpCard?.Invoke(
                _gameState.TrumpCard,
                _turnManager.Dealer);

            _gameState.TrumpStolen = false;

            ChangeGameState(GamePhase.HandleTrumps);
            return;
        }

        public void HandleTrumps()
        {
            var allCards = _gameState.Deck.Cards.Concat(_gameState.Deck.DealtCards);
            var trumpCards = _rules.GetTrumpCardsSorted(allCards, _gameState.TrumpCard);

            var display = trumpCards.ToDictionary(
                card => card,
                card => _rules.GetCardScore(card, _gameState.TrumpCard));

            OnTrumpSuitRevealed?.Invoke(display);

            ChangeGameState(GamePhase.PlayerTurn_LeadStart);
            return;
        }

        private void PlayerTurn_LeadStart()
        {
            _turnManager.ChangeToLeader();
            OnLeadPlayerTurn?.Invoke(_turnManager.CurrentPlayer);

            if (_rules.CanPlayerSteal(
                _turnManager.CurrentPlayer.Hand,
                _gameState.TrumpCard))
            {
                ChangeGameState(GamePhase.PlayerTurn_StealDecision);
                return;
            }

            ChangeGameState(GamePhase.PlayerTurn_LeadPlayCard);
            return;
        }

        private void PlayerTurn_LeadPlayCard()
        { 
            OnRelayTrumpInfo?.Invoke(_gameState.TrumpCard.GetSuitSymbolUnicoded());

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
            OnCardPlayed?.Invoke(
                chosenCard,
                _turnManager.CurrentPlayer);

            SetWinner(
                _gameState.LedCard,
                _turnManager.CurrentPlayer);

            OnRoundNewWinner?.Invoke(
                chosenCard,
                _turnManager.CurrentPlayer);

            UpdatePlayedCards(
                _turnManager.CurrentPlayer,
                chosenCard);

            _turnManager.CurrentPlayer.Hand.Remove(chosenCard);

            OnRelayTrumpLeadInfo?.Invoke(
                _gameState.TrumpCard.GetSuitSymbolUnicoded(),
                _gameState.LedCard.GetSuitSymbolUnicoded());

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
            OnCardDiscarded?.Invoke(
                droppedCard,
                _turnManager.CurrentPlayer);

            _turnManager.CurrentPlayer.Hand.Remove(droppedCard);

            _turnManager.CurrentPlayer.Hand.Add(_gameState.TrumpCard);

            OnPlayerSteal?.Invoke(
                _gameState.TrumpCard,
                _turnManager.CurrentPlayer);

            _gameState.TrumpStolen = true;

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

            if (!_gameState.TrumpStolen)
            {
                bool canPlayerSteal = false;

                if (_rules.CanPlayerSteal(
                    _turnManager.CurrentPlayer.Hand,
                    _gameState.TrumpCard))

                    canPlayerSteal = true;

                else if (_turnManager.CurrentPlayer.Equals(_turnManager.Dealer) &&
                    _rules.IsTrumpCardStealable(_gameState.TrumpCard))
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
            var playableCards = _rules.GetPlayableCards(
                _turnManager.CurrentPlayer.Hand,
                _gameState.TrumpCard,
                _gameState.LedCard);

            RequestPlayerDecision(
                    _turnManager.CurrentPlayer,
                    PlayerDecisionType.PlayCard,
                    playableCards
                    );

            return;
        }

        public void SubmitPlayerCard(Card chosenCard)
        { 
            OnCardPlayed?.Invoke(
                chosenCard,
                _turnManager.CurrentPlayer);

            UpdatePlayedCards(
                _turnManager.CurrentPlayer,
                chosenCard);

            if (_rules.IsCardBetter(
                chosenCard,
                _gameState.RoundWinningCard,
                _gameState.LedCard,
                _gameState.TrumpCard))

                SetWinner(
                    chosenCard,
                    _turnManager.CurrentPlayer);

            _turnManager.CurrentPlayer.Hand.Remove(chosenCard);

            OnRoundNewWinner?.Invoke(
                _gameState.RoundWinningCard,
                _gameState.RoundWinningPlayer);

            ChangeGameState(GamePhase.PlayerTurn_Start);
            return;
        }

        private void Scoring()
        {
            _rules.Scoring(_gameState.RoundWinningPlayer);

            OnRoundEnded?.Invoke(
                _gameState.RoundWinningCard,
                _gameState.RoundWinningPlayer);

            OnScoreChanged?.Invoke(_gameState.Players);

            if (_rules.IsGameOver(_gameState.RoundWinningPlayer))
            {
                OnGameOver?.Invoke(_gameState.RoundWinningPlayer);
                ChangeGameState(GamePhase.AwaitingReplayDecision);
                return;
            }

            if (ArePlayersOutOfCards())
            {
                ChangeGameState(GamePhase.NewRound);
                return;
            }

            _turnManager.SetLeader(_gameState.RoundWinningPlayer); //Winner becomes leader
            OnLeadPlayerSelected?.Invoke(_gameState.RoundWinningPlayer);

            ChangeGameState(GamePhase.PlayerTurn_LeadStart);
            return;
        }

        private void NewRound()
        {
            ResetCardsPlayed();

            NewDeck();
            OnDeckCreated?.Invoke(_gameState.Deck);

            _gameState.Deck.Shuffle();
            OnDeckShuffled?.Invoke(_gameState.Deck);

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