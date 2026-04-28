using System;
using System.Collections.Generic;
using System.Linq;
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
        public event Action<string>? OnRelayTrumpInfo;
        public event Action<string, string>? OnRelayTrumpLeadInfo;
        public event Action<GameState>? OnStateSnapshot;

        //Setup
        public event Action<Deck>? OnDeckCreated;
        public event Action<Deck>? OnDeckShuffled;

        //Dealing
        public event Action<Player>? OnDealerSelected;
        public event Action? OnDealingStarted;
        public event Action<Deck, Card, Player>? OnCardDealtToPlayer;

        //Trump mechanics
        public event Action<Card, Player>? OnPlayerFlipsTrumpCard;
        public event Action<Dictionary<Card, int>>? OnTrumpSuitRevealed;
        public event Action<Player>? OnTrumpCardIsAceOTrumps;
        public event Action<Card, Player>? OnCardDiscarded;

        //Turn Flow
        public event Action<Player>? OnLeadPlayerSelected;
        public event Action<Card, Player>? OnPlayerSteal;
        public event Action<Player>? OnLeadPlayerTurn;
        public event Action<Player>? OnPlayerTurnStarted;
        public event Action<Player, PlayerDecisionType, List<Card>?>? OnPlayerInputRequest;

        //Card Play
        public event Action<Card, Player>? OnLeadCardPlayed;
        public event Action<Card, Player>? OnCardPlayed;

        //Scoring/Rounds
        public event Action<List<Player>>? OnScoreChanged;
        public event Action<Card, Player>? OnRoundNewWinner;
        public event Action<Player>? OnDealersTrick;
        public event Action<Card, Player>? OnRoundEnded;
        public event Action? OnNewRound;

        //Game State
        public event Action<GamePhase>? OnGameStateChange;
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

        private Deck NewDeck()
        {
            _gameState.Deck = new Deck();
            _gameState.Deck.Add52CardsToDeck();

            return _gameState.Deck;
        }

        private void UpdatePlayedCards(Player player, Card chosenCard)
        {
            _gameState.PlayedCards.Add(new PlayedCard(player, chosenCard));
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
                player.Hand.Add(card);

                OnCardDealtToPlayer?.Invoke(
                    deck,
                    card,
                    player);
            }
        }

        private void SetLedCard(Card card)
        {
            _gameState.LedCard = card;
        }
        private void SetWinningCard(Card card)
        {
            _gameState.RoundWinningCard = card;
        }

        private void SetWinningPlayer(Player player)
        {
            _gameState.RoundWinningPlayer = player;
        }

        private static bool ArePlayersOutOfCards(List<Player> players)
            => players.All(p => p.Hand.Count == 0);

        private void ChangeGameState(GamePhase state)
        {
            _gameState.CurrentPhase = state;
            OnGameStateChange?.Invoke(state);
        }

        private static void ClearPlayersHands(List<Player> players)
        {
            foreach (var player in players)
                player.Hand.Clear();
        }

        private static void ClearPlayedCards(List<PlayedCard> playedCards)
        {
            playedCards.Clear();
        }

        private static void ClearPlayerPoints(List<Player> players)
        {
            foreach (var player in players)
                player.Points = 0;
        }

        public bool IsGameOver()
        {
            return _gameState.CurrentPhase == GamePhase.EndGame;
        }

        public void StartGame()
        {
            PublishState();
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
            List<Card>? options)
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
            if (_gameState.Players.All(p => p.Hand.Count == 0))
            {
                var deck = NewDeck();
                OnDeckCreated?.Invoke(deck);

                deck.Shuffle();
                OnDeckShuffled?.Invoke(deck);
            }
            
            var dealer = _turnManager.AssignRandomDealer(_rng);
            OnDealerSelected?.Invoke(dealer);

            _turnManager.SetLeader(_turnManager.NextPlayer(dealer));

            ClearPlayedCards(_gameState.PlayedCards);

            ChangeGameState(GamePhase.DealCards);
            return;
        }

        private void HandleDealCards()  
        {
            var deck = _gameState.GetDeckOrThrow();
            var players = _gameState.GetPlayersOrThrow();
            var dealer = _turnManager.GetDealerOrThrow();

            _turnManager.ChangeToDealer();

            OnDealingStarted?.Invoke();

            if (_gameState.Players.All(p => p.Hand.Count == 0))
            {
                DealCards(deck, players, dealer);
            }

            _gameState.TrumpCard = deck.Draw();

            RequestPlayerDecision(
                    dealer,
                    PlayerDecisionType.FlipTrump,
                    null);

            return;
        }

        public void AcceptTrumpCardFlip()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var dealer = _turnManager.GetDealerOrThrow();

            OnPlayerFlipsTrumpCard?.Invoke(
                trumpCard,
                dealer);

            _gameState.TrumpStolen = false;

            ChangeGameState(GamePhase.HandleTrumps);
            return;
        }

        public void HandleTrumps()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();

            var deck = _gameState.GetDeckOrThrow();

            var allCards = deck.Cards.Concat(deck.DealtCards);
            var trumpCards = _rules.GetTrumpCardsSorted(allCards, trumpCard);

            var display = trumpCards.ToDictionary(
                card => card,
                card => _rules.GetCardScore(card, trumpCard));

            OnTrumpSuitRevealed?.Invoke(display);

            ChangeGameState(GamePhase.PlayerTurn_LeadStart);
            return;
        }

        private void PlayerTurn_LeadStart()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            _turnManager.ChangeToLeader();
            OnLeadPlayerTurn?.Invoke(currentPlayer);

            if (_rules.CanPlayerSteal(
                currentPlayer.Hand,
                trumpCard))
            {
                ChangeGameState(GamePhase.PlayerTurn_StealDecision);
                return;
            }

            ChangeGameState(GamePhase.PlayerTurn_LeadPlayCard);
            return;
        }

        private void PlayerTurn_LeadPlayCard()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            OnRelayTrumpInfo?.Invoke(trumpCard.GetSuitSymbolUnicoded());

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

            SetLedCard(ledCard);
            OnLeadCardPlayed?.Invoke(
                ledCard,
                currentPlayer);

            SetWinningPlayer(currentPlayer);
            SetWinningCard(ledCard);

            OnRoundNewWinner?.Invoke(
                ledCard,
                currentPlayer);

            UpdatePlayedCards(
                currentPlayer,
                ledCard);

            currentPlayer.Hand.Remove(ledCard);

            OnRelayTrumpLeadInfo?.Invoke(
                trumpCard.GetSuitSymbolUnicoded(),
                ledCard.GetSuitSymbolUnicoded());

            ChangeGameState(GamePhase.PlayerTurn_Start);
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

            OnCardDiscarded?.Invoke(
                droppedCard,
                currentPlayer);

            currentPlayer.Hand.Remove(droppedCard);

            currentPlayer.Hand.Add(trumpCard);

            OnPlayerSteal?.Invoke(
                trumpCard,
                currentPlayer);

            _gameState.TrumpStolen = true;

            if(currentPlayer == _turnManager.Leader)
                ChangeGameState(GamePhase.PlayerTurn_LeadPlayCard);
            else
                ChangeGameState(GamePhase.PlayerTurn_PlayCard);

            return;
        }

        private void PlayerTurn_Start()
        {
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var currentPlayer = _turnManager.RotateCurrentPlayer();

            OnPlayerTurnStarted?.Invoke(currentPlayer);

            if (_turnManager.IsCurrentPlayerTheLeader())
            {
                ChangeGameState(GamePhase.Scoring);
                return;
            }

            if (!_gameState.TrumpStolen)
            {
                bool canPlayerSteal = false;

                if (_rules.CanPlayerSteal(
                    currentPlayer.Hand,
                    trumpCard))

                    canPlayerSteal = true;

                else if (_turnManager.IsCurrentPlayerTheDealer() &&
                    _rules.IsTrumpCardStealable(trumpCard))
                {
                    canPlayerSteal = true;
                    OnTrumpCardIsAceOTrumps?.Invoke(currentPlayer);
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
            var trumpCard = _gameState.GetTrumpCardOrThrow();
            var ledCard = _gameState.GetLedCardOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            var playableCards = _rules.GetPlayableCards(
                currentPlayer.Hand,
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
            var roundWinningCard = _gameState.GetRoundWinningCardOrThrow();
            var roundWinningPlayer = _gameState.GetRoundWinningPlayerOrThrow();
            var currentPlayer = _turnManager.GetCurrentPlayerOrThrow();

            OnCardPlayed?.Invoke(
                chosenCard,
                currentPlayer);

            UpdatePlayedCards(
                currentPlayer,
                chosenCard);

            if (_rules.IsCardBetter(
                chosenCard,
                roundWinningCard,
                ledCard,
                trumpCard))
            {
                SetWinningPlayer(currentPlayer);
                SetWinningCard(chosenCard);

                if (_turnManager.IsCurrentPlayerTheDealer())
                    OnDealersTrick?.Invoke(currentPlayer);
            }

            currentPlayer.Hand.Remove(chosenCard);

            OnRoundNewWinner?.Invoke(
                roundWinningCard,
                roundWinningPlayer);

            ChangeGameState(GamePhase.PlayerTurn_Start);
            return;
        }

        private void Scoring()
        {
            var players = _gameState.GetPlayersOrThrow();
            var roundWinningCard = _gameState.GetRoundWinningCardOrThrow();
            var roundWinningPlayer = _gameState.GetRoundWinningPlayerOrThrow();

            _rules.Scoring(roundWinningPlayer);

            OnRoundEnded?.Invoke(
                roundWinningCard,
                roundWinningPlayer);

            OnScoreChanged?.Invoke(players);

            if (_rules.IsGameOver(roundWinningPlayer))
            {
                OnGameOver?.Invoke(roundWinningPlayer);
                ChangeGameState(GamePhase.AwaitingReplayDecision);
                return;
            }

            if (ArePlayersOutOfCards(players))
            {
                ChangeGameState(GamePhase.NewRound);
                return;
            }

            _turnManager.SetLeader(roundWinningPlayer); //Winner becomes leader
            OnLeadPlayerSelected?.Invoke(roundWinningPlayer);

            ChangeGameState(GamePhase.PlayerTurn_LeadStart);
            return;
        }

        private void NewRound()
        {
            var deck = _gameState.GetDeckOrThrow();

            OnNewRound?.Invoke();

            ClearPlayedCards(_gameState.PlayedCards);

            NewDeck();
            OnDeckCreated?.Invoke(deck);

            deck.Shuffle();
            OnDeckShuffled?.Invoke(deck);
            
            var dealer = _turnManager.RotateDealer();

            OnDealerSelected?.Invoke(dealer);
            _turnManager.SetLeader(_turnManager.NextPlayer(dealer));

            ChangeGameState(GamePhase.DealCards);
            return;
        }

        public void NewGame()
        {
            var players = _gameState.GetPlayersOrThrow();

            ClearPlayersHands(players);
            ClearPlayedCards(_gameState.PlayedCards);
            ClearPlayerPoints(players);

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