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
        private const int afkTime = 200;
        private const int actionTime = 20;
        public Dictionary<Guid, Table> TablesWithGames { get; private set; }
        public TableService(INotificationService notificationService)
        {
            TablesWithGames = new Dictionary<Guid, Table>();
            _notificationService = notificationService;
        }

        public Guid CreateTable()
        {
            var game = new Game();
            var table = new Table { Id = Guid.NewGuid(), Game = game, PlayersAtTheTable = new List<PlayerAtTheTable> { } };
            TablesWithGames.Add(table.Id, table);
            return table.Id;
        }

        public string LeaveTable(Guid tableId, string playerSecret)
        {
            var (table, playerAtTheTable) = GetTableAndPlayer(tableId, playerSecret);
            var alertMessage = $"Fucking player:{playerAtTheTable.Player.Name} has left the table.";

            // we need another owner to start the game
            if (table.Owner == playerAtTheTable.Player)
            {
                var newOwner = table.Game.Players.FirstOrDefault(p => p != playerAtTheTable.Player);
                if (newOwner != null)
                {
                    table.Owner = newOwner;
                }
            }
            var message = table.Game.RemovePlayer(playerAtTheTable.Player);
            table.PlayersAtTheTable.Remove(playerAtTheTable);
            alertMessage += message;

            // Delete the table if all players left
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
                var playerAtTheTable = new PlayerAtTheTable { Player = player, SecretKey = playerSecret };
                table.PlayersAtTheTable.Add(playerAtTheTable);

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
            var (table, playerAtTheTable) = GetTableAndPlayer(tableId, playerSecret);

            if (table.Owner != playerAtTheTable.Player)
            {
                throw new FoolExceptions("You are not table owner, you cant start the game");
            }

            table.Game.PrepareForTheGame();
            table.SetTimerForAttackingPlayersAction();
        }

        public GetStatusModel GetStatus(string playerSecret)
        {
            //If player is already siting behind the table then return the table status
            // otherwise return all tables with players
            var table = FindTableWhereUserIsPlaying(playerSecret);
            var playerAtTheTable = table == null ? null : table.PlayersAtTheTable.FirstOrDefault(p => p.SecretKey.Equals(playerSecret));
            var player = playerAtTheTable == null ? null : playerAtTheTable.Player;

            if (table == null && player == null)
            {
                return new GetStatusModel
                {
                    Table = null,
                    Tables = TablesWithGames.Select(t => new GetStatusModel.TableModel
                    {
                        Id = t.Value.Id,
                        Players = t.Value.PlayersAtTheTable
                                                  .Select(x => new GetStatusModel.PlayerModel { Name = x.Player.Name })
                                                  .ToArray()
                    }).ToArray()
                };

            }
            else
            {
                var statusModel = new GetStatusModel();
                statusModel.Tables = null;
                statusModel.Table = new GetStatusModel.TableModel();
                statusModel.Table.Id = table!.Id;
                statusModel.Table.MyIndex = table.Game.Players.IndexOf(player!);
                statusModel.Table.DefenderIndex = table.Game.Players.IndexOf(table.Game.DefendingPlayer!);
                statusModel.Table.FoolPlayerIndex = table.Game.FoolPlayer != null
                                                                          ? table.Game.Players.IndexOf(table.Game.FoolPlayer)
                                                                          : null;

                statusModel.Table.DoIWishToFinishTheRound = player!.WantsToFinishRound;
                statusModel.Table.PlayerHand = player.Hand.Select(c => new GetStatusModel.CardModel(c)).ToArray();
                statusModel.Table.DeckCardsCount = table.Game.Deck.CardsCount;
                statusModel.Table.Trump = table.Game.Deck.TrumpCard != null
                                                                    ? new GetStatusModel.CardModel(table.Game.Deck.TrumpCard)
                                                                    : null;

                statusModel.Table.CardsOnTheTable = table.Game.CardsOnTheTable.Select(c => new GetStatusModel.TableCardModel(c)).ToArray();
                statusModel.Table.Status = table.Game.GameStatus == null
                                                                  ? null
                                                                  : table.Game.GameStatus.ToString();

                statusModel.Table.AttackingSecretKey = table.PlayersAtTheTable.FirstOrDefault(p => p.Player == table.Game.AttackingPlayer)?.SecretKey;

                statusModel.Table.OwnerSecretKey = table.Owner != null
                                                                ? table.PlayersAtTheTable.FirstOrDefault(p => p.Player == table.Owner)?.SecretKey
                                                                : null;

                statusModel.Table.DefenderSecretKey = table.PlayersAtTheTable.FirstOrDefault(p => p.Player == table.Game.DefendingPlayer)?.SecretKey;

                statusModel.Table.FoolSecretKey = table.PlayersAtTheTable.FirstOrDefault(p => p.Player == table.Game.FoolPlayer)?.SecretKey;

                statusModel.Table.RoundIsEnding = table.RoundWasStoppedAt != null
                                                                           ? true
                                                                           : false;

                bool? isRoundEnding = table.RoundWasStoppedAt != null;
                bool isPlayerActive = playerAtTheTable?.WasLastActiveAt != null;

                // Set the AFK timer flag for the table based on conditions
                statusModel.Table.isAfkTimeOn = isRoundEnding == false && isPlayerActive;

                // Populate Players array
                statusModel.Table.Players = table.Game.Players
                    .Where(p => p != player)
                    .Select((c, i) => new GetStatusModel.PlayerModel
                    {
                        Index = i,
                        Name = c.Name,
                        CardsCount = c.Hand.Count,
                        WantsToFinishRound = c.WantsToFinishRound,
                        isAfkTimerOn = false
                    })
                    .ToArray();

                // Update isAfkTimerOn for each player if Round is not ending
                if (isRoundEnding == false)
                {
                    foreach (var playerModel in statusModel.Table.Players)
                    {
                        var playerAtTheTableModel = table.PlayersAtTheTable
                            .FirstOrDefault(s => s.Player.Name == playerModel.Name);

                        if (playerAtTheTableModel != null)
                        {
                            playerModel.isAfkTimerOn = playerAtTheTableModel.WasLastActiveAt != null;
                        }
                    }
                }

                return statusModel;
            }
        }


        public void Attack(Guid tableId, string playerSecret, int[] cardIds)
        {
            if (cardIds.Length == 0)
            {
                throw new FoolExceptions("You can't attack without providing card IDs");
            }

            var (table, playerAtTheTable) = GetTableAndPlayer(tableId, playerSecret);

            var allowedToAddCardsCount = 6 - table.Game.CardsOnTheTable.Count;
            cardIds = cardIds.Take(allowedToAddCardsCount).ToArray();

            var defendingPlayerCardsCount = table.Game.DefendingPlayer!.Hand.Count;
            if (defendingPlayerCardsCount < cardIds.Length)
            {
                throw new FoolExceptions("You can't attack with more cards than the defending player has");
            }

            if (table.Game.RoundStarted)
            {
                playerAtTheTable.Player.Attack(cardIds);
            }
            else
            {
                playerAtTheTable.Player.FirstAttack(cardIds);
            }

            if (table.RoundWasStoppedAt != null)
            {
                // Stop Finishing of the round if somebody decided to add attacking cards
                // Not for Surrender
                if (IfAnyAttackingPlayerDecidedToFinishTheRound(table))
                {
                    table.RoundWasStoppedAt = null;
                    table.Game.RefreshTheRound();
                    if (isGameInProgress(table))
                    {
                        table.SetTimerForDefendingPlayersAction();
                    }
                }
            }
            else
            {
                if (isGameInProgress(table))
                {
                    table.SetTimerForDefendingPlayersAction();
                }
                else
                {
                    table.ClearAllTimers();
                }
            }
        }


        public void Defend(Guid tableId, string playerSecret, int defendingCardIndex, int attackingCardIndex)
        {
            var (table, playerAtTheTable) = GetTableAndPlayer(tableId, playerSecret);

            if (table.RoundWasStoppedAt != null)
            {
                throw new FoolExceptions("You have already decided to surrender, its too late to change your mind!");
            }
            else
            {
                playerAtTheTable.Player.Defend(defendingCardIndex, attackingCardIndex);
            }

            if (AreAllCardsOnTheTableWereDefended(table))
            {
                if (isGameInProgress(table))
                {
                    table.SetTimerForAttackingPlayersAction();
                }
                else
                {
                    table.ClearAllTimers();
                }
            }
        }


        public void SurrenderCurrentRound(Guid tableId, string playerSecret)
        {
            var (table, playerAtTheTable) = GetTableAndPlayer(tableId, playerSecret);

            if (table.Game.FoolPlayer != null)
            {
                throw new FoolExceptions("Game is already over!");
            }

            if (table.Game.DefendingPlayer != playerAtTheTable.Player)
            {
                throw new FoolExceptions("Only the defending player can surrender.");
            }

            if (table.RoundWasStoppedAt != null)
            {
                throw new FoolExceptions("The round is already in the process of stopping!");
            }

            table.ClearAllTimers();
            table.RoundWasStoppedAt = DateTime.UtcNow;
        }


        public void EndCurrentRound(Guid tableId, string playerSecret)
        {
            var (table, playerAtTheTable) = GetTableAndPlayer(tableId, playerSecret);

            if (!table.Game.CardsOnTheTable.All(c => c.DefendingCard != null))
            {
                throw new FoolExceptions("All Cards on the table should be defended");
            }

            if (playerAtTheTable.Player == table.Game.DefendingPlayer)
            {
                throw new FoolExceptions("Only attacking player can finish the round");
            }

            if (playerAtTheTable.Player.WantsToFinishRound)
            {
                throw new FoolExceptions("You have already finished the round!");
            }

            if (table.RoundWasStoppedAt != null)
            {
                throw new FoolExceptions("The round is already in the process of stopping!");
            }


            // if 2 players are playing, then the round is finished
            if (table.PlayersAtTheTable.Count.Equals(2))
            {
                table.RoundWasStoppedAt = null;
                table.Game.FinishTheRound();
                table.ClearAllTimers();
                if (isGameInProgress(table))
                {
                    table.SetTimerForAttackingPlayersAction();
                }
            }
            else
            {
                playerAtTheTable.Player.WantsToFinishTheRound();
                table.ClearAllTimers();
                table.RoundWasStoppedAt = DateTime.UtcNow;
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

                    _notificationService.SendRoundFinishedAsync();

                    // If game isnt over yet
                    if (isGameInProgress(table))
                    {
                        table.SetTimerForAttackingPlayersAction();
                    }
                }
                else
                {
                    var totalRoundTime = 20;
                    var elapsedTime = (DateTime.UtcNow - table.RoundWasStoppedAt.Value).TotalSeconds;
                    var timeRemaining = totalRoundTime - elapsedTime;

                    if (IfAnyAttackingPlayerDecidedToFinishTheRound(table))
                    {
                        _notificationService.SendTimePassedAsync(Math.Round(timeRemaining), false);
                    }
                    else
                    {
                        _notificationService.SendTimePassedAsync(Math.Round(timeRemaining), true);
                    }
                }
            }
        }

        public void CheckIfThereAreAfkPlayers()
        {
            foreach (var table in TablesWithGames.Values)
            {
                if (table.PlayersAtTheTable.Any(p => p.WasLastActiveAt != null))
                {
                    foreach (var tablePlayer in table.PlayersAtTheTable)
                    {
                        if (tablePlayer.WasLastActiveAt != null)
                        {
                            if (isThinkingTimeUp(tablePlayer))
                            {
                                var message = LeaveTable(table.Id, tablePlayer.SecretKey);
                                tablePlayer.WasLastActiveAt = null;
                                _notificationService.SendAfkPlayerWasKickedAsync(message);
                                if (isGameInProgress(table))
                                {
                                    table.SetTimerForAttackingPlayersAction();
                                }
                            }
                            else
                            {
                                var totalAfkTime = 200; // Example: total AFK time allowed in seconds
                                var elapsedTime = (DateTime.UtcNow - tablePlayer.WasLastActiveAt.Value).TotalSeconds;
                                var timeRemaining = totalAfkTime - elapsedTime;

                                _notificationService.SendAfkPlayerTimeLeftAsync(Math.Round(timeRemaining));
                            }
                        }
                    }
                }
            }
        }




        private (Table, PlayerAtTheTable) GetTableAndPlayer(Guid tableId, string playerSecret)
        {
            if (!TablesWithGames.TryGetValue(tableId, out var table))
                throw new FoolExceptions("Table was not found");

            var playerAtTheTable = table.PlayersAtTheTable.FirstOrDefault(p => p.SecretKey == playerSecret);
            if (playerAtTheTable == null)
                throw new FoolExceptions("Player was not found");

            return (table, playerAtTheTable);
        }



        private bool AreAllCardsOnTheTableWereDefended(Table table)
        {
            return table.Game.CardsOnTheTable.All(c => c.DefendingCard != null);
        }

        private bool CheckIfPlayerIsAlreadyPlayingOnAnotherTable(string playerSecret)
        {
            return TablesWithGames.Values.Any(table => table.PlayersAtTheTable.Any(p => p.SecretKey == playerSecret));
        }


        private Table? FindTableWhereUserIsPlaying(string playerSecret)
        {
            return TablesWithGames.Values.FirstOrDefault(t => t.PlayersAtTheTable.Any(p => p.SecretKey == playerSecret));
        }


        private bool IfThereIsOnlyOnePlayerOnTheTable(Table table)
        {
            return table.PlayersAtTheTable.Count == 1;
        }

        private bool IfAnyAttackingPlayerDecidedToFinishTheRound(Table table)
        {
            return table.PlayersAtTheTable.Any(p => p.Player.WantsToFinishRound);
        }

        private bool IsTimeUp(Table table)
        {
            var shouldFinishAt = table.RoundWasStoppedAt.Value.AddSeconds(actionTime);
            return DateTime.UtcNow >= shouldFinishAt;
        }

        private bool isThinkingTimeUp(PlayerAtTheTable player)
        {
            var shouldFinishAt = player.WasLastActiveAt!.Value.AddSeconds(afkTime);
            return DateTime.UtcNow >= shouldFinishAt;
        }

        private bool isGameInProgress(Table table)
        {
            return table.Game.GameStatus == GameStatus.InProgress;
        }

    }
}
