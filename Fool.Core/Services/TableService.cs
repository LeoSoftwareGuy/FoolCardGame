using Fool.CardGame.Web.Models;
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



        public GetStatusModel GetStatus(string playerSecret)
        {
            var playerTable = FindTableWhereUserIsPlaying(playerSecret);
            var player = playerTable == null ? null : playerTable.PlayersAndTheirSecretKeys[playerSecret];

            if (playerTable == null && player == null)
            {
                return new GetStatusModel
                {
                    Table = null,
                    Tables = TablesWithGames.Select(t => new GetStatusModel.TableModel { Id = t.Value.Id }).ToArray()
                };

            }
            else
            {
                return new GetStatusModel
                {
                    Table = new GetStatusModel.TableModel
                    {
                        Id = playerTable!.Id,
                        PlayerHand = player!.Hand.Select(c => new GetStatusModel.CardModel(c)).ToArray(),
                        DeckCardsCount = playerTable.Game.Deck.CardsCount,
                        Trump = new GetStatusModel.CardModel(playerTable.Game.Deck.TrumpCard),
                        CardsOnTheTable = playerTable.Game.CardsOnTheTable.Select(c => new GetStatusModel.TableCardModel(c)).ToArray(),
                        Players = playerTable.Game.Players.Select(c => new GetStatusModel.PlayerModel { Name = c.Name, CardsCount = c.Hand.Count }).ToArray()
                    },
                    Tables = null
                };
            }
        }

        public void Attack(string playerSecret, string playerName, int[] cardIds)
        {
            if (cardIds.Length == 0)
            {
                throw new FoolExceptions("You cant Attack without providing card ids");
            }

            var playerTable = FindTableWhereUserIsPlaying(playerSecret);
            var player = playerTable == null ? null : playerTable.PlayersAndTheirSecretKeys[playerSecret];

            if (playerTable == null || player == null)
            {
                throw new FoolExceptions("Player or player table is not found");
            }

            if (playerTable.Game.RoundStarted)
            {
                player.Attack(cardIds);
            }
            else
            {
                player.FirstAttack(cardIds);
            }
        }

        private bool CheckIfPlayerIsAlreadyPlayingOnAnotherTable(string playerSecret)
        {
            return TablesWithGames.Values.Any(table => table.PlayersAndTheirSecretKeys.ContainsKey(playerSecret));
        }

        private Table? FindTableWhereUserIsPlaying(string playerSecret)
        {
            var table = TablesWithGames.Values.FirstOrDefault(t => t.PlayersAndTheirSecretKeys.ContainsKey(playerSecret));
            return table == null ? null : table;
        }
    }
}
