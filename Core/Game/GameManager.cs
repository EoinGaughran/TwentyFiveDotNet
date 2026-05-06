using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models;
using TwentyFiveDotNet.Core.Models;

namespace TwentyFiveDotNet.Core.Game
{
    public class GameManager
    {
        private readonly RulesEngine _rules;
        private readonly TurnManager _turnManager;
        private readonly GameState _gameState;

        private static readonly Random _rng = new();

        //Messaging
        public event Action<GameState>? OnStateSnapshot;

        //Dealing
        public event Action<Player, Player>? OnRolesSelected;
        public event Action<GameState>? OnDealingCompleted;

        //Trump mechanics
        public event Action<TrumpData, Player>? OnTrumpResolved;
        public event Action<Card, Player>? OnCardDiscarded;

        //Turn Flow
        public event Action<Card, Player>? OnPlayerSteal;
        public event Action<Player>? OnPlayerTurnStarted;
        public event Action<Player, PlayerDecisionType, List<Card>?>? OnPlayerInputRequest;

        //Card Play
        public event Action<CardPlayedEvent>? OnCardPlayed;

        //Scoring
        public event Action<List<Player>>? OnScoreChanged;
        public event Action<Card, Player, bool>? OnTrickNewWinner;
        public event Action<Card, Player>? OnTrickScored;
        public event Action<Player, int>? OnNewTrick;

        //Game State
        public event Action<GamePhase>? OnGamePhaseChange;
        public event Action<Player>? OnGameOver;
        public event Action? OnNewGame;
        public event Action? OnGameEnded;
        public event Action? OnProgramClosed;

        public GameManager(
            RulesEngine rules,
            GameState gameState)
        {
            _rules = rules;
            _gameState = gameState;

            var players = _gameState.GetPlayersOrThrow();

            _turnManager = new TurnManager(players);
        }

        public void PublishState()
        {
            OnStateSnapshot?.Invoke(_gameState);
        }

        private void UpdatePlayedCards(Player player, Card chosenCard)
        {
            _gameState.AddPlayedCard(player, chosenCard);
        }

        private void DealCards(Deck deck, List<Player> players, Player dealer)
        {
            foreach (var amount in _rules.GetDealPattern())
            {
                for( int i = 0; i < players.Count; i++ )
                {
                    var currentPlayer = _turnManager.RotateCurrentPlayer();
                    GivePlayerCards(deck, currentPlayer, amount);
                }
            }
        }

        private void GivePlayerCards(Deck deck, Player player, int maxCards)
        {
            for (int i = 0; i < maxCards; i++)
            {
                var card = deck.Draw();
                player.AddCard(card);
            }
        }

        private void ChangeGamePhase(GamePhase phase)
        {
            _gameState.SetGamePhase(phase);
            OnGamePhaseChange?.Invoke(phase);
        }

        public bool IsGameOver()
        {
            return _gameState.CurrentPhase == GamePhase.EndGame;
        }

        public void StartGame()
        {
            AdvanceGame();
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

                case GamePhase.NewTurn:

                    NewTurn();
                    break;

                case GamePhase.AssignRandomDealer:

                    AssignRandomDealer();
                    break;

                case GamePhase.RotateDealer:

                    RotateDealer();
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

                case GamePhase.PlayerTurn_StealCheck:

                    PlayerTurn_StealCheck();
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

                case GamePhase.AwaitingReplayDecision:

                    OnGameEnded?.Invoke();
                    break;

                case GamePhase.TrickEnd:

                    TrickEnd();
                    break;

                case GamePhase.TurnEnd:

                    TurnEnd();
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
            List<Card>? options)
        {

            if (player is PlayerCPU cpu)
            {
                _gameState.SetPendingDecisionType(type);
                var chosen = cpu.Decide(
                    type,
                    options,
                    _gameState.TrumpCard,
                    _gameState.LedCard);

                SubmitPlayerAction(chosen);
            }
            else
            {
                _gameState.SetPendingPlayer(player);
                _gameState.SetPendingOptions(options);
                _gameState.SetPendingDecisionType(type);

                ChangeGamePhase(GamePhase.AwaitingPlayerInput);

                OnPlayerInputRequest?.Invoke(
                    player,
                    type,
                    options);
            }
        }

        public void SubmitPlayerAction(Card? chosenCard)
        {
            switch (_gameState.PendingDecisionType)
            {

                case PlayerDecisionType.FlipTrump:
                    AcceptTrumpCardFlip();
                    break;

                case PlayerDecisionType.LeadCard:
                    var card = ValidateCard(chosenCard);
                    SubmitLeadCard(card);
                    break;

                case PlayerDecisionType.StealTrump:
                    card = ValidateCard(chosenCard);
                    DiscardCard(card);
                    break;

                case PlayerDecisionType.PlayCard:
                    card = ValidateCard(chosenCard);
                    SubmitPlayerCard(card);
                    break;
            }
        }

