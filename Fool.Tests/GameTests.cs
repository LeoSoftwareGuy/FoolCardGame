using Fool.Core.Exceptions;
using Fool.Core.Models;

namespace Fool.Tests
{
    public class GameTests
    {
        public GameTests()
        {

        }

        [Fact]
        public void Game_ActivePlayerHasTheLowestTrumpCard()
        {
            var game = new Game(new List<string> { "Leo", "Martha", "Zera" });
            game.PrepareForTheGame();

            Assert.NotNull(game.FirstToPlayPlayer);

            var activePlayersLowestTrumpCard = GetPlayersLowestTrumpCard(game.FirstToPlayPlayer, game.Deck.TrumpCard);

            Assert.NotNull(activePlayersLowestTrumpCard);

            foreach (var player in game.Players.Where(p => p.Name != game.FirstToPlayPlayer.Name))
            {
                var playersLowestTrumpCard = GetPlayersLowestTrumpCard(player, game.Deck.TrumpCard);

                if (playersLowestTrumpCard != null)
                {
                    Assert.True(activePlayersLowestTrumpCard.Rank.Value < playersLowestTrumpCard.Rank.Value);
                }
            }
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
