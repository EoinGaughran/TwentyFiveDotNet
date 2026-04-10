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
            LeadTurn,
            PlayerTurn,
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
        private Player Dealer { get; set; }
        private Player RoundWinningPlayer { get; set; }
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
            RulesEngine rules,
            List<Player> players)
        {
            _rules = rules;
            _players = players;

            _turnManager = new TurnManager(_players);

            // Initialize the properties in the constructor
            ChangeGameState(GameState.Initialize);
        }

        private void AssignRandomDealer()
        {
            Dealer = _players[_rng.Next(0, _players.Count)];
        }

        private void RotateDealer()
        {
            Dealer = _turnManager.NextPlayer(Dealer);
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
            _turnManager.ChangeToPlayer(Dealer);

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

        private void HandleStealing()
        {
            var droppedCard = _turnManager.CurrentPlayer.StealTrump(TrumpCard, LedCard);
            OnCardDiscarded?.Invoke(droppedCard, _turnManager.CurrentPlayer);
            _turnManager.CurrentPlayer.Hand.Remove(droppedCard);

            _turnManager.CurrentPlayer.Hand.Add(TrumpCard);
            OnPlayerSteal?.Invoke(TrumpCard, _turnManager.CurrentPlayer);

            TrumpStolen = true;
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

                case GameState.AwaitingReplayDecision:

                    OnGameEnded?.Invoke();
                    break;

                case GameState.NewGame:

                    NewGame();
                    break;

                case GameState.EndGame:

                    break;
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

            _turnManager.SetLeader(_turnManager.NextPlayer(Dealer));

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

            var allCards = Deck.Cards.Concat(Deck.DealtCards);
            var trumpCards = _rules.GetTrumpCardsSorted(allCards, TrumpCard);

            var display = trumpCards.ToDictionary(
                card => card,
                card => _rules.GetCardScore(card, TrumpCard));

            OnTrumpCardRevealed?.Invoke(display);

            TrumpStolen = false;

            ChangeGameState(GameState.LeadTurn);
            return;
        }

        private void LeadTurn()
        {
            _turnManager.ChangeToLeader();

            if (_rules.CanPlayerSteal(_turnManager.CurrentPlayer.Hand, TrumpCard))
            {
                HandleStealing();
            }

            _turnManager.SetLeader();
            OnLeadPlayerTurn?.Invoke(_turnManager.CurrentPlayer);
            OnRelayTrumpInfo?.Invoke(TrumpCard.GetSuitSymbolUnicoded());

            var chosenCard = _turnManager.CurrentPlayer.LeadCard();
            SetLedCard(chosenCard);
            OnCardPlayed?.Invoke(chosenCard, _turnManager.CurrentPlayer);

            SetWinner(LedCard, _turnManager.CurrentPlayer);
            OnRoundNewWinner?.Invoke(chosenCard, _turnManager.CurrentPlayer);

            UpdatePlayedCards(_turnManager.CurrentPlayer, chosenCard);
            _turnManager.CurrentPlayer.Hand.Remove(chosenCard);

            OnRelayTrumpLeadInfo?.Invoke(TrumpCard.GetSuitSymbolUnicoded(), LedCard.GetSuitSymbolUnicoded());

            ChangeGameState(GameState.PlayerTurn);
            return;
        }

        private void PlayerTurn()
        {
            _turnManager.RotateCurrentPlayer();

            if (_turnManager.IsCurrentPlayerTheLeader())
            {
                ChangeGameState(GameState.Scoring);
                return;
            }

            if (!HasPlayerStolen())
            {
                if(_rules.CanPlayerSteal(_turnManager.CurrentPlayer.Hand, TrumpCard))
                    HandleStealing();

                else if (_turnManager.CurrentPlayer.Equals(Dealer) &&
                    _rules.IsTrumpCardStealable(TrumpCard))
                    HandleStealing();
            }

            OnNextPlayerTurn?.Invoke(_turnManager.CurrentPlayer);

            var playableCards = _rules.GetPlayableCards(_turnManager.CurrentPlayer.Hand, TrumpCard, LedCard);

            var chosenCard = _turnManager.CurrentPlayer.ChooseCard(playableCards, TrumpCard, LedCard);

            OnCardPlayed?.Invoke(chosenCard, _turnManager.CurrentPlayer);

            UpdatePlayedCards(_turnManager.CurrentPlayer, chosenCard);

            if (_rules.IsCardBetter(chosenCard, RoundWinningCard, LedCard, TrumpCard))
                SetWinner(chosenCard, _turnManager.CurrentPlayer);

            _turnManager.CurrentPlayer.Hand.Remove(chosenCard);

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