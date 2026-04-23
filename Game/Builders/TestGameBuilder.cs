using System;
using System.Collections.Generic;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Game.Builders
{
    public static class TestGameBuilder
    {
        public static GameState CreateBasicGame(RulesEngine rulesEngine)
        {
            GameState gameState = new GameState();

            gameState.PlayedCards = new List<(Player player, Card card)>();
            gameState.CurrentPlayerIndex = 0;
            gameState.Deck = new Deck();
            gameState.Deck.Add52CardsToDeck();
            gameState.CurrentPhase = GamePhase.NotStarted;

            gameState.Players = new List<Player>
            {
                new PlayerHuman("You"),
                new PlayerCPU("Opponent 1", rulesEngine),
                new PlayerCPU("Opponent 2", rulesEngine),
                new PlayerCPU("Opponent 3", rulesEngine),
            };

            for (int i = 0; i < gameState.Players.Count; i++)
            {
                gameState.Players[i].Id = i;
                gameState.Players[i].Hand = CreateHand(5, gameState.Deck);
            }

            return gameState;
        }

        private static List<Card> CreateHand(int count, Deck deck)
        {
            List<Card> hand = new List<Card>();

            for (int i = 0; i < count; i++)
            {
                hand.Add(deck.Draw());
            }

            return hand;
        }
    }
}
