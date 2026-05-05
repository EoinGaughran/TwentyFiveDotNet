using System;
using System.Collections.Generic;

namespace TwentyFiveDotNet.Core.Models
{
    public abstract class Player
    { 
        public int Id { get; private set; }
        public string Name { get; protected set; } = string.Empty;
        public int Points { get; private set; }
        public IReadOnlyList<Card> Hand => _hand;
        protected readonly List<Card> _hand = new();
        public IReadOnlyList<Card> PlayedCards => _playedCards;
        protected readonly List<Card> _playedCards = new();

        protected Player() { }

        public Player(string name)
        {
            Name = name;
            _hand = new List<Card>();
            Points = 0;
        }

        public override string ToString()
        {
            return Name;
        }

        public void SetID(int id) => Id = id;

        public void AddPoints(int amount) => Points += amount;
        public void ResetPoints() => Points = 0;

        public void AddCard(Card card) => _hand.Add(card);

        public void AddCardToPlayedCards(Card card) => _playedCards.Add(card);

        public void AddCards(List<Card> cards)
        {
            foreach(Card card in cards)
                _hand.Add(card);
        }

        public void AddCardsToPlayedCards(List<Card> cards)
        {
            foreach (Card card in cards)
                _playedCards.Add(card);
        }

        public void RemoveCard(Card card)
        {
            if (_hand.Contains(card))
                _hand.Remove(card);
            else
                throw new InvalidOperationException($"{Name}'s hand does not contain the {card}");
        }

        public List<Card> GetCards() => _hand;
        public void ClearHand() => _hand.Clear();
    }
}
