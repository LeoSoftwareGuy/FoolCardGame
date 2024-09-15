using Fool.CardGame.Web.Models;
using Fool.Core.Exceptions;
using Fool.Core.Models;
using Fool.Core.Models.Table;
using Fool.Core.Services.Interfaces;

namespace Fool.Core.Services
{
    public class TableService : ITableService
    {
        private readonly INotificationService _notificationService;
        public Dictionary<Guid, Table> TablesWithGames { get; private set; }
        public TableService(INotificationService notificationService)
        {
            TablesWithGames = new Dictionary<Guid, Table>();
            _notificationService = notificationService;
        }

        public Guid CreateTable()
        {
            var game = new Game();
            var table = new Table { Id = Guid.NewGuid(), Game = game, PlayersAndTheirSecretKeys = new Dictionary<string, Player> { } };
            TablesWithGames.Add(table.Id, table);
            return table.Id;
        }

        public string LeaveTable(Guid tableId, string playerSecret)
        {
            var (table, player) = GetTableAndPlayer(tableId, playerSecret);
            var alertMessage = $"Fucking player:{player.Name} has left the table.";

            // we need another owner to start the game
            if (table.Owner == player)
            {
                var newOwner = table.Game.Players.FirstOrDefault(p => p != player);
                if (newOwner != null)
                {
                    table.Owner = newOwner;
                }
            }
            var message = table.Game.RemovePlayer(player);
            table.PlayersAndTheirSecretKeys.Remove(playerSecret);
            alertMessage += message;

            // Delete the table completelt if all players left
            if (table.Game.Players.Count.Equals(0))
            {
                TablesWithGames.Remove(table.Id);

            }

            return alertMessage;
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

        public void StartGame(Guid tableId, string playerSecret)
        {
            var (table, player) = GetTableAndPlayer(tableId, playerSecret);

            if (table.Owner != player)
            {
                throw new FoolExceptions("You are not table owner, you cant start the game");
            }

            table.Game.PrepareForTheGame();
        }

        public GetStatusModel GetStatus(string playerSecret)
        {
            //If player is already siting behind the table then return the table status
            // otherwise return all tables with players
            var table = FindTableWhereUserIsPlaying(playerSecret);
            var player = table == null ? null : table.PlayersAndTheirSecretKeys[playerSecret];

            if (table == null && player == null)
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
                        Id = table!.Id,
                        MyIndex = table.Game.Players.IndexOf(player!),
                        DefenderIndex = table.Game.Players.IndexOf(table.Game.DefendingPlayer!),
                        FoolPlayerIndex = table.Game.FoolPlayer != null ? table.Game.Players.IndexOf(table.Game.FoolPlayer)
                                                                        : null,
                        DoIWishToFinishTheRound = player!.WantsToFinishRound,
                        PlayerHand = player.Hand.Select(c => new GetStatusModel.CardModel(c)).ToArray(),
                        DeckCardsCount = table.Game.Deck.CardsCount,
                        Trump = table.Game.Deck.TrumpCard != null
                                                                ? new GetStatusModel.CardModel(table.Game.Deck.TrumpCard)
                                                                : null,
                        CardsOnTheTable = table.Game.CardsOnTheTable.Select(c => new GetStatusModel.TableCardModel(c)).ToArray(),
                        Players = table.Game.Players.Where(p => p != player)
                                                          .Select((c, i) => new GetStatusModel.PlayerModel { Index = i, Name = c.Name, CardsCount = c.Hand.Count, WantsToFinishRound = c.WantsToFinishRound })
                                                          .ToArray(),
                        Status = table.Game.GameStatus == null
                                                             ? null
                                                             : table.Game.GameStatus.ToString(),
                        AttackingSecretKey = table.PlayersAndTheirSecretKeys.FirstOrDefault(p => p.Value == table.Game.AttackingPlayer).Key,
                        OwnerSecretKey = table.Owner != null
                                                     ? table.PlayersAndTheirSecretKeys.FirstOrDefault(p => p.Value == table.Owner).Key
                                                     : null,
                        DefenderSecretKey = table.PlayersAndTheirSecretKeys.FirstOrDefault(p => p.Value == table.Game.DefendingPlayer).Key,
                        FoolSecretKey = table.PlayersAndTheirSecretKeys.FirstOrDefault(p => p.Value == table.Game.FoolPlayer).Key,
                        SurrenderHasStarted = table.RoundWasStoppedAt != null
                                                                      ? true
                                                                      : false,
                    },
                    Tables = null
                };
            }
        }


        public void Attack(Guid tableId, string playerSecret, int[] cardIds)
        {
            if (cardIds.Length == 0)
            {
                throw new FoolExceptions("You can't attack without providing card IDs");
            }

            var (table, player) = GetTableAndPlayer(tableId, playerSecret);

            var allowedToAddCardsCount = 6 - table.Game.CardsOnTheTable.Count;
            cardIds = cardIds.Take(allowedToAddCardsCount).ToArray();

            var defendingPlayerCardsCount = table.Game.DefendingPlayer!.Hand.Count;
            if (defendingPlayerCardsCount < cardIds.Length)
            {
                throw new FoolExceptions("You can't attack with more cards than the defending player has");
            }

            if (table.Game.RoundStarted)
            {
                player.Attack(cardIds);
            }
            else
            {
                player.FirstAttack(cardIds);
            }

            table.Game.RefreshTheRound();
        }


        public void Defend(Guid tableId, string playerSecret, int defendingCardIndex, int attackingCardIndex)
        {
            var (table, player) = GetTableAndPlayer(tableId, playerSecret);

            if (table.RoundWasStoppedAt != null)
            {
                throw new FoolExceptions("You have already decided to surrender, its too late to change your mind!");
            }
            else
            {
                player.Defend(defendingCardIndex, attackingCardIndex);
            }
        }


        public void SurrenderCurrentRound(Guid tableId, string playerSecret)
        {
            var (table, player) = GetTableAndPlayer(tableId, playerSecret);

            if (table.Game.FoolPlayer != null)
            {
                throw new FoolExceptions("Game is already over!");
            }

            if (table.Game.DefendingPlayer != player)
            {
                throw new FoolExceptions("Only the defending player can surrender.");
            }

            if (table.RoundWasStoppedAt != null)
            {
                throw new FoolExceptions("The round is already in the process of stopping!");
            }

            table.RoundWasStoppedAt = DateTime.UtcNow;
        }


        public void EndCurrentRound(Guid tableId, string playerSecret)
        {
            var (table, player) = GetTableAndPlayer(tableId, playerSecret);

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

        public void CheckIfRoundWasStopped()
        {
            foreach (var table in TablesWithGames.Values)
            {
                if (table.RoundWasStoppedAt == null)
                    continue;

                if (IsTimeUp(table))
                {
                    table.RoundWasStoppedAt = null;
                    table.Game.FinishTheRound();
                    _notificationService.SendSurrenderFinishedAsync();
                }
                else
                {
                    var amountOfTimeRemaining = (DateTime.UtcNow - table.RoundWasStoppedAt.Value).TotalSeconds;
                    _notificationService.SendTimePassedAsync(Math.Round(amountOfTimeRemaining));
                }
            }
        }


        private (Table, Player) GetTableAndPlayer(Guid tableId, string playerSecret)
        {
            if (!TablesWithGames.TryGetValue(tableId, out var table))
                throw new FoolExceptions("Table was not found");

            if (!table.PlayersAndTheirSecretKeys.TryGetValue(playerSecret, out var player))
                throw new FoolExceptions("Player was not found");

            return (table, player);
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

        private bool IsTimeUp(Table table)
        {
            var shouldFinishAt = table.RoundWasStoppedAt.Value.AddSeconds(20);
            return DateTime.UtcNow >= shouldFinishAt;
        }

    }
}
