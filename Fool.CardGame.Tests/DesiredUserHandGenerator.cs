using Fool.Core.Exceptions;
using Fool.Core.Models.Cards;

namespace Fool.CardGame.Tests
{
    public class DesiredUserHandGenerator : CardDeckGenerator
    {
        private string[] _cardValues;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardValues"
        /// Cards which should be dealt to player
        /// Cards should be in format "RankSuit" where Rank is number or letter and Suit is one of the following: ♠, ♦, ♣, ♥
        /// Array of cards length == amount of players
        /// Example of string[] cardValues = new string[] { "6♠", "7♠", "8♠", "9♠", "10♠", "J♠" };
        /// ></param>
        public DesiredUserHandGenerator(string[] cardValues)
        {
            _cardValues = cardValues;
        }

        public override List<Card> GenerateDeck()
        {
            var deckCards = CardsHolder.GetCards();
            var playerHand = new List<Card>();

            foreach (var cardValue in _cardValues)
            {
                var cards = cardValue.Split(',');
                foreach (var card in cards)
                {
                    var cardRank = card.Substring(0, card.Length - 1);
                    var cardSuit = card[^1];

                    string rankValue = cardRank switch
                    {
                        "J" => "Jack",
                        "Q" => "Queen",
                        "K" => "King",
                        "A" => "Ace",
                        _ => cardRank
                    };

                    var cardFromDeck = deckCards.FirstOrDefault(c => c.Rank.Name == rankValue && c.Suit.IconChar == cardSuit);
                    if (cardFromDeck == null)
                    {
                        throw new FoolExceptions(card + " not found");
                    }
                    playerHand.Add(cardFromDeck);
                    deckCards.Remove(cardFromDeck);
                } 
            }

            deckCards.AddRange(playerHand);
            return deckCards;
        }

    }
}
