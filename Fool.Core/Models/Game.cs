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

            var player = new Player(playerName, this);
            Players.Add(player);

            if (Players.Count >= 2)
            {
                GameStatus = GameStatus.ReadyToBegin;
            }
            return player;
        }


        public void PrepareForTheGame()
        {
            if (GameStatus != GameStatus.ReadyToBegin)
            {
                throw new FoolExceptions($"Status of the game is: {GameStatus.ToString()}, Must be ReadyToBegin");
            }
            else
            {
                Deck.Shuffle();
                DealHands();
                _attackingPlayer = DecideWhoPlaysFirst();
            }

            GameStatus = GameStatus.InProgress;
        }


        public void DealHands()
        {
            foreach (var player in Players)
            {
                player.TakeCards(Deck.DealHand());
            }
        }

        public void FinishTheRound()
        {
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
        }

        internal void FirstAttack(Player player, List<Card> cards)
        {
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
                }
            }
            RoundStarted = true;
        }

        internal void Attack(Player player, List<Card> cards)
        {
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
            }
        }

        internal void Defend(Player player, Card defendingCard, Card attackingCard)
        {
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




        private Player DecideWhoPlaysFirst()
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
            // The logic is that when defending player fails to defend, he cant become attacking player next round
            var attackingPlayerIndex = Players.IndexOf(_attackingPlayer);
            attackingPlayerIndex++;
            var defendingPlayerIndex = Players.IndexOf(DefendingPlayer);
            if (defendingPlayerIndex.Equals(attackingPlayerIndex))
                attackingPlayerIndex++;

            if (attackingPlayerIndex >= Players.Count)
            {
                attackingPlayerIndex = 0;
                if (defendingPlayerIndex.Equals(_attackingPlayer))
                    attackingPlayerIndex++;
            }
            _attackingPlayer = Players[attackingPlayerIndex];
        }
    }
}
