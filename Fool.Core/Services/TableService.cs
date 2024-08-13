using Fool.Core.Exceptions;
using Fool.Core.Models;
using Fool.Core.Models.Table;
using Fool.Core.Services.Interfaces;

namespace Fool.Core.Services
{
    public class TableService : ITableService
    {
        public Dictionary<Guid, Table> TablesWithGames { get; private set; }
        public TableService()
        {
            TablesWithGames = new Dictionary<Guid, Table>();
        }

        public Guid CreateTable()
        {
            var tableId = Guid.NewGuid();
            var game = new Game();
            var table = new Table { Game = game, PlayersAndTheirSecretKeys = new Dictionary<string, Player> { } };
            TablesWithGames.Add(tableId, table);
            return tableId;
        }

        public void SitToTheTable(string playerSecret, string playerName, Guid tableId)
        {
            if (CheckIfPlayerIsAlreadyPlayingOnAnotherTable(playerSecret))
            {
                throw new FoolExceptions("You are already plaing on another table");
            }

            if (TablesWithGames.TryGetValue(tableId, out var table))
            {
                var player = table.Game.AddPlayer(playerName);
                table.PlayersAndTheirSecretKeys.Add(playerSecret, player);

                var debug = true;
                if (debug)
                {
                    table.Game.AddPlayer("Elmaz");
                    table.Game.PrepareForTheGame();
                }
            }
            else
            {
                throw new FoolExceptions("Table was not found");
            }
        }



        // If Player has a table, return the table id, his hand, deck count and trump card
        // If Player does not have a table, return all table ids
        public dynamic GetStatus(string playerSecret)
        {
            var playerTable = TablesWithGames.Values.FirstOrDefault(t => t.PlayersAndTheirSecretKeys.ContainsKey(playerSecret));
            var player = playerTable == null ? null : playerTable.PlayersAndTheirSecretKeys[playerSecret];

            var result = new
            {
                Table = playerTable == null ? null : new
                {
                    TableId = playerTable.Id,
                    PlayerHand = player.Hand.Select(c => new { Rank = c.Rank.Value, Suit = c.Suit.IconChar }),
                    DeckCardsCount = playerTable.Game.Deck.CardsCount,
                    Trump = new
                    {
                        Rank = playerTable.Game.Deck.TrumpCard.Rank.Value,
                        Suit = playerTable.Game.Deck.TrumpCard.Suit.IconChar
                    }
                },
                Tables = playerTable != null ? null : TablesWithGames.Select(t => new
                {
                    Id = t.Value.Id
                }),
            };

            return result;
        }

        public void Attack(Guid tableId, string playerSecret, string playerName, int cardId)
        {
            var playerTable = TablesWithGames.Values.FirstOrDefault(t => t.PlayersAndTheirSecretKeys.ContainsKey(playerSecret));
            var player = playerTable == null ? null : playerTable.PlayersAndTheirSecretKeys[playerSecret];

            player.FirstAttack(cardId);
        }

        private bool CheckIfPlayerIsAlreadyPlayingOnAnotherTable(string playerSecret)
        {
            return TablesWithGames.Values.Any(table => table.PlayersAndTheirSecretKeys.ContainsKey(playerSecret));
        }
    }
}
