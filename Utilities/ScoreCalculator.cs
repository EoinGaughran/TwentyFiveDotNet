using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Utilities
{
    public class ScoreCalculator
    {
        public static Dictionary<Player, int> CalculateScores(List<Player> players, List<Card> tableCards)
        {
            var scores = new Dictionary<Player, int>();

            foreach (var player in players)
            {
                // Example: Calculate score based on cards the player has
                int score = player.Hand.Sum(card => card.Score);
                scores[player] = score;
            }

            return scores;
        }
    }
}
