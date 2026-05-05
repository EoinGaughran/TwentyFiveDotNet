using System.Collections.Generic;
using TwentyFiveDotNet.Core.Models;

namespace TwentyFiveDotNet.Core.Game.Builders
{
    public static class TestGameBuilder
    {
        public static GameState CreateBasicGame(RulesEngine rulesEngine)
        {
            GameState gameState = new();

            gameState.Deck.Add52CardsToDeck();
            gameState.SetGamePhase(GamePhase.NotStarted);

            gameState.AddPlayers(new List<Player>
            {
                new PlayerHuman("You"),
                new PlayerCPU("Opponent 1", rulesEngine),
                new PlayerCPU("Opponent 2", rulesEngine),
                new PlayerCPU("Opponent 3", rulesEngine),
            });

            

            for (int i = 0; i < gameState.Players.Count; i++)
            {
                gameState.Players[i].SetID(i);
                gameState.Players[i].AddCards(CreateHand(5, gameState.Deck));
                gameState.Players[i].AddCardsToPlayedCards(CreateHand(5, gameState.Deck));
            }

            return gameState;
        }

        private static List<Card> CreateHand(int count, Deck deck)
        {
            List<Card> hand = new();

            for (int i = 0; i < count; i++)
            {
                hand.Add(deck.Draw());
            }

            return hand;
        }
    }
}
