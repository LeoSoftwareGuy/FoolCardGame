using Fool.Core.Exceptions;
using Fool.Core.Models.Cards;
using Fool.Core.Models.Table;

namespace Fool.Core.Models
{
    public class Game
    {
        private Player? _attackingPlayer;
        public Game()
        {
            Deck = new Deck(new CardDeckGenerator());
            Players = new List<Player>();
            CardsOnTheTable = new List<TableCard>();
            RoundStarted = false;
            GameStatus = GameStatus.WaitingForPlayers;
        }
        public Deck Deck { get; set; }
        public List<Player> Players { get; }

        public List<TableCard> CardsOnTheTable { get; private set; }
        public GameStatus GameStatus { get; private set; }
        public bool RoundStarted { get; private set; }

        public Player? FoolPlayer { get; private set; }
        public Player? AttackingPlayer
        {
            get
            {
                if (_attackingPlayer == null)
                {
                    return null;
                }
                else
                {
                    return _attackingPlayer;
                }
            }
        }

        public Player? DefendingPlayer
        {
            get
            {
                if (_attackingPlayer == null)
                {
                    return null;
                }
                else
                {
                    var index = Players.IndexOf(_attackingPlayer);
                    if (index + 1 >= Players.Count)
                    {
                        return Players[0];
                    }
                    else
                    {
                        return Players[index + 1];
                    }

                }
            }
        }


        public Player AddPlayer(string playerName)
        {
            if (Players.Count >= 6)
            {
                throw new FoolExceptions("Max number of player per table is 6");
            }

            if (GameStatus == GameStatus.InProgress)
            {
                throw new FoolExceptions("Game has already started, you cant join now");
            }

            if (GameStatus == GameStatus.Finished)
            {
                throw new FoolExceptions("Game has already finished, you cant join now");
            }

            var player = new Player(playerName, this);
            Players.Add(player);

            if (Players.Count >= 2)
            {
                GameStatus = GameStatus.ReadyToBegin;
            }
            return player;
        }

        public string RemovePlayer(Player player)
        {
            if (player == null)
            {
                throw new FoolExceptions("Player was not found!");
            }

            Players.Remove(player);

            if (GameStatus == GameStatus.InProgress)
            {
                GameStatus = GameStatus.Finished;
            }
            else
            {
                if (Players.Count <= 1)
                {
                    GameStatus = GameStatus.WaitingForPlayers;
                }
            }

            switch (GameStatus)
            {
                case GameStatus.WaitingForPlayers:
                    return string.Empty;
                case GameStatus.ReadyToBegin:
                    return string.Empty;
                case GameStatus.Finished:
                    return "Game has finished";
                default:
                    return string.Empty;
            }
        }


        public void PrepareForTheGame()
        {
            if (GameStatus != GameStatus.ReadyToBegin)
            {
                throw new FoolExceptions($"Status of the game is: {GameStatus.ToString()}, Must be ReadyToBegin");
            }
            else
            {
                var foolPlayerIndex = -1;
                if (FoolPlayer != null)
                {
                    foolPlayerIndex = Players.IndexOf(FoolPlayer);
                    FoolPlayer = null;
                }

                Deck.Shuffle();
                DealHands();
                _attackingPlayer = DecideWhoPlaysFirst(foolPlayerIndex);
            }

            GameStatus = GameStatus.InProgress;
        }




        public void FinishTheRound()
        {
            CheckIfGameIsOver();

            var wasDefendingPlayerSuccessful = true;
            if (!CardsOnTheTable.All(c => c.DefendingCard != null))
            {
                DefendingPlayer?.TakeCards(CardsOnTheTable.Select(c => c.AttackingCard).ToList());
                DefendingPlayer?.TakeCards(CardsOnTheTable.Where(c => c.DefendingCard != null)
                                         .Select(c => c.DefendingCard)
                                         .ToList()!);
                wasDefendingPlayerSuccessful = false;
            }

            CardsOnTheTable.Clear();
            DrawMissingCardsAfterRound();
            RoundStarted = false;
            AssignNewAttackingPlayer(wasDefendingPlayerSuccessful);
            // Check cards in players hand if there are no cards in the deck and no cards on playerhs hand, then he has won.
            // He is out of the game 
        }

        internal void FirstAttack(Player player, List<Card> cards)
        {
            CheckIfGameIsOver();

            if (RoundStarted)
            {
                throw new FoolExceptions("Round has already started, use Attack method instead!");
            }

            if (_attackingPlayer != player)
            {
                throw new FoolExceptions($"Player:{player.Name} cant attack as its player:{_attackingPlayer?.Name} turn");
            }

            if (cards.Count > 0)
            {
                foreach (var card in cards)
                {
                    var tableCard = new TableCard(Deck.TrumpCard, card);
                    CardsOnTheTable.Add(tableCard);
                    player.Hand.Remove(card);  // Remove cards since they are now on the table
                }
            }
            RoundStarted = true;

            //not sure if it is possible
            CheckIfAnybodyHasWon();
        }

