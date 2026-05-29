public readonly struct PlayerScoreViewData
{
    public readonly int PlayerId;
    public readonly string PlayerName;
    public readonly int Score;

    public PlayerScoreViewData(int playerId, string playerName, int score)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        Score = score;
    }
}
