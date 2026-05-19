namespace TwentyFiveDotNet.Core.Models
{
    public enum GamePhase
    {
        NotStarted,
        Initialize,
        AssignRandomDealer,
        RotateDealer,
        DealCards,
        HandleTrumps,

        PlayerTurn_LeadStart,
        PlayerTurn_LeadPlayCard,
        PlayerTurn_Start,
        PlayerTurn_StealCheck,
        PlayerTurn_StealDecision,
        PlayerTurn_PlayCard,

        AwaitingPlayerInput,

        Scoring,
        NewTurn,
        HandEnd,
        TrickEnd,

        AwaitingReplayDecision,
        NewGame,
        EndGame
    }
}
