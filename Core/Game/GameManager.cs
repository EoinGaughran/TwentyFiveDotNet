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
        private readonly GameState _gameState;

        private static readonly Random _rng = new();

        //Messaging
        public event Action<GameState>? OnStateSnapshot;

        //Dealing
        public event Action<Player, Player>? OnRolesSelected;
        public event Action<GameSnapshot>? OnDealingCompleted;

        //Trump mechanics
        public event Action<TrumpData, Player, Deck>? OnTrumpResolved;
        public event Action<Card, Player>? OnCardDiscarded;

        //Turn Flow
        public event Action<Card, Player>? OnPlayerSteal;
        public event Action<Player>? OnPlayerTurnStarted;
        public event Action<Player, PlayerDecisionType, IReadOnlyList<Card>?>? OnPlayerInputRequest;
        public event Action<Player, int>? OnNewTrick;
        public event Action? OnHandEnd;

        //Card Play
        public event Action<CardPlayedEvent>? OnCardPlayed;

        //Scoring
        public event Action<List<Player>>? OnScoreChanged;
        public event Action<Card, Player, bool>? OnTrickNewWinner;
        public event Action<Card, Player>? OnTrickScored;

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
                    var currentPlayer = _gameState.RotateCurrentPlayer();
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

                case GamePhase.HandEnd:

                    HandEnd();
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
            IReadOnlyList<Card>? options)
        {
            _gameState.SetPendingPlayer(player);
            _gameState.SetPendingOptions(options);
            _gameState.SetPendingDecisionType(type);

            Card? trumpCard = null;
            Card? ledCard = null;

            if (player is PlayerCPU cpu)
            {
                switch(type)
                {
                    case PlayerDecisionType.LeadCard:
                        trumpCard = _gameState.GetTrumpCardOrThrow();
                        break;

                    case PlayerDecisionType.StealTrump:
                        trumpCard = _gameState.GetTrumpCardOrThrow();
                        break;

                    case PlayerDecisionType.PlayCard:
                        trumpCard = _gameState.GetTrumpCardOrThrow();
                        ledCard = _gameState.GetLedCardOrThrow();
                        break;
                }

                var chosen = cpu.Decide(
                    type,
                    options,
                    trumpCard,
                    ledCard);

                SubmitPlayerAction(chosen?.Id);
            }
            else
            {
                ChangeGamePhase(GamePhase.AwaitingPlayerInput);

                OnPlayerInputRequest?.Invoke(
                    player,
                    type,
                    options);
            }
        }

        public void SubmitPlayerAction(int? chosenCardID)
        {
            var pendingDecisionType = _gameState.GetPendingDecisionTypeOrThrow();

            switch (pendingDecisionType)
            {

                case PlayerDecisionType.FlipTrump:
                    AcceptTrumpCardFlip();
                    break;

                case PlayerDecisionType.LeadCard:
                    var card = ValidateCard(chosenCardID);
                    SubmitLeadCard(card);
                    break;

                case PlayerDecisionType.StealTrump:
                    card = ValidateCard(chosenCardID);
                    DiscardCard(card);
                    break;

                case PlayerDecisionType.PlayCard:
                    card = ValidateCard(chosenCardID);
                    SubmitPlayerCard(card);
                    break;
            }
        }

        public Card ValidateCard(int? chosenCardID)
        {
            var pendingOptions = _gameState.GetPendingOptionsOrThrow();

            if (chosenCardID == null)
                throw new InvalidOperationException("No card selected.");

            var card = pendingOptions
                .FirstOrDefault(c => c.Id == chosenCardID.Value);

            return card ?? throw new InvalidOperationException("Selected card is not legal.");
        }

        private void Initialize()
        {
            ChangeGamePhase(GamePhase.AssignRandomDealer);
            return;
        }

        private void AssignRandomDealer()
        {
            _gameState.AssignRandomDealer(_rng);

            ChangeGamePhase(GamePhase.NewTurn);
            return;
        }

        private void RotateDealer()
        {
            _gameState.RotateDealer();

            ChangeGamePhase(GamePhase.NewTurn);
            return;
        }

        private void NewTurn()
        {
            var dealer = _gameState.GetDealerOrThrow();

            _gameState.SetupTurn();

            var deck = _gameState.NewDeck();

            deck.Shuffle();

            var leader = _gameState.NextPlayer(dealer);

            _gameState.SetLeader(leader);

            OnRolesSelected?.Invoke(dealer, leader);

            ChangeGamePhase(GamePhase.DealCards);
            return;
        }

        private void HandleDealCards()  
        {
            var deck = _gameState.GetDeckOrThrow();
            var players = _gameState.GetPlayersOrThrow();
            var dealer = _gameState.GetDealerOrThrow();

            _gameState.ChangeToDealer();

            if (_gameState.Players.All(p => p.Hand.Count == 0))
            {
                DealCards(deck, players, dealer);
            }

            OnDealingCompleted?.Invoke(_gameState.CreateSnapshot());

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
            var dealer = _gameState.GetDealerOrThrow();

            var deck = _gameState.GetDeckOrThrow();

            var allCards = deck.Cards.Concat(deck.DealtCards);
            var trumpCards = _rules.GetTrumpCardsSorted(allCards, trumpCard);

            var display = trumpCards.ToDictionary(
                card => card,
                card => _rules.GetCardScore(card, trumpCard));

            TrumpData trumpData = new(trumpCard, display);

            OnTrumpResolved?.Invoke(trumpData, dealer, deck);

            ChangeGamePhase(GamePhase.PlayerTurn_LeadStart);
            return;
        }

        private void PlayerTurn_LeadStart()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();

            _gameState.ChangeToLeader();
            var currentPlayer = _gameState.GetCurrentPlayerOrThrow();

            bool isLeader = _gameState.IsCurrentPlayerTheLeader();

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
            var currentPlayer = _gameState.GetCurrentPlayerOrThrow();

            RequestPlayerDecision(
                    currentPlayer,
                    PlayerDecisionType.LeadCard,
                    currentPlayer.Hand
                    );

            return;
        }

        public void SubmitLeadCard(Card ledCard)
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _gameState.GetCurrentPlayerOrThrow();

            bool isLeader = _gameState.IsCurrentPlayerTheLeader();

            UpdatePlayedCards(
                currentPlayer,
                ledCard);

            currentPlayer.RemoveCard(ledCard);
            currentPlayer.AddCardToPlayedCards(ledCard);

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

            ChangeGamePhase(GamePhase.PlayerTurn_Start);
            return;
        }

        private void PlayerTurn_StealCheck()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _gameState.GetCurrentPlayerOrThrow();

            bool canPlayerSteal = false;

            if (_rules.CanPlayerSteal(
                currentPlayer.GetCards(),
                trumpCard))

                canPlayerSteal = true;

            else if (_gameState.IsCurrentPlayerTheDealer() &&
                _rules.IsTrumpCardStealable(trumpCard))
            {
                canPlayerSteal = true;
            }

            if (canPlayerSteal)
            {
                ChangeGamePhase(GamePhase.PlayerTurn_StealDecision);
                return;
            }

            if (_gameState.IsCurrentPlayerTheLeader())
                ChangeGamePhase(GamePhase.PlayerTurn_LeadPlayCard);
            else
                ChangeGamePhase(GamePhase.PlayerTurn_PlayCard);

            return;
        }

        private void PlayerTurn_StealDecision()
        {
            var currentPlayer = _gameState.GetCurrentPlayerOrThrow();

            RequestPlayerDecision(
                    currentPlayer,
                    PlayerDecisionType.StealTrump,
                    currentPlayer.Hand
                    );

            return;
        }

        public void DiscardCard(Card droppedCard)
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _gameState.GetCurrentPlayerOrThrow();

            currentPlayer.RemoveCard(droppedCard);

            OnCardDiscarded?.Invoke(
                droppedCard,
                currentPlayer);

            currentPlayer.AddCard(trumpCard);

            OnPlayerSteal?.Invoke(
                trumpCard,
                currentPlayer);

            _gameState.SetTrumpStolenBool(true);

            if (_gameState.IsCurrentPlayerTheLeader())
                ChangeGamePhase(GamePhase.PlayerTurn_LeadPlayCard);
            else
                ChangeGamePhase(GamePhase.PlayerTurn_PlayCard);

            return;
        }

        private void PlayerTurn_Start()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _gameState.RotateCurrentPlayer();

            bool isLeader = _gameState.IsCurrentPlayerTheLeader();

            OnPlayerTurnStarted?.Invoke(currentPlayer);

            if (_gameState.IsCurrentPlayerTheLeader())
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
            var currentPlayer = _gameState.GetCurrentPlayerOrThrow();

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
            var currentPlayer = _gameState.GetCurrentPlayerOrThrow();

            bool isLeader = _gameState.IsCurrentPlayerTheLeader();
            bool isDealer = _gameState.IsCurrentPlayerTheDealer();

            UpdatePlayedCards(
                currentPlayer,
                chosenCard);

            currentPlayer.RemoveCard(chosenCard);
            currentPlayer.AddCardToPlayedCards(chosenCard);

            OnCardPlayed?.Invoke(
                new CardPlayedEvent(
                currentPlayer,
                chosenCard,
                trumpCard.Suit,
                isLeader));

            if (_rules.IsCardBetter(
                chosenCard,
                trickWinningCard,
                ledCard,
                trumpCard))
            {
                _gameState.SetTrickResult(
                currentPlayer,
                chosenCard);

                trickWinningCard = _gameState.GetTrickWinningCardOrThrow();
                var trickWinningPlayer = _gameState.GetTrickWinningPlayerOrThrow();

                OnTrickNewWinner?.Invoke(
                    trickWinningCard,
                    trickWinningPlayer,
                    isDealer);
            }

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
                ChangeGamePhase(GamePhase.HandEnd);
                return;
            }

            _gameState.SetLeader(trickWinningPlayer); //Winner becomes leader

            ChangeGamePhase(GamePhase.PlayerTurn_LeadStart);

            return;
        }

        public void HandEnd()
        {
            OnHandEnd?.Invoke();
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