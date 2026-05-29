public class PendingGameState
{
    public readonly string PlayerName;
    public readonly int TotalPlayers;

    public PendingGameState(string playerName, int totalPlayers)
    {
        PlayerName = playerName;
        TotalPlayers = totalPlayers;
    }
}