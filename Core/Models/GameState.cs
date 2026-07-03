using System;
using System.Collections.Generic;
using System.Linq;

namespace TwentyFiveDotNet.Core.Models
{
    public class GameState
    {
        public IReadOnlyList<Player> Players => _players;
        private readonly List<Player> _players = new();
        public Deck Deck { get; private set; } = new Deck();

        // Turn structure
        public int CurrentPlayerIndex { get; private set; } = 0;
        public int TrickNumber { get; private set; } = 1;
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Initialize;

        // Cards in play
        public IReadOnlyList<PlayedCard> PlayedCards => _playedCards;
        private readonly List<PlayedCard> _playedCards = new();

        // Turn
        public Player? CurrentPlayer { get; private set; }
        public Player? Leader { get; private set; }
        public Player? Dealer { get; private set; }

        // Trump & trick info
        public Card? TrumpCard { get; private set; }
        public Card? LedCard { get; private set; }
        public Card? TrickWinningCard { get; private set; }
        public Player? TrickWinningPlayer { get; private set; }
        public Player? PendingPlayer { get; private set; }
        public IReadOnlyList<Card>? PendingOptions { get; private set; } = new List<Card>();
        public PlayerDecisionType? PendingDecisionType { get; private set; }

        public bool TrumpStolen { get; private set; } = false;

        public List<Player> GetPlayersOrThrow()
        {
            if(_players.Count == 0)
                throw new InvalidOperationException("No Players were set");

            return _players;
        }
        public Deck GetDeckOrThrow()
        {
            return Deck ?? throw new InvalidOperationException("No deck was set");
        }

        public Card GetTrumpCardOrThrow()
        {
            return TrumpCard ?? throw new InvalidOperationException("Trump not set");
        }

        public Card GetLedCardOrThrow()
        {
            return LedCard ?? throw new InvalidOperationException("LedCard not set");
        }

        public Card GetTrickWinningCardOrThrow()
        {
            return TrickWinningCard ?? throw new InvalidOperationException("TrickWinningCard not set");
        }

        public Player GetTrickWinningPlayerOrThrow()
        {
            return TrickWinningPlayer ?? throw new InvalidOperationException("TrickWinningPlayer not set");
        }

        public Deck NewDeck()
        {
            Deck = new Deck();
            Deck.Add52CardsToDeck();

            return Deck;
        }

        public void AddPlayers(List<Player> players)
        {
            foreach(Player p in players)
                _players.Add(p);
        }

        public void AddPlayer(Player player)
        {
            _players.Add(player);
        }

        public void AddPlayedCard(Player player, Card card)
        {
            _playedCards.Add(new PlayedCard(player, card));
        }

        public Player GetCurrentPlayerOrThrow()
        {
            return CurrentPlayer ?? throw new InvalidOperationException("CurrentPlayer was never set");
        }
        public Player GetDealerOrThrow()
        {
            return Dealer ?? throw new InvalidOperationException("Dealer player was never set");
        }

        public Player RotateCurrentPlayer()
        {
            var player = GetCurrentPlayerOrThrow();
            CurrentPlayer = NextPlayer(player);
            return CurrentPlayer;
        }

        public Player NextPlayer(Player player)
        {
            int index = _players.IndexOf(player);

            if (index == -1)
                throw new InvalidOperationException("Player not found in player list");

            int nextIndex = (index + 1) % _players.Count;
            return _players[nextIndex];
        }

        public void ChangeToPlayer(Player player)
        {
            CurrentPlayer = player;
        }
        public void SetLeader(Player player)
        {
            Leader = player;
        }
        public void SetLeader()
        {
            Leader = CurrentPlayer;
        }

        public Player SetDealer(Player player)
        {
            Dealer = player;
            return Dealer ?? throw new InvalidOperationException("The dealer was never set");
        }
        public Player AssignRandomDealer(Random _rng)
        {
            return SetDealer(_players[_rng.Next(0, _players.Count)]);
        }
        public Player RotateDealer()
        {
            var player = GetDealerOrThrow();
            Dealer = NextPlayer(player);
            return Dealer;
        }

        public void ChangeToLeader()
        {
            CurrentPlayer = Leader;
        }

        public void ChangeToDealer()
        {
            CurrentPlayer = Dealer;
        }

        public bool IsCurrentPlayerTheLeader()
        {
            return CurrentPlayer == Leader;
        }

        public bool IsCurrentPlayerTheDealer()
        {
            return CurrentPlayer == Dealer;
        }

        public void SetLedCard(Card card)
        {
            LedCard = card;
        }
        public void SetTrickResult(Player player, Card card)
        {
            TrickWinningPlayer = player;
            TrickWinningCard = card;
        }
        public void SetTrumpCard(Card card)
        {
            TrumpCard = card;
        }

        public void SetTrumpStolenBool(bool value)
        {
            TrumpStolen = value;
        }

        public void SetGamePhase(GamePhase gamePhase)
        {
            CurrentPhase = gamePhase;
        }

        public void SetPendingDecisionType(PlayerDecisionType decisionType)
        {
            PendingDecisionType = decisionType;
        }

        public void SetPendingPlayer(Player player)
        {
            PendingPlayer = player;
        }

        public void SetPendingOptions(IReadOnlyList<Card>? options)
        {
            PendingOptions = options;
        }

        public void ClearPlayerPoints()
        {
            foreach (var player in Players)
                player.ResetPoints();
        }
        public void ClearPlayedCards()
        {
            _playedCards.Clear();

            foreach (var player in Players)
                player.ClearPlayedCards();
        }
        public bool ArePlayersOutOfCards()
            => Players.All(p => p.Hand.Count == 0);

        public void ClearPlayersHands()
        {
            foreach (var player in Players)
                player.ClearHand();
        }

        public void SetupTurn()
        {
            ClearPlayersHands();
            ClearPlayedCards();
            TrumpStolen = false;
            LedCard = null;
            TrickWinningCard = null;
            TrickWinningPlayer = null;
            TrickNumber = 1;
        }

        public void IncreaseTrickCounter()
        {
            TrickNumber += 1;
        }

        public void NewGame()
        {
            SetupTurn();
            ClearPlayerPoints();
        }
    }
}