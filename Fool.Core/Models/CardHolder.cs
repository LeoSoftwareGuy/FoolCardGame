namespace Fool.Core.Models
{
    public static class CardsHolder
    {
        public static List<Card> GetCards()
        {
            var ranks = new List<CardRank>()
            {
                new CardRank(6, "6"),
                new CardRank(7, "7"),
                new CardRank(8, "8"),
                new CardRank(9, "9"),
                new CardRank(10, "10"),
                new CardRank(11, "Jack"),
                new CardRank(12, "Queen"),
                new CardRank(13, "King"),
                new CardRank(14, "Ace"),
            };
            var suits = new List<CardSuit>()
            {
                new CardSuit(0, "Clubs", '♣'),
                new CardSuit(1, "Diamonds", '♦'),
                new CardSuit(2, "Hearts", '♥'),
                new CardSuit(3, "Spades", '♠'),
            };

            var cards = new List<Card>();
            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    var card = new Card(rank, suit);
                    cards.Add(card);
                }
            }
            return cards;
        }
    }
}
