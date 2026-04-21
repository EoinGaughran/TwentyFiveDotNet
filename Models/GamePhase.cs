using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwentyFiveDotNet.Models
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