        public static Card ValidateCard(Card? chosenCard)
        {
            return chosenCard ?? throw new InvalidOperationException("ChosenCard is null");
        }

        private void Initialize()
        {
            ChangeGamePhase(GamePhase.AssignRandomDealer);
            return;
        }

        private void AssignRandomDealer()
        {
            _turnManager.AssignRandomDealer(_rng);

            ChangeGamePhase(GamePhase.NewTurn);
            return;
        }

        private void RotateDealer()
        {
            _turnManager.RotateDealer();

            ChangeGamePhase(GamePhase.NewTurn);
            return;
        }

        private void NewTurn()
        {
            var dealer = _turnManager.GetDealerOrThrow();

            _gameState.SetupTurn();

            var deck = _gameState.NewDeck();

            deck.Shuffle();

            var leader = _turnManager.NextPlayer(dealer);

            _turnManager.SetLeader(leader);

            OnRolesSelected?.Invoke(dealer, leader);

            ChangeGamePhase(GamePhase.DealCards);
            return;
        }

        private void HandleDealCards()  
        {
            var deck = _gameState.GetDeckOrThrow();
            var players = _gameState.GetPlayersOrThrow();
            var dealer = _turnManager.GetDealerOrThrow();

            _turnManager.ChangeToDealer();

            if (_gameState.Players.All(p => p.Hand.Count == 0))
            {
                DealCards(deck, players, dealer);
            }

            OnDealingCompleted?.Invoke(_gameState);

            _gameState.SetTrumpCard(deck.Draw());

            RequestPlayerDecision(
                    dealer,
                    PlayerDecisionType.FlipTrump,
                    null);

            return;
        }

        public void AcceptTrumpCardFlip()
        {
            _gameState.SetTrumpStolenBool(false);

            ChangeGamePhase(GamePhase.HandleTrumps);
            return;
        }

        public void HandleTrumps()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var dealer = _turnManager.GetDealerOrThrow();

            var deck = _gameState.GetDeckOrThrow();

            var allCards = deck.Cards.Concat(deck.DealtCards);
            var trumpCards = _rules.GetTrumpCardsSorted(allCards, trumpCard);

            var display = trumpCards.ToDictionary(
                card => card,
                card => _rules.GetCardScore(card, trumpCard));

            TrumpData trumpData = new(trumpCard, display);

            OnTrumpResolved?.Invoke(trumpData, dealer);

            ChangeGamePhase(GamePhase.PlayerTurn_LeadStart);
            return;
        }

        private void PlayerTurn_LeadStart()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            _turnManager.ChangeToLeader();

            bool isLeader = _turnManager.IsCurrentPlayerTheLeader();

            OnNewTrick?.Invoke(currentPlayer, _gameState.TrickNumber);

            if (_gameState.TrickNumber == 1
                && !_gameState.TrumpStolen)
            {
                ChangeGamePhase(GamePhase.PlayerTurn_StealCheck);
                return;
            }

