using Fool.Core.Models;

namespace Fool.Tests
{
    public class DeckTests
    {
        public DeckTests()
        {
        }

        [Fact]
        public void Deck_Initialization_ShouldHave36Cards()
        {
            const int initialAmountOfCards = 36;
            const int cardNominals = 9;
            const int cardSuits = 4;

            var deck = new Deck();
            deck.Shuffle();

            Assert.Equal(deck?.CardsCount, initialAmountOfCards);
            Assert.Equal(deck?.Cards.GroupBy(x => x.Suit).Count(), cardSuits);
            Assert.Equal(deck?.Cards.Select(x => x.Rank).Distinct().Count(), cardNominals);
        }

        [Fact]
        public void Deck_ShouldHaveTrumpCard()
        {
            var deck = new Deck();
            deck.Shuffle();

            Assert.NotNull(deck?.TrumpCard);
        }

        [Fact]
        public void Deck_ShouldPullCard()
        {
            var deck = new Deck();
            deck.Shuffle();

            var card = deck.PullCard();

            Assert.NotNull(card);
            Assert.Equal(deck?.CardsCount, 35);
        }

        [Fact]
        public void Deck_ShouldDealHand()
        {
            var deck = new Deck();
            deck.Shuffle();

            var hand = deck.DealHand();

            Assert.NotNull(hand);
            Assert.Equal(hand?.Count, 6);
            Assert.Equal(deck?.CardsCount, 30);
        }

        [Fact]
        public void Deck_GameWithThreePlayers()
        {
            var players = new List<string> { "Leo", "Vins", "Alan" };
            var game = new Game(players);
            Assert.Equal(game?.Deck.CardsCount, 18);
            Assert.Equal(game?.Players.Count, 3);
            Assert.Equal(game?.Players[0].Hand.Count, 6);         
        }
    }
}