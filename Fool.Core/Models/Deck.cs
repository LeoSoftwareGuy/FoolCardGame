﻿using Fool.Core.Exceptions;
using Fool.Core.Models.Cards;

namespace Fool.Core.Models
{
    public class Deck
    {
        private const int _intitialHandSize = 6;
        private CardDeckGenerator _cardDeckGenerator;

        public Deck(CardDeckGenerator cardDeckGenerator)
        {
            Cards = new List<Card>();
            _cardDeckGenerator = cardDeckGenerator;
        }

        public List<Card> Cards { get; private set; }
        public Card TrumpCard { get;  private set; }
        public int CardsCount => Cards.Count;

        public void Shuffle()
        {
            Cards = FillInTheDeck();
            TrumpCard = Cards.First();
        }

        public Card PullCard()
        {
            var card = Cards.LastOrDefault();
            if (card == null)
            {
                throw new FoolExceptions("Your Deck is Empty");
            }
            Cards.Remove(card);
            return card;
        }

        public List<Card> DealHand()
        {
            var cardsToBeGive = new List<Card>();
            for (var card = 0; card < _intitialHandSize; card++)
            {
                if (Cards.Count == 0) break;

                cardsToBeGive.Add(PullCard());
            }

            return cardsToBeGive;
        }

        public bool HasCards()
        {
            return Cards.Any();
        }

        private List<Card> FillInTheDeck()
        {
            return _cardDeckGenerator.GenerateDeck();
        }
    }
}
