using System;
using System.Collections.Generic;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Game.Builders
{
    public static class TestGameBuilder
    {
        private static readonly Random _rng = new();
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

            gameState.PlayedCards = new List<(Player player, Card card)>();
            gameState.CurrentPlayerIndex = 0;
            gameState.Deck = new Deck();
            gameState.Deck.Add52CardsToDeck();
            gameState.CurrentPhase = GamePhase.NotStarted;

            return gameState;
        }

        private static List<Card> CreateHand(int count)
        {
            List<Card> hand = new List<Card>();

            var suits = (Card.Suits[])Enum.GetValues(typeof(Card.Suits));
            var ranks = (Card.Ranks[])Enum.GetValues(typeof(Card.Ranks));

            for (int i = 0; i < count; i++)
            {
                var suit = (Card.Suits)suits.GetValue(_rng.Next(suits.Length));
                var rank = (Card.Ranks)ranks.GetValue(_rng.Next(ranks.Length));

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
