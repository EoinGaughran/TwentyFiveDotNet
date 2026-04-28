using System;
using System.Collections.Generic;
using TwentyFiveDotNet.Core.Models;

namespace TwentyFiveDotNet.Core.Game
{
    internal class TurnManager
    {
        private readonly List<Player> _players;

        public Player? CurrentPlayer { get; private set; }
        public Player? Leader { get; private set; }
        public Player? Dealer { get; private set; }

        public TurnManager(List<Player> players)
        {
            _players = players;
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
    }
}
