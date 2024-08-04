namespace Fool.Core.Models.Cards
{
    public class Card
    {
        public Card(CardRank rank, CardSuit suit)
        {
            Rank = rank;
            Suit = suit;
        }


        public CardRank Rank { get; }
        public CardSuit Suit { get; }


        public override string ToString()
        {
            return Rank?.ShortName + Suit?.IconChar;
        }
    }
}
