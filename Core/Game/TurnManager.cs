using TwentyFiveDotNet.Core.Models;

namespace TwentyFiveDotNet.Core.Game
{
    internal class TurnManager
    {
        private List<Player> _players;

        public Player CurrentPlayer { get; private set; }
        public Player Leader { get; private set; }
        public Player Dealer { get; private set; }

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
        public void SetDealer()
        {
            Dealer = CurrentPlayer;
        }

        public void SetDealer(Player player)
        {
            Dealer = player;
        }
        public void AssignRandomDealer(Random _rng)
        {
            SetDealer(_players[_rng.Next(0, _players.Count)]);
        }
        public void RotateDealer()
        {
            Dealer = NextPlayer(Dealer);
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
