using Fool.Core.Models;

namespace Fool.CardGame.Tests
{
    public class DeckTests
    {
        public DeckTests()
        {
        }

        [Test]
        public void Deck_Initialization_ShouldHave36Cards()
        {
            const int initialAmountOfCards = 36;
            const int cardNominals = 9;
            const int cardSuits = 4;

            var deck = new Deck();
            deck.Shuffle();

            Assert.That(deck?.CardsCount, Is.EqualTo(initialAmountOfCards));
            Assert.That(deck?.Cards.GroupBy(x => x.Suit).Count(), Is.EqualTo(cardSuits));
            Assert.That(deck?.Cards.Select(x => x.Rank).Distinct().Count(), Is.EqualTo(cardNominals));
        }

        [Test]
        public void Deck_ShouldHaveTrumpCard()
        {
            var deck = new Deck();
            deck.Shuffle();

            Assert.IsNotNull(deck?.TrumpCard);
        }

        [Test]
        public void Deck_ShouldPullCard()
        {
            var deck = new Deck();
            deck.Shuffle();

            var card = deck.PullCard();

            Assert.NotNull(card);
            Assert.That(deck?.CardsCount, Is.EqualTo(35));
        }

        [Test]
        public void Deck_ShouldDealHand()
        {
            var deck = new Deck();
            deck.Shuffle();

            var hand = deck.DealHand();

            Assert.NotNull(hand);
            Assert.That(hand?.Count, Is.EqualTo(6));
            Assert.That(deck?.CardsCount, Is.EqualTo(30));
        }

        [Test]
        public void Deck_GameWithThreePlayers()
        {
            var players = new List<string> { "Leo", "Vins", "Alan" };
            var game = new Game(players);
            Assert.That(game?.Deck.CardsCount, Is.EqualTo(18));
            Assert.That(game?.Players.Count, Is.EqualTo(3));
            Assert.That(game?.Players[0].Hand.Count, Is.EqualTo(6));
        }
    }
}
