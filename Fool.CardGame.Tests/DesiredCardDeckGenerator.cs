using Fool.Core.Models.Cards;

namespace Fool.CardGame.Tests
{
    public class TestDeckGenerator : CardDeckGenerator
    {
        // Not Sorted Deck
        public override List<Card> GenerateDeck()
        {
            return new List<Card>
            {
                new Card(new CardRank(6, "6"), new CardSuit(3, "Spades", '♠')),
                new Card(new CardRank(6, "6"), new CardSuit(0, "Clubs", '♣')),

                          new Card(new CardRank(7, "7"), new CardSuit(2, "Hearts", '♥')),
                new Card(new CardRank(8, "8"), new CardSuit(0, "Clubs", '♣')),
                new Card(new CardRank(9, "9"), new CardSuit(0, "Clubs", '♣')),
                new Card(new CardRank(10, "10"), new CardSuit(0, "Clubs", '♣')),
                new Card(new CardRank(11, "Jack"), new CardSuit(0, "Clubs", '♣')),
                new Card(new CardRank(12, "Queen"), new CardSuit(0, "Clubs", '♣')),
                new Card(new CardRank(13, "King"), new CardSuit(0, "Clubs", '♣')),
                new Card(new CardRank(14, "Ace"), new CardSuit(0, "Clubs", '♣')),

                new Card(new CardRank(6, "6"), new CardSuit(1, "Diamonds", '♦')),
                new Card(new CardRank(7, "7"), new CardSuit(1, "Diamonds", '♦')),
              
                new Card(new CardRank(9, "9"), new CardSuit(1, "Diamonds", '♦')),
                new Card(new CardRank(10, "10"), new CardSuit(1, "Diamonds", '♦')),
                new Card(new CardRank(11, "Jack"), new CardSuit(1, "Diamonds", '♦')),
                new Card(new CardRank(12, "Queen"), new CardSuit(1, "Diamonds", '♦')),
                new Card(new CardRank(13, "King"), new CardSuit(1, "Diamonds", '♦')),
                new Card(new CardRank(14, "Ace"), new CardSuit(1, "Diamonds", '♦')),
                       new Card(new CardRank(13, "King"), new CardSuit(3, "Spades", '♠')),
                new Card(new CardRank(6, "6"), new CardSuit(2, "Hearts", '♥')),
      
                         new Card(new CardRank(9, "9"), new CardSuit(3, "Spades", '♠')),
                new Card(new CardRank(8, "8"), new CardSuit(2, "Hearts", '♥')),
                new Card(new CardRank(9, "9"), new CardSuit(2, "Hearts", '♥')),
                new Card(new CardRank(10, "10"), new CardSuit(2, "Hearts", '♥')),
                new Card(new CardRank(11, "Jack"), new CardSuit(2, "Hearts", '♥')),
                new Card(new CardRank(12, "Queen"), new CardSuit(2, "Hearts", '♥')),
                     new Card(new CardRank(10, "10"), new CardSuit(3, "Spades", '♠')),

                new Card(new CardRank(14, "Ace"), new CardSuit(2, "Hearts", '♥')),


                new Card(new CardRank(7, "7"), new CardSuit(3, "Spades", '♠')),
                new Card(new CardRank(8, "8"), new CardSuit(3, "Spades", '♠')),
                 new Card(new CardRank(7, "7"), new CardSuit(0, "Clubs", '♣')),
             new Card(new CardRank(13, "King"), new CardSuit(2, "Hearts", '♥')),
                new Card(new CardRank(11, "Jack"), new CardSuit(3, "Spades", '♠')),
                new Card(new CardRank(12, "Queen"), new CardSuit(3, "Spades", '♠')),
         
                  new Card(new CardRank(8, "8"), new CardSuit(1, "Diamonds", '♦')),
                new Card(new CardRank(14, "Ace"), new CardSuit(3, "Spades", '♠')),
            };
        }
    }
}
