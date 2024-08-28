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
            var table = new Table { Id = tableId, Game = game, PlayersAndTheirSecretKeys = new Dictionary<string, Player> { } };
            TablesWithGames.Add(tableId, table);
            return tableId;
        }

        public void SitToTheTable(string playerSecret, string playerName, Guid tableId)
        {
            // player who sit first is the main player
            // when function to quit the game will be added will come back to this, as we will need to make new main player
            // when all players leave the table, table is deleted
            if (CheckIfPlayerIsAlreadyPlayingOnAnotherTable(playerSecret))
            {
                throw new FoolExceptions("You are already plaing on another table");
            }

            if (TablesWithGames.FirstOrDefault(c => c.Key.Equals(tableId)).Value is Table table)
            {
                var player = table.Game.AddPlayer(playerName);
                table.PlayersAndTheirSecretKeys.Add(playerSecret, player);

                if (IfThereIsOnlyOnePlayerOnTheTable(table))
                {
                    table.Owner = player;
                }
            }
            else
            {
                throw new FoolExceptions("Table was not found");
            }
        }



        public GetStatusModel GetStatus(string playerSecret)
        {
            //If player is already siting behind the table then return the table status
            // otherwise return all tables with players
            var playerTable = FindTableWhereUserIsPlaying(playerSecret);
            var player = playerTable == null ? null : playerTable.PlayersAndTheirSecretKeys[playerSecret];

            if (playerTable == null && player == null)
            {
                return new GetStatusModel
                {
                    Table = null,
                    Tables = TablesWithGames.Select(t => new GetStatusModel.TableModel
                    {
                        Id = t.Value.Id,
                        Players = t.Value.PlayersAndTheirSecretKeys.Select(x => x.Value)
                                                                   .Select(x => new GetStatusModel.PlayerModel { Name = x.Name })
                                                                   .ToArray()
                    }).ToArray()
                };

            }
            else
            {
                return new GetStatusModel
                {
                    Table = new GetStatusModel.TableModel
                    {
                        Id = playerTable!.Id,
                        MyIndex = playerTable.Game.Players.IndexOf(player!),
                        DoIWishToFinishTheRound = player!.WantsToFinishRound,
                        ActivePlayerIndex = playerTable.Game.Players.IndexOf(playerTable.Game.AttackingPlayer!),
                        AttackingSecretKey = playerTable.PlayersAndTheirSecretKeys.FirstOrDefault(p => p.Value == playerTable.Game.AttackingPlayer).Key,
                        PlayerHand = player.Hand.Select(c => new GetStatusModel.CardModel(c)).ToArray(),
                        DeckCardsCount = playerTable.Game.Deck.CardsCount,
                        Trump = playerTable.Game.Deck.TrumpCard != null
                                                                ? new GetStatusModel.CardModel(playerTable.Game.Deck.TrumpCard)
                                                                : null,
                        CardsOnTheTable = playerTable.Game.CardsOnTheTable.Select(c => new GetStatusModel.TableCardModel(c)).ToArray(),
                        Players = playerTable.Game.Players.Where(p => p != player)
                                                          .Select((c, i) => new GetStatusModel.PlayerModel { Index = i, Name = c.Name, CardsCount = c.Hand.Count, WantsToFinishRound = c.WantsToFinishRound })
                                                          .ToArray(),
                        Status = playerTable.Game.GameStatus != null
                                                             ? playerTable.Game.GameStatus.ToString()
                                                             : null,
                        OwnerSecretKey = playerTable.Owner != null
                                                  ? playerTable.PlayersAndTheirSecretKeys.FirstOrDefault(p => p.Value == playerTable.Owner).Key
                                                : null,
                        DefenderSecretKey = playerTable.PlayersAndTheirSecretKeys.FirstOrDefault(p => p.Value == playerTable.Game.DefendingPlayer).Key
                    },
                    Tables = null
                };
            }
        }

        public void StartGame(Guid tableId, string playerSecret)
        {
            var table = TablesWithGames[tableId];
            var player = table.PlayersAndTheirSecretKeys[playerSecret];
            if (table == null || player == null)
            {
                throw new FoolExceptions("Either table or player were not found!");
            }

            if (table.Owner != player)
            {
                throw new FoolExceptions("You are not table owner, you cant start the game");
            }

            table.Game.PrepareForTheGame();
        }


        public void Attack(Guid tableId, string playerSecret, int[] cardIds)
        {
            if (cardIds.Length == 0)
            {
                throw new FoolExceptions("You cant Attack without providing card ids");
            }

            var playerTable = TablesWithGames[tableId];
            var player = playerTable == null ? null : playerTable.PlayersAndTheirSecretKeys[playerSecret];

            if (playerTable == null || player == null)
            {
                throw new FoolExceptions("Player or player table is not found");
            }

            if (playerTable.Game.RoundStarted)
            {
                player.Attack(cardIds);
                playerTable.Game.RefreshTheRound();
            }
            else
            {
                player.FirstAttack(cardIds);
                playerTable.Game.RefreshTheRound();
            }
        }

        public void Defend(Guid tableId, string playerSecret, int defendingCardIndex, int attackingCardIndex)
        {
            var table = TablesWithGames[tableId];
            var player = table.PlayersAndTheirSecretKeys[playerSecret];

            if (table == null || player == null)
            {
                throw new FoolExceptions("Player or player table is not found");
            }

            player.Defend(defendingCardIndex, attackingCardIndex);
        }


        public void SurrenderCurrentRound(Guid tableId, string playerSecret)
        {
            var table = TablesWithGames[tableId];
            var player = table.PlayersAndTheirSecretKeys[playerSecret];

            if (table == null || player == null)
            {
                throw new FoolExceptions("Player or player table is not found");
            }

            if (table.Game.DefendingPlayer == player)
            {
                table.Game.FinishTheRound();
            }
            else
            {
                throw new FoolExceptions("You can only surrender if you are defending");
            }
        }

        public void EndCurrentRound(Guid tableId, string playerSecret)
        {
            var table = TablesWithGames[tableId];
            var player = table.PlayersAndTheirSecretKeys[playerSecret];

            if (table == null || player == null)
            {
                throw new FoolExceptions("Player or player table is not found");
            }

            if (!table.Game.CardsOnTheTable.All(c => c.DefendingCard != null))
            {
                throw new FoolExceptions("All Cards on the table should be defended");
            }

            if (player == table.Game.DefendingPlayer)
            {
                throw new FoolExceptions("Only attacking player can finish the round");
            }

            if (player.WantsToFinishRound)
            {
                throw new FoolExceptions("You have already finished the round!");
            }

            // We set the current player prop that he want to finish the round, then we check other attacking players,
            // if they all want to finish the round , then we actually finish
            // otherwise nothing happens appart from setting the prop.

            player.WantsToFinishTheRound();
            if (IfAllAttackingPlayersWantToFinishTheRound(table))
            {
                table.Game.FinishTheRound();
                table.Game.RefreshTheRound();          
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

        private bool IfThereIsOnlyOnePlayerOnTheTable(Table table)
        {
            return table.PlayersAndTheirSecretKeys.Values.Count == 1;
        }

        private bool IfAllAttackingPlayersWantToFinishTheRound(Table table)
        {
            return table.Game.Players.Where(player => player != table.Game.DefendingPlayer).All(player => player.WantsToFinishRound);
        }
    }
}
