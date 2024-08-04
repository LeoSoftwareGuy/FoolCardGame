using Fool.Core.Exceptions;
using Fool.Core.Models.Cards;
using Fool.Core.Models.Table;

namespace Fool.Core.Models
{
    public class Game
    {
        private Player? _attackingPlayer;
        private List<TableCard> _cardsOnTheTable;
        public Game(List<string> playerNames)
        {
            Deck = new Deck();
            Deck.Shuffle();
            Players = CreatePlayersAndTheirHands(playerNames);
        }
        public Deck Deck { get;  private set; }
        public List<Player> Players { get; }

        public IEnumerable<TableCard> CardsOnTheTable => _cardsOnTheTable;


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
            _attackingPlayer = DecideWhoPlaysFirst();
        }
        private List<Player> CreatePlayersAndTheirHands(List<string> playerNames)
        {
            var players = new List<Player>();
            foreach (var playerName in playerNames)
            {
                var player = new Player(playerName);
                player.TakeCards(Deck.DealHand());
                players.Add(player);
            }
            return players;
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

                var lowestValueAmongPlayers = playerAndItsLowestTrumpCard.Min(x => x.Value.Rank.Value);
                var playerWhoPlaysFirst = playerAndItsLowestTrumpCard.FirstOrDefault(x => x.Value.Rank.Value == lowestValueAmongPlayers).Key;

                if (playerWhoPlaysFirst != null)
                {
                    return playerWhoPlaysFirst;
                }
                else
                {
                    Deck = new Deck();
                    Deck.Shuffle();
                    foreach (var player in Players)
                    {
                        player.DropHand();
                        player.TakeCards(Deck.DealHand());
                    }
                }
            }
        }


        public void Attack(Player player, Card card)
        {
            if (AtatackingPlayer != player)
            {
                throw new FoolExceptions($"Player:{player.Name} cant attack as its player:{AtatackingPlayer?.Name} turn");
            }
            var tableCard = new TableCard(Deck.TrumpCard, card);
            _cardsOnTheTable.Add(tableCard);
        }

        public void Defend(Player player, Card defendingCard, Card attackingCard)
        {
            if (DefendingPlayer != player)
            {
                throw new FoolExceptions($"Player:{player.Name} cant defend, as defending player is {DefendingPlayer?.Name}");
            }

            var card = _cardsOnTheTable.FirstOrDefault(x => x.AttackingCard == attackingCard);
            if (card == null)
            {
                throw new FoolExceptions("Attacking card was not found on the table");
            }

            card.Defend(defendingCard);
        }
    }
}
