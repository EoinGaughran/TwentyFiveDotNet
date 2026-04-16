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

            gameState.Players = new List<Player>
            {
                new PlayerHuman("You"),
                new PlayerCPU("Opponent 1", rulesEngine),
                new PlayerCPU("Opponent 2", rulesEngine),
                new PlayerCPU("Opponent 3", rulesEngine),
            };

            foreach (var player in gameState.Players)
            {
                player.Hand = CreateHand(5);
            }

            gameState.TrickPile = new List<Card>();
            gameState.CurrentPlayerIndex = 0;

            return gameState;
        }

        private static List<Card> CreateHand(int count)
        {
            List<Card> hand = new List<Card>();
            Random rng = new Random();
            var suits = (Card.Suits[])Enum.GetValues(typeof(Card.Suits));
            var ranks = (Card.Ranks[])Enum.GetValues(typeof(Card.Ranks));

            for (int i = 0; i < count; i++)
            {
                var suit = (Card.Suits)suits.GetValue(rng.Next(suits.Length));
                var rank = (Card.Ranks)ranks.GetValue(rng.Next(ranks.Length));

                hand.Add(new Card
                {
                    Suit = suit,
                    Rank = rank
                });
            }

            return hand;
        }
    }
}
