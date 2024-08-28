using Fool.Core.Exceptions;
using Fool.Core.Models;
using Fool.Core.Models.Cards;
using Fool.Core.Models.Table;
using NuGet.Frameworks;
using NUnit.Framework.Constraints;
using System.Numerics;

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
            var game = new Game();

            game.AddPlayer("Leo");
            game.AddPlayer("Martha");
            game.AddPlayer("Zera");

            game.PrepareForTheGame();

            Assert.IsNotNull(game.AttackingPlayer);

            var activePlayersLowestTrumpCard = GetPlayersLowestTrumpCard(game.AttackingPlayer, game.Deck.TrumpCard);

            Assert.IsNotNull(activePlayersLowestTrumpCard);

            foreach (var player in game.Players.Where(p => p.Name != game.AttackingPlayer.Name))
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
            var game = new Game();

            game.AddPlayer("Leo");
            game.AddPlayer("Martha");
            game.AddPlayer("Zera");

            game.PrepareForTheGame();
            Assert.IsNotNull(game.AttackingPlayer);
            Assert.IsNotNull(game.DefendingPlayer);


            Assert.That(game.Players.IndexOf(game.DefendingPlayer) - 1 == game.Players.IndexOf(game.AttackingPlayer));
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
            var game = new Game();

            game.AddPlayer("Leo");
            game.AddPlayer("Elmaz");

            game.Deck = new Deck(new TestDeckGenerator());
            game.Deck.Shuffle();
            game.PrepareForTheGame();
            game.AttackingPlayer?.FirstAttack([0]);
            game.DefendingPlayer?.Defend(0, 0);
            game.FinishTheRound();


            Assert.That(game.AttackingPlayer!.Hand.Count, Is.EqualTo(6));
            Assert.That(game.DefendingPlayer!.Hand.Count, Is.EqualTo(6));
            Assert.That(game.Deck.CardsCount, Is.EqualTo(22));
        }

        [Test]
        public void Game_PlayOneRound_UnsuccessfulDefence_DefencingPlayerTakesAllCards()
        {
            var game = new Game();

            game.AddPlayer("Leo");
            game.AddPlayer("Elmaz");
            game.AddPlayer("Mio");

            game.Deck = new Deck(new TestDeckGenerator());
            game.Deck.Shuffle();
            game.PrepareForTheGame();

            var attackingPlayer = game.AttackingPlayer;
            var secondAttackingPlayer = game.Players[0];
            var defendingPlayer = game.DefendingPlayer;

            attackingPlayer?.FirstAttack([0]);
            defendingPlayer.Defend(3, 0);
            secondAttackingPlayer.Attack([1]);

            game.FinishTheRound();

            Assert.That(game.AttackingPlayer!.Hand.Count, Is.EqualTo(6));
            Assert.That(defendingPlayer!.Hand.Count, Is.EqualTo(8));
            Assert.That(game.Deck.CardsCount, Is.EqualTo(36 - 6 - 6 - 6 - 2));
        }



        [Test]
        public void Game_PlayTwoRounds_FirstRound2AttackingCards_SecondRound2AttackingCards_SuccessfulDefence()
        {
            var game = new Game();
            game.AddPlayer("Leo");
            game.AddPlayer("Elmaz");

            game.Deck = new Deck(new DesiredUserHandGenerator(new string[]
            {
               "J♠,10♣,8♥,8♠,7♠,6♠",  // Player 1's cards (In the hand they will have opposite indexes)
               "Q♠,Q♥,9♠,J♥,7♥,6♥"    // Player 2's cards
            }, "A♣"));

            game.Deck.Shuffle();
            game.PrepareForTheGame();

            game.AttackingPlayer.FirstAttack([2, 3]);

            var startingDefensiveCardIndex = 3;
            var startingAttackingCardIndex = 0;
            foreach (var attackingCard in game.CardsOnTheTable.Where(c => c.DefendingCard == null).ToList())
            {
                game.DefendingPlayer.Defend(startingDefensiveCardIndex, startingAttackingCardIndex);
                startingDefensiveCardIndex--;
                startingAttackingCardIndex++;
            }

            game.AttackingPlayer.Attack([3]);
            var attackingCardIndex = game.CardsOnTheTable.FirstOrDefault(c => c.DefendingCard == null).Id;
            game.DefendingPlayer.Defend(3, 2);

            game.FinishTheRound();

            Assert.That(game.AttackingPlayer!.Hand.Count, Is.EqualTo(6));
            Assert.That(game.DefendingPlayer!.Hand.Count, Is.EqualTo(6));
            Assert.That(game.Deck.CardsCount, Is.EqualTo(36 - 6 - 6 - 3 - 3));
        }


        [Test]
        public void Game_PlayOneRound_2PlayersAreAttackingDefendingPlayerTakesAllCards()
        {
            var game = new Game();
            game.AddPlayer("Leo");
            game.AddPlayer("Elmaz");
            game.AddPlayer("Viktor");

            game.Deck = new Deck(new DesiredUserHandGenerator(new string[]
            {
                "A♠,Q♦,Q♣,Q♥,10♥,7♦",
                "Q♠,A♦,A♣,A♥,10♣,10♠",
                "J♠,J♦,J♣,J♥,9♣,9♠"

            }, "6♦"));


            game.Deck.Shuffle();
            game.PrepareForTheGame();

            var firstAttackingPlayer = game.AttackingPlayer;
            var defendingPlayer = game.DefendingPlayer;
            var secondAttackingPlayer = game.Players[1];

            firstAttackingPlayer.FirstAttack([5]);
            secondAttackingPlayer.Attack([2, 3, 4]);

            game.FinishTheRound();

            Assert.That(firstAttackingPlayer.Hand.Count, Is.EqualTo(6));
            Assert.That(secondAttackingPlayer.Hand.Count, Is.EqualTo(6));
            Assert.That(defendingPlayer.Hand.Count, Is.EqualTo(6 + 4));
            Assert.That(game.Deck.CardsCount, Is.EqualTo(36 - 6 * 3 - 4));
        }


        [Test]
        public void Game_CorrectChangeOfAttackingPlayersWhenDefenceWasSuccessful()
        {
            var game = new Game();
            game.AddPlayer("1");
            game.AddPlayer("2");

            game.Deck = new Deck(new DesiredUserHandGenerator(new string[]
            {
                "A♠,Q♦,Q♣,Q♥,10♥,7♦",
                "Q♠,A♦,A♣,A♥,10♣,10♠"
             }, "6♦"));

            game.PrepareForTheGame();
            var attackingPlayerName = game.AttackingPlayer.Name;
            game.AttackingPlayer.FirstAttack([1]);
            game.DefendingPlayer.Defend(2, 0);
            game.FinishTheRound();

            Assert.That(game.DefendingPlayer.Name, Is.EqualTo(attackingPlayerName));
        }

        [Test]
        public void Game_CorrectChangeOfAttackingPlayersWhenDefenceWasNotSuccessful()
        {
            var game = new Game();
            game.AddPlayer("1");
            game.AddPlayer("2");

            game.Deck = new Deck(new DesiredUserHandGenerator(new string[]
              {
                "A♠,Q♦,Q♣,Q♥,10♥,7♦",
                "Q♠,A♦,A♣,A♥,10♣,10♠"
              }, "6♦"));

            game.PrepareForTheGame();
            var attackingPlayerName = game.AttackingPlayer.Name;
            game.AttackingPlayer.FirstAttack([1]);
            game.FinishTheRound();

            Assert.That(game.AttackingPlayer.Name, Is.EqualTo(attackingPlayerName));
        }

        [Test]
        public void Game_RoundIsNotFinishedBecauseOnlyOnePlayerWantsTo()
        {
            var game = new Game();
            game.AddPlayer("1");
            game.AddPlayer("2");
            game.AddPlayer("3");

            game.Deck = new Deck(new DesiredUserHandGenerator(new string[]
               {
                "A♠,Q♦,Q♣,Q♥,10♥,7♦",
                "Q♠,A♦,A♣,A♥,10♣,10♠",
                "J♠,J♦,J♣,J♥,9♣,9♠"

               }, "6♦"));

            game.PrepareForTheGame();

            var deckCount = game.Deck.CardsCount;
            var firstAttackingPlayer = game.AttackingPlayer;
            var defendingPlayer = game.DefendingPlayer;
            var secondAttackingPlayer = game.Players[1];

            firstAttackingPlayer.FirstAttack([1]);
            defendingPlayer.Defend(2, 0);

            firstAttackingPlayer.WantsToFinishTheRound();
            if (IfAllAttackingPlayersWantToFinishTheRound(game))
            {
                game.FinishTheRound();
                game.RefreshTheRound();
            }

            var anotherDeckCount = game.Deck.CardsCount;
            Assert.That(deckCount, Is.EqualTo(anotherDeckCount));
        }



        [Test]
        public void Game_RoundIsFinished_AllAttackingPlayersWantToFinish()
        {
            var game = new Game();
            game.AddPlayer("1");
            game.AddPlayer("2");
            game.AddPlayer("3");

            game.Deck = new Deck(new DesiredUserHandGenerator(new string[]
               {
                "A♠,Q♦,Q♣,Q♥,10♥,7♦",
                "Q♠,A♦,A♣,A♥,10♣,10♠",
                "J♠,J♦,J♣,J♥,9♣,9♠"

               }, "6♦"));

            game.PrepareForTheGame();

            var deckCount = game.Deck.CardsCount;
            var firstAttackingPlayer = game.AttackingPlayer;
            var defendingPlayer = game.DefendingPlayer;
            var secondAttackingPlayer = game.Players[1];

            firstAttackingPlayer.FirstAttack([1]);
            defendingPlayer.Defend(2, 0);

            firstAttackingPlayer.WantsToFinishTheRound();
            secondAttackingPlayer.WantsToFinishTheRound();
            if (IfAllAttackingPlayersWantToFinishTheRound(game))
            {
                game.FinishTheRound();
                game.RefreshTheRound();
            }

            var anotherDeckCount = game.Deck.CardsCount;
            Assert.That(deckCount, Is.Not.EqualTo(anotherDeckCount));
        }


        [Test]
        public void Game_RoundIsNotFinished_OnePlayerAttacksAfterOtherPlayersWantToFinish()
        {
            var game = new Game();
            game.AddPlayer("1");
            game.AddPlayer("2");
            game.AddPlayer("3");
            game.AddPlayer("4");

            game.Deck = new Deck(new DesiredUserHandGenerator(new string[]
               {
                "A♠,Q♦,Q♣,Q♥,10♥,7♦", // Attacking game.Players[3]
                "Q♠,A♦,A♣,A♥,10♣,10♠", 
                "J♠,J♦,J♣,J♥,9♣,9♠",
                "6♠,8♦,8♣,6♥,10♦,9♦" //Defending  game.Players[1]

               }, "6♦"));

            game.PrepareForTheGame();

            var deckCount = game.Deck.CardsCount;
            var firstAttackingPlayer = game.AttackingPlayer;
            var defendingPlayer = game.DefendingPlayer;
            var secondAttackingPlayer = game.Players[2];
            var thirdAttackingPlayer = game.Players[1];

            firstAttackingPlayer.FirstAttack([1]);
            defendingPlayer.Defend(0, 0);

            firstAttackingPlayer.WantsToFinishTheRound();
            secondAttackingPlayer.WantsToFinishTheRound();

            thirdAttackingPlayer.Attack([0]);
            if (IfAllAttackingPlayersWantToFinishTheRound(game))
            {
                game.FinishTheRound();
                game.RefreshTheRound();
            }

            var anotherDeckCount = game.Deck.CardsCount;
            Assert.That(deckCount, Is.EqualTo(anotherDeckCount));
        }



        [Test]
        public void Game_RoundIsFinished_OnePlayerAttacksAfterOtherPlayersWantToFinish_CardsIsDefended_ThenAllWantToFinish()
        {
            var game = new Game();
            game.AddPlayer("1");
            game.AddPlayer("2");
            game.AddPlayer("3");
            game.AddPlayer("4");

            game.Deck = new Deck(new DesiredUserHandGenerator(new string[]
               {
                "A♠,Q♦,Q♣,Q♥,10♥,7♦", // Attacking game.Players[3]
                "Q♠,A♦,A♣,A♥,10♣,10♠",
                "J♠,J♦,J♣,J♥,9♣,9♠",  // 3rd attacking player
                "6♠,8♦,8♣,6♥,10♦,9♦" //Defending  game.Players[1]

               }, "6♦"));

            game.PrepareForTheGame();

            var deckCount = game.Deck.CardsCount;
            var firstAttackingPlayer = game.AttackingPlayer;
            var defendingPlayer = game.DefendingPlayer;
            var secondAttackingPlayer = game.Players[2];
            var thirdAttackingPlayer = game.Players[1];

            firstAttackingPlayer.FirstAttack([1]);
            defendingPlayer.Defend(0, 0);

            firstAttackingPlayer.WantsToFinishTheRound();
            secondAttackingPlayer.WantsToFinishTheRound();

            thirdAttackingPlayer.Attack([0]);
            defendingPlayer.Defend(0, 1);

            firstAttackingPlayer.WantsToFinishTheRound();
            secondAttackingPlayer.WantsToFinishTheRound();
            thirdAttackingPlayer.WantsToFinishTheRound();
            if (IfAllAttackingPlayersWantToFinishTheRound(game))
            {
                game.FinishTheRound();
                game.RefreshTheRound();
            }

            var anotherDeckCount = game.Deck.CardsCount;
            Assert.That(deckCount, Is.Not.EqualTo(anotherDeckCount));
        }



        private bool IfAllAttackingPlayersWantToFinishTheRound(Game game)
        {
            return game.Players.Where(player => player != game.DefendingPlayer).All(player => player.WantsToFinishRound);
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