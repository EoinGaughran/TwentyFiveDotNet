using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Interfaces
{
    internal interface IGameInteraction
    {
        void ShowMessage(string message);
        void ShowPlayers(List<Player> players);
        void ShowPlayedCards(Dictionary<Player, Card> cards);

        void WaitForInput();
        string GetInput();
        bool PlayAgainQuestion(string message);
    }
}
