using Fool.Core.Exceptions;

namespace Fool.Core.Models
{
    public class Deck
    {
        private const int _intitialHandSize = 6;

        public Deck()
        {
            Cards = new List<Card>();
        }

        public List<Card> Cards { get; set; }
        public int CardsCount => Cards.Count;
        public Card TrumpCard { get; set; }

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

        private List<Card> FillInTheDeck()
        {
            return CardsHolder.GetCards()
                .Select(x => new { Order = Globals.Random.Next(), Card = x })
                .OrderBy(x => x.Order)
                .Select(x => x.Card).ToList();
        }
    }
}
