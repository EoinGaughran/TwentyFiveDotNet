using System.Collections.Generic;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Interfaces
{
    internal interface IGameInteraction
    {
        void ShowPlayers(List<Player> players);
        void ShowPlayedCards(Dictionary<Player, Card> cards);
        void WaitForInput();
        string GetInput();
        bool PlayAgainQuestion(string message);
    }
}
