namespace TwentyFiveDotNet.Core.Models
{
    public enum GamePhase
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
}
