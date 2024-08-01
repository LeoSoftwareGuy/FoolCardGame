using System.Net.Http.Headers;

namespace Fool.Core.Models
{
    public class Game
    {
        private Player? _firstToPlayPlayer;
        public Game(List<string> playerNames)
        {
            Deck = new Deck();
            Players = CreatePlayersAndTheirHands(playerNames);
        }
        public Deck Deck { get; private set; }
        public List<Player> Players { get; }
        public Player? FirstToPlayPlayer
        {
            get
            {
                if (_firstToPlayPlayer == null)
                {
                    return null;
                }
                else
                {
                    return _firstToPlayPlayer;
                }
            }
        }


        public void PrepareForTheGame()
        {
            _firstToPlayPlayer = DecideWhoPlaysFirst();
        }
        private List<Player> CreatePlayersAndTheirHands(List<string> playerNames)
        {
            Deck.Shuffle();
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
    }
}
