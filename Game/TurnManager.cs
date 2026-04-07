using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Game
{
    internal class TurnManager
    {
        private List<Player> _players;

        public Player CurrentPlayer { get; private set; }
        public Player Leader { get; private set; }

        public TurnManager(List<Player> players)
        {
            _players = players;
        }

        public void RotateCurrentPlayer()
        {
            CurrentPlayer = NextPlayer(CurrentPlayer);
        }

        public Player NextPlayer(Player player)
        {
            int index = _players.IndexOf(player);

            if (index == -1)
                throw new InvalidOperationException("Player not found in player list.");

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

        public void ChangeToLeader()
        {
            CurrentPlayer = Leader;
        }

        public bool IsCurrentPlayerTheLeader()
        {
            return CurrentPlayer == Leader;
        }
    }
}