            ChangeGamePhase(GamePhase.PlayerTurn_LeadPlayCard);
            return;
        }

        private void PlayerTurn_LeadPlayCard()
        {
            _ = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            RequestPlayerDecision(
                    currentPlayer,
                    PlayerDecisionType.LeadCard,
                    null
                    );

            return;
        }

        public void SubmitLeadCard(Card ledCard)
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            bool isLeader = _turnManager.IsCurrentPlayerTheLeader();
            bool isDealer = _turnManager.IsCurrentPlayerTheDealer();

            _gameState.SetLedCard(ledCard);
            OnCardPlayed?.Invoke(
                new CardPlayedEvent(
                    currentPlayer,
                    ledCard,
                    trumpCard.Suit,
                    isLeader));

            _gameState.SetTrickResult(
                currentPlayer,
                ledCard);

            OnTrickNewWinner?.Invoke(
                ledCard,
                currentPlayer,
                isDealer);

            UpdatePlayedCards(
                currentPlayer,
                ledCard);

            currentPlayer.RemoveCard(ledCard);
            currentPlayer.AddCardToPlayedCards(ledCard);

            ChangeGamePhase(GamePhase.PlayerTurn_Start);
            return;
        }

        private void PlayerTurn_StealCheck()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            bool canPlayerSteal = false;

            if (_rules.CanPlayerSteal(
                currentPlayer.GetCards(),
                trumpCard))

                canPlayerSteal = true;

            else if (_turnManager.IsCurrentPlayerTheDealer() &&
                _rules.IsTrumpCardStealable(trumpCard))
            {
                canPlayerSteal = true;
            }

            if (canPlayerSteal)
            {
                ChangeGamePhase(GamePhase.PlayerTurn_StealDecision);
                return;
            }

            if (_turnManager.IsCurrentPlayerTheLeader())
                ChangeGamePhase(GamePhase.PlayerTurn_LeadPlayCard);
            else
                ChangeGamePhase(GamePhase.PlayerTurn_PlayCard);

            return;
        }

        private void PlayerTurn_StealDecision()
        {
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            RequestPlayerDecision(
                    currentPlayer,
                    PlayerDecisionType.StealTrump,
                    null
                    );

            return;
        }

        public void DiscardCard(Card droppedCard)
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            currentPlayer.RemoveCard(droppedCard);

            OnCardDiscarded?.Invoke(
                droppedCard,
                currentPlayer);

            currentPlayer.AddCard(trumpCard);

            OnPlayerSteal?.Invoke(
                trumpCard,
                currentPlayer);

            _gameState.SetTrumpStolenBool(true);

            if (_turnManager.IsCurrentPlayerTheLeader())
                ChangeGamePhase(GamePhase.PlayerTurn_LeadPlayCard);
            else
                ChangeGamePhase(GamePhase.PlayerTurn_PlayCard);

            return;
        }

        private void PlayerTurn_Start()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _turnManager.RotateCurrentPlayer();

            bool isLeader = _turnManager.IsCurrentPlayerTheLeader();

            OnPlayerTurnStarted?.Invoke(currentPlayer);

            if (_turnManager.IsCurrentPlayerTheLeader())
            {
                ChangeGamePhase(GamePhase.Scoring);
                return;
            }

            if (_gameState.TrickNumber == 1
                && !_gameState.TrumpStolen)
            {
                ChangeGamePhase(GamePhase.PlayerTurn_StealCheck);
                return;
            }

            ChangeGamePhase(GamePhase.PlayerTurn_PlayCard);
            return;
        }

        private void PlayerTurn_PlayCard()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var ledCard = _gameState.GetLedCardOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            var playableCards = _rules.GetPlayableCards(
                currentPlayer.GetCards(),
                trumpCard,
                ledCard);

            RequestPlayerDecision(
                    currentPlayer,
                    PlayerDecisionType.PlayCard,
                    playableCards
                    );

            return;
        }

        public void SubmitPlayerCard(Card chosenCard)
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var ledCard = _gameState.GetLedCardOrThrow();
            var trickWinningCard = _gameState.GetTrickWinningCardOrThrow();
            var trickWinningPlayer = _gameState.GetTrickWinningPlayerOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            bool isLeader = _turnManager.IsCurrentPlayerTheLeader();
            bool isDealer = _turnManager.IsCurrentPlayerTheDealer();

            OnCardPlayed?.Invoke(
                new CardPlayedEvent(
                currentPlayer,
                chosenCard,
                trumpCard.Suit,
                isLeader));

            UpdatePlayedCards(
                currentPlayer,
                chosenCard);

            if (_rules.IsCardBetter(
                chosenCard,
                trickWinningCard,
                ledCard,
                trumpCard))
            {
                _gameState.SetTrickResult(
                currentPlayer,
                ledCard);
            }

            currentPlayer.RemoveCard(chosenCard);
            currentPlayer.AddCardToPlayedCards(chosenCard);

            OnTrickNewWinner?.Invoke(
                trickWinningCard,
                trickWinningPlayer,
                isDealer);

            ChangeGamePhase(GamePhase.PlayerTurn_Start);
            return;
        }

        private void Scoring()
        {
            var players = _gameState.GetPlayersOrThrow();
            var trickWinningCard = _gameState.GetTrickWinningCardOrThrow();
            var trickWinningPlayer = _gameState.GetTrickWinningPlayerOrThrow();

            _rules.Scoring(trickWinningPlayer);

            OnTrickScored?.Invoke(
                trickWinningCard,
                trickWinningPlayer);

            OnScoreChanged?.Invoke(players);

            if (_rules.IsGameOver(trickWinningPlayer))
            {
                OnGameOver?.Invoke(trickWinningPlayer);
                ChangeGamePhase(GamePhase.AwaitingReplayDecision);
                return;
            }

            ChangeGamePhase(GamePhase.TrickEnd);
            return;
        }

        public void TrickEnd()
        {
            var trickWinningPlayer = _gameState.GetTrickWinningPlayerOrThrow();

            _gameState.IncreaseTrickCounter();

            if (_gameState.ArePlayersOutOfCards())
            {
                ChangeGamePhase(GamePhase.TurnEnd);
                return;
            }

            _turnManager.SetLeader(trickWinningPlayer); //Winner becomes leader

            ChangeGamePhase(GamePhase.PlayerTurn_LeadStart);

            return;
        }

        public void TurnEnd()
        {
            ChangeGamePhase(GamePhase.RotateDealer);
            return;
        }
        public void NewGame()
        {
            _gameState.NewGame();

            OnNewGame?.Invoke();

            ChangeGamePhase(GamePhase.RotateDealer);
            return;
        }

        public void EndGame()
        {
            OnProgramClosed?.Invoke();
            ChangeGamePhase(GamePhase.EndGame);
            return;
        }
    }
}