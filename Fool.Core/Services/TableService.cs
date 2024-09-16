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
            var player = table == null ? null : table.PlayersAtTheTable.FirstOrDefault(p => p.SecretKey.Equals(playerSecret))?.Player;

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
                        AttackingSecretKey = table.PlayersAtTheTable.FirstOrDefault(p => p.Player == table.Game.AttackingPlayer)?.SecretKey,
                        OwnerSecretKey = table.Owner != null
                                                     ? table.PlayersAtTheTable.FirstOrDefault(p => p.Player == table.Owner)?.SecretKey
                                                     : null,
                        DefenderSecretKey = table.PlayersAtTheTable.FirstOrDefault(p => p.Player == table.Game.DefendingPlayer)?.SecretKey,
                        FoolSecretKey = table.PlayersAtTheTable.FirstOrDefault(p => p.Player == table.Game.FoolPlayer)?.SecretKey,
                        RoundIsEnding = table.RoundWasStoppedAt != null
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
                // Without this check, surrendering timer would be reset
                if (IfAnyAttackingPlayerDecidedToFinishTheRound(table))
                {
                    table.RoundWasStoppedAt = null;
                    table.Game.RefreshTheRound();
                    table.SetTimerForDefendingPlayersAction();
                }
            }

            table.Game.RefreshTheRound();
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
            table.SetTimerForAttackingPlayersAction();
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
            playerAtTheTable.Player.WantsToFinishTheRound();
            table.ClearAllTimers();
            table.RoundWasStoppedAt = DateTime.UtcNow;
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
                }
                else
                {
                    var amountOfTimeRemaining = (DateTime.UtcNow - table.RoundWasStoppedAt.Value).TotalSeconds;
                    if (IfAnyAttackingPlayerDecidedToFinishTheRound(table))
                    {
                        _notificationService.SendTimePassedAsync(Math.Round(amountOfTimeRemaining), false);
                    }
                    else
                    {
                        _notificationService.SendTimePassedAsync(Math.Round(amountOfTimeRemaining), true);
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
                        if (tablePlayer.WasLastActiveAt != null && isThinkingTimeUp(tablePlayer))
                        {
                            var message = LeaveTable(table.Id, tablePlayer.SecretKey);
                            tablePlayer.WasLastActiveAt = null;
                            _notificationService.SendAfkPlayerWasKickedAsync(message);
                            break;
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
            var shouldFinishAt = table.RoundWasStoppedAt.Value.AddSeconds(20);
            return DateTime.UtcNow >= shouldFinishAt;
        }

        private bool isThinkingTimeUp(PlayerAtTheTable player)
        {
            var shouldFinishAt = player.WasLastActiveAt!.Value.AddSeconds(60);
            return DateTime.UtcNow >= shouldFinishAt;
        }

    }
}
