using Fool.Core.Exceptions;
using Fool.Core.Models;
using Fool.Core.Models.Cards;
using Fool.Core.Models.Table;

namespace Fool.CardGame.Tests
{
    public class GameTests
    {
        public GameTests()
        {

        }

        [Test]
        public void Game_ActivePlayerHasTheLowestTrumpCard()
        {
            var game = new Game(new List<string> { "Leo", "Martha", "Zera" });
            game.PrepareForTheGame();

            Assert.IsNotNull(game.AtatackingPlayer);

            var activePlayersLowestTrumpCard = GetPlayersLowestTrumpCard(game.AtatackingPlayer, game.Deck.TrumpCard);

            Assert.IsNotNull(activePlayersLowestTrumpCard);

            foreach (var player in game.Players.Where(p => p.Name != game.AtatackingPlayer.Name))
            {
                var playersLowestTrumpCard = GetPlayersLowestTrumpCard(player, game.Deck.TrumpCard);

                if (playersLowestTrumpCard != null)
                {
                    Assert.True(activePlayersLowestTrumpCard.Rank.Value < playersLowestTrumpCard.Rank.Value);
                }
            }
        }


        [Test]
        public void Game_DefendingPlayerGoesAfterAttackingPlayer()
        {
            var game = new Game(new List<string> { "Leo", "Martha", "Zera" });
            game.PrepareForTheGame();
            Assert.IsNotNull(game.AtatackingPlayer);
            Assert.IsNotNull(game.DefendingPlayer);


            Assert.That(game.Players.IndexOf(game.DefendingPlayer) - 1 == game.Players.IndexOf(game.AtatackingPlayer));
        }

        //[Test]
        public void Game_DefendingPlayerAssignmentIsCircular()
        {
            var game = new Game(new List<string> { "Leo", "Martha", "Zera" });

            game.PrepareForTheGame();
            while (game?.AtatackingPlayer?.Name != "Zera")
            {
                game?.Deck.Shuffle();
                game?.PrepareForTheGame();
            }

            Assert.That(game?.DefendingPlayer?.Name == ("Leo"));
        }


        [Test]
        public void Game_DefendFromAttackingCard()
        {
            var nineOfClubs = CardsHolder.GetCards()[3];
            var tenOfClubs = CardsHolder.GetCards()[4];
            var trumpCard = CardsHolder.GetCards()[8];

            var tableCard = new TableCard(trumpCard, nineOfClubs);
            tableCard.Defend(tenOfClubs);
        }


        [Test]
        public void Game_DefendWithLowerRank_ExceptionThrown()
        {
            var nineOfClubs = CardsHolder.GetCards()[4];
            var tenOfClubs = CardsHolder.GetCards()[3];
            var trumpCard = CardsHolder.GetCards()[8];

            var tableCard = new TableCard(trumpCard, nineOfClubs);


            var exeption = Assert.Throws<FoolExceptions>(() => tableCard.Defend(tenOfClubs));
            Assert.That(exeption?.Message, Is.EqualTo("Defending Cards Rank is smaller then Attacking, same Suits"));
        }

        [Test]
        public void Game_DefendWithWrongSuit_ExceptionThrown()
        {
            var nineOfClubs = CardsHolder.GetCards()[4];
            var differentSuitCard = CardsHolder.GetCards()[13];
            var trumpCard = CardsHolder.GetCards()[8];

            var tableCard = new TableCard(trumpCard, nineOfClubs);

            var exeption = Assert.Throws<FoolExceptions>(() => tableCard.Defend(differentSuitCard));
            Assert.That(exeption?.Message, Is.EqualTo("Attacking Card is Trump, but Defending is not"));
        }


        [Test]
        [TestCase(0, 1, 8, true, TestName = "Successful defence without Exceptions")]
        [TestCase(2, 1, 8, false, TestName = "Defencing card is trump card but with lower value")]
        [TestCase(0, 1, 9, true, TestName = "Successful defence without Exceptions")]
        [TestCase(2, 1, 9, false, TestName = "Defencing card is trump card but with lower value")]
        public void Game_SuccessfulDefence(int attackingCardIndex, int defencingCardIndex, int trumpCardIndex, bool isSucess)
        {
            var attackingCard = CardsHolder.GetCards()[attackingCardIndex];
            var defendingCard = CardsHolder.GetCards()[defencingCardIndex];
            var trumpCard = CardsHolder.GetCards()[trumpCardIndex];

            var tableCard = new TableCard(trumpCard, attackingCard);
            if (isSucess)
            {
                tableCard.Defend(defendingCard);
            }
            else
            {
                Assert.Throws<FoolExceptions>(() => tableCard.Defend(defendingCard));
            }
        }

        [Test]
        public void Game_PlayOneRound_SuccessfulDefence()
        {
            var game = new Game(new List<string> { "Leo", "Elmaz" });
            game.Deck = new Deck(new TestDeckGenerator());
            game.Deck.Shuffle();
            game.PrepareForTheGame();
            game.AtatackingPlayer?.Attack(0);
            game.DefendingPlayer?.Defend(0, 0);
            game.FinishTheRound();


            Assert.That(game.AtatackingPlayer!.Hand.Count, Is.EqualTo(6));
            Assert.That(game.DefendingPlayer!.Hand.Count, Is.EqualTo(6));
            Assert.That(game.Deck.CardsCount, Is.EqualTo(22));
        }

        private Card GetPlayersLowestTrumpCard(Player player, Card trumpCard)
        {
            if (player == null || trumpCard == null)
            {
                throw new FoolExceptions("Either player or trumpCard is null");
            }

            return player.Hand
                .Where(c => c.Suit.Name.Equals(trumpCard.Suit.Name))
                .OrderBy(c => c.Rank.Value)
                .FirstOrDefault()!;
        }
    }
}