        internal void Attack(Player player, List<Card> cards)
        {
            CheckIfGameIsOver();
            //TODO add lock to prevent parallel calls
            if (RoundStarted == false)
            {
                throw new FoolExceptions("Round has not started, use FirstAttack method instead!");
            }

            if (DefendingPlayer == player)
            {
                throw new FoolExceptions($"Player:{player.Name} cant attack as he is defending");
            }

            var cardsThatCanBeAddedDueToOtherAttackingCardsOnTheTable = CardsOnTheTable.Select(c => c.AttackingCard.Rank.Value).ToList();
            var cardsThatCanBeAddedDueToOtherDefendingCardsOnTheTable = CardsOnTheTable.Select(c => c.DefendingCard?.Rank.Value).ToList();

            if (!cards.All(c => cardsThatCanBeAddedDueToOtherAttackingCardsOnTheTable.Contains(c.Rank.Value)
                          || cardsThatCanBeAddedDueToOtherDefendingCardsOnTheTable.Contains(c.Rank.Value)))
            {
                throw new FoolExceptions($"Cards which are added to the attack should have the same rank as those on the table");
            }

            foreach (var card in cards)
            {
                var tableCard = new TableCard(Deck.TrumpCard, card);
                CardsOnTheTable.Add(tableCard);
                player.Hand.Remove(card);  // Remove cards since they are now on the table
            }

            CheckIfAnybodyHasWon();
        }

        internal void Defend(Player player, Card defendingCard, Card attackingCard)
        {
            CheckIfGameIsOver();

            if (RoundStarted == false)
            {
                throw new FoolExceptions("Round has not started, there is nothing to defend against!");
            }

            if (DefendingPlayer != player)
            {
                throw new FoolExceptions($"Player:{player.Name} cant defend, as defending player is {DefendingPlayer?.Name}");
            }

            var card = CardsOnTheTable.FirstOrDefault(x => x.AttackingCard == attackingCard);
            if (card == null)
            {
                throw new FoolExceptions("Attacking card was not found on the table");
            }

            card.Defend(defendingCard);
        }


        public void RefreshTheRound()
        {
            foreach (var player in Players)
            {
                player.RefreTheRound();
            }
        }


        private void DealHands()
        {
            foreach (var player in Players)
            {
                player.TakeCards(Deck.DealHand());
            }
        }

        private Player DecideWhoPlaysFirst(int foolPlayerIndex)
        {
            // Teach fool a lesson rule!
            if (foolPlayerIndex != -1)
            {
                var playerWhoAttacksFirstIndex = foolPlayerIndex - 1;
                if (playerWhoAttacksFirstIndex < 0)
                {
                    playerWhoAttacksFirstIndex = Players.Count - 1;
                }
                return Players[playerWhoAttacksFirstIndex];
            }
            else
            {
                while (true)
                {
                    var trumpSuit = Deck.TrumpCard.Suit.Name;
                    var playerAndItsLowestTrumpCard = new Dictionary<Player, Card>();
                    foreach (var player in Players)
                    {
                        var playersLowestTrumpCard = player.Hand.FindAll(x => x.Suit.Name == trumpSuit)
                                                          .OrderBy(x => x.Rank.Value)
                                                          .FirstOrDefault();
                        if (playersLowestTrumpCard != null)
                        {
                            playerAndItsLowestTrumpCard.Add(player, playersLowestTrumpCard);
                        }
                    }

                    if (playerAndItsLowestTrumpCard != null && playerAndItsLowestTrumpCard.Count > 0)
                    {
                        var lowestValueAmongPlayers = playerAndItsLowestTrumpCard.Min(x => x.Value.Rank.Value);
                        var playerWhoPlaysFirst = playerAndItsLowestTrumpCard.FirstOrDefault(x => x.Value.Rank.Value == lowestValueAmongPlayers).Key;
                        return playerWhoPlaysFirst;
                    }
                    else
                    {
                        Deck = new Deck(new CardDeckGenerator());
                        Deck.Shuffle();
                        foreach (var player in Players)
                        {
                            player.DropHand();
                            player.TakeCards(Deck.DealHand());
                        }
                    }
                }
            }
        }

        private void DrawMissingCardsAfterRound()
        {
            // make sure that each player has 6 cards in the end of the round
            // first player to take cards is attacking player, last one to take is defending player

            var orderedPlayers = Players.OrderBy(p => p != _attackingPlayer) // if expression gives false, it goes before true, so all attacking players are first
                                          .ThenBy(p => p == DefendingPlayer)
                                          .ThenBy(p => Players.IndexOf(p))      // Maintain order for other players
                                          .ToList();

            foreach (var player in orderedPlayers)
            {
                while (player.Hand.Count < 6 && Deck.HasCards())
                {
                    player.TakeCard(Deck.PullCard());
                }
            }
        }

        private void AssignNewAttackingPlayer(bool wasDefendingPlayerSuccessful)
        {
            var attackingPlayerIndex = Players.IndexOf(_attackingPlayer);
            var defendingPlayerIndex = Players.IndexOf(DefendingPlayer);


            if (wasDefendingPlayerSuccessful)
            {
                attackingPlayerIndex++;
            }
            else
            {
                attackingPlayerIndex += 2;
            }


            if (attackingPlayerIndex >= Players.Count)
            {
                attackingPlayerIndex = attackingPlayerIndex - Players.Count;
            }

            _attackingPlayer = Players[attackingPlayerIndex];
        }

        private void CheckIfAnybodyHasWon()
        {
            var playersWithoutCards = Players.Where(x => x.Hand.Count > 0);
            if (playersWithoutCards.Count() == 1 && Deck.CardsCount.Equals(0))
            {
                FoolPlayer = playersWithoutCards.First();
                GameStatus = GameStatus.Finished;
            }
        }

        private void CheckIfGameIsOver()
        {
            if (GameStatus == GameStatus.Finished)
            {
                throw new FoolExceptions("Game is over!");
            }
        }

    }
}
