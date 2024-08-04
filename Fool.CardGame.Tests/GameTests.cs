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

        [Test]
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
          

            var exeption = Assert.Throws<FoolExceptions>(()=> tableCard.Defend(tenOfClubs));
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

        private Card GetPlayersLowestTrumpCard(Player player, Card trumpCard)
        {
            if (player == null || trumpCard == null)
            {
                throw new FoolExceptions("Either player or trumpCard is null");
            }

            return player.Hand
                .Where(c => c.Suit.Name.Equals(trumpCard.Suit.Name))
                .OrderBy(c => c.Rank.Value)
                .FirstOrDefault();
        }
    }
}