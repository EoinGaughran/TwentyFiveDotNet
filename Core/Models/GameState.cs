using System;
using System.Collections.Generic;
using System.Linq;
using TwentyFiveDotNet.Core.Config;

namespace TwentyFiveDotNet.Core.Models
{
    public class GameState
    {
        // Core setup
        public IReadOnlyList<Player> Players => _players;
        private readonly List<Player> _players = new();
        public Deck Deck { get; private set; } = new Deck();

        // Turn structure
        public int CurrentPlayerIndex { get; private set; } = 0;
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Initialize;

        // Cards in play
        public IReadOnlyList<PlayedCard> PlayedCards => _playedCards;
        private readonly List<PlayedCard> _playedCards = new();

        // Trump & trick info
        public Card? TrumpCard { get; private set; }
        public Card? LedCard { get; private set; }
        public Card? RoundWinningCard { get; private set; }
        public Player? RoundWinningPlayer { get; private set; }
        public Player? PendingPlayer { get; private set; }
        public List<Card>? PendingOptions { get; private set; } = new List<Card>();
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

        public Card GetRoundWinningCardOrThrow()
        {
            return RoundWinningCard ?? throw new InvalidOperationException("RoundWinningCard not set");
        }

        public Player GetRoundWinningPlayerOrThrow()
        {
            return RoundWinningPlayer ?? throw new InvalidOperationException("RoundWinningPlayer not set");
        }

        public Player GetPendingPlayerOrThrow()
        {
            return PendingPlayer ?? throw new InvalidOperationException("PendingPlayer not set");
        }

        public List<Card> GetPendingOptionsOrThrow()
        {
            return PendingOptions ?? throw new InvalidOperationException("PendingOptions not set");
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
        public void SetLedCard(Card card)
        {
            LedCard = card;
        }
        public void SetTrickResult(Player player, Card card)
        {
            RoundWinningPlayer = player;
            RoundWinningCard = card;
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

        public void SetPendingOptions(List<Card>? options)
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
        }
        public bool ArePlayersOutOfCards()
            => Players.All(p => p.Hand.Count == 0);

        public void ClearPlayersHands()
        {
            foreach (var player in Players)
                player.ClearHand();
        }

        public void StartNewRound()
        {
            ClearPlayersHands();
            ClearPlayedCards();
            TrumpStolen = false;
            LedCard = null;
            RoundWinningCard = null;
            RoundWinningPlayer = null;
        }
    }
}