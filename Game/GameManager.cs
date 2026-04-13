using System;
using System.Collections.Generic;
using System.Linq;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Game
{
    internal class GameManager
    {
        public enum GameState
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

        private static readonly Random _rng = new();

        private readonly List<Player> _players;
        private List<(Player player, Card card)> PlayedCards { get; set; }
        private Deck Deck { get; set; }
        private GameState CurrentState { get; set; }
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
        public event Action<GameState> OnGameStateChange;
        public event Action<Player> OnGameOver;
        public event Action OnNewGame;
        public event Action OnGameEnded;
        public event Action OnProgramClosed;

        public GameManager(
            RulesEngine rules,
            List<Player> players)
        {
            _rules = rules;
            _players = players;

            _turnManager = new TurnManager(_players);

            // Initialize the properties in the constructor
            ChangeGameState(GameState.Initialize);
        }

        private void NewDeck()
        {
            Deck = new Deck();
            Deck.Add52CardsToDeck();
        }

        private void ResetCardsPlayed()
        {
            PlayedCards = [];
        }

        private void UpdatePlayedCards(Player player, Card chosenCard)
        {
            PlayedCards.Add((player, chosenCard));
        }

        private void DealCards()
        {
            _turnManager.ChangeToPlayer(_turnManager.Dealer);

            foreach (var amount in _rules.GetDealPattern())
            {
                for( int i = 0; i < _players.Count; i++ )
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

        private bool ArePlayersOutOfCards() => _players.All(p => p.Hand.Count == 0);

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

        public bool IsGameOver()
        {
            if(CurrentState == GameState.EndGame) return true;

            return false;
        }

        public void StartGame()
        {
            ChangeGameState(GameState.Initialize);
        }
        public void AdvanceGame()
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

                case GameState.HandleTrumps:

                    HandleTrumps();
                    break;

                case GameState.PlayerTurn_LeadStart:

                    PlayerTurn_LeadStart();
                    break;

                case GameState.PlayerTurn_StealDecision:

                    PlayerTurn_StealDecision();
                    break;

                case GameState.PlayerTurn_LeadPlayCard:

                    PlayerTurn_LeadPlayCard();
                    break;

                case GameState.PlayerTurn_Start:

                    PlayerTurn_Start();
                    break;

                case GameState.PlayerTurn_PlayCard:

                    PlayerTurn_PlayCard();
                    break;

                case GameState.Scoring:

                    Scoring();
                    break;

                case GameState.NewRound:

                    NewRound();
                    break;

                case GameState.AwaitingReplayDecision:

                    OnGameEnded?.Invoke();
                    break;

                case GameState.NewGame:

                    NewGame();
                    break;

                case GameState.EndGame:

                    break;

                case GameState.AwaitingPlayerInput:

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

                ChangeGameState(GameState.AwaitingPlayerInput);
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
            NewDeck();
            OnDeckCreated?.Invoke(Deck);

            Deck.Shuffle();
            OnDeckShuffled?.Invoke(Deck);

            _turnManager.AssignRandomDealer(_rng);
            OnDealerSelected?.Invoke(_turnManager.Dealer);

            _turnManager.SetLeader(_turnManager.NextPlayer(_turnManager.Dealer));

            ResetCardsPlayed();

            ChangeGameState(GameState.DealCards);
            return;
        }

        private void HandleDealCards()  
        {
            OnDealingStarted?.Invoke();

            DealCards();

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

            ChangeGameState(GameState.HandleTrumps);
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

            ChangeGameState(GameState.PlayerTurn_LeadStart);
            return;
        }

        private void PlayerTurn_LeadStart()
        {
            _turnManager.ChangeToLeader();
            OnLeadPlayerTurn?.Invoke(_turnManager.CurrentPlayer);

            if (_rules.CanPlayerSteal(_turnManager.CurrentPlayer.Hand, TrumpCard))
            {
                ChangeGameState(GameState.PlayerTurn_StealDecision);
                return;
            }

            ChangeGameState(GameState.PlayerTurn_LeadPlayCard);
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

            ChangeGameState(GameState.PlayerTurn_Start);
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
                ChangeGameState(GameState.PlayerTurn_LeadPlayCard);
            else
                ChangeGameState(GameState.PlayerTurn_PlayCard);

            return;
        }

        private void PlayerTurn_Start()
        {
            _turnManager.RotateCurrentPlayer();
            OnPlayerTurnStarted?.Invoke(_turnManager.CurrentPlayer);

            if (_turnManager.IsCurrentPlayerTheLeader())
            {
                ChangeGameState(GameState.Scoring);
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
                    ChangeGameState(GameState.PlayerTurn_StealDecision);
                    return;
                }
            }

            ChangeGameState(GameState.PlayerTurn_PlayCard);
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

            ChangeGameState(GameState.PlayerTurn_Start);
            return;
        }

        private void Scoring()
        {
            _rules.Scoring(RoundWinningPlayer);
            OnRoundEnded?.Invoke(RoundWinningCard, RoundWinningPlayer);

            OnScoreChanged?.Invoke(_players);

            if (_rules.IsGameOver(RoundWinningPlayer))
            {
                OnGameOver?.Invoke(RoundWinningPlayer);
                ChangeGameState(GameState.AwaitingReplayDecision);
                return;
            }

            if (ArePlayersOutOfCards())
            {
                ChangeGameState(GameState.NewRound);
                return;
            }

            _turnManager.SetLeader(RoundWinningPlayer); //Winner becomes leader
            OnLeadPlayerSelected?.Invoke(RoundWinningPlayer);

            ChangeGameState(GameState.PlayerTurn_LeadStart);
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