using Fool.Core.Exceptions;
using Fool.Core.Models.Cards;
using Fool.Core.Models.Table;
using System.ComponentModel.DataAnnotations;

namespace Fool.Core.Models
{
    public class Game
    {
        private Player? _attackingPlayer;
        public Game(List<string> playerNames)
        {
            Deck = new Deck(new CardDeckGenerator());
            Players = new List<Player>();
            CardsOnTheTable = new List<TableCard>();

            Deck.Shuffle();
            LetPlayersToTheTable(playerNames);
        }
        public Deck Deck { get; set; }
        public List<Player> Players { get; }

        public List<TableCard> CardsOnTheTable { get; private set; }


        public Player? AtatackingPlayer
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


        public void PrepareForTheGame()
        {
            DealHands();
            _attackingPlayer = DecideWhoPlaysFirst();
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
            if (CardsOnTheTable.All(c => c.DefendingCard != null))
            {
                // Defending player defended successfully
                CardsOnTheTable = new List<TableCard>();
            }
            else
            {
                DefendingPlayer?.TakeCards(CardsOnTheTable.Select(c => c.AttackingCard).ToList());
                DefendingPlayer?.TakeCards(CardsOnTheTable.Where(c => c.DefendingCard != null)
                                         .Select(c => c.DefendingCard)
                                         .ToList()!);
                CardsOnTheTable.Clear();
            }

            DrawMissingCardsAfterRound();

            // need to change attacking player, defending player becomes attackign player
        }

        internal void FirstAttack(Player player, Card card)
        {
            if (_attackingPlayer != player)
            {
                throw new FoolExceptions($"Player:{player.Name} cant attack as its player:{_attackingPlayer?.Name} turn");
            }
            var tableCard = new TableCard(Deck.TrumpCard, card);
            CardsOnTheTable.Add(tableCard);
        }

        internal void Attack(Player player, Card card)
        {
            if (DefendingPlayer == player)
            {
                throw new FoolExceptions($"Player:{player.Name} cant attack as he is defending");
            }

            var cardsThatCanBeAddedDueToOtherAttackingCardsOnTheTable = CardsOnTheTable.Select(c => c.AttackingCard.Rank.Value).ToList();
            var cardsThatCanBeAddedDueToOtherDefendingCardsOnTheTable = CardsOnTheTable.Select(c => c.DefendingCard?.Rank.Value).ToList();

            if (!cardsThatCanBeAddedDueToOtherAttackingCardsOnTheTable.Contains(card.Rank.Value) &&
                !cardsThatCanBeAddedDueToOtherDefendingCardsOnTheTable.Contains(card.Rank.Value))
            {
                throw new FoolExceptions($"Cards which are added to the attack should have the same rank as those on the table");
            }
            var tableCard = new TableCard(Deck.TrumpCard, card);
            CardsOnTheTable.Add(tableCard);
        }

        internal void Defend(Player player, Card defendingCard, Card attackingCard)
        {
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

        private void LetPlayersToTheTable(List<string> playerNames)
        {
            foreach (var name in playerNames)
            {
                Players.Add(new Player(name, this));
            }
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
    }
}
