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
                        DoIWishToFinishTheRound = player!.WantsToFinishRound,
                        ActivePlayerIndex = table.Game.Players.IndexOf(table.Game.AttackingPlayer!),
                        AttackingSecretKey = table.PlayersAndTheirSecretKeys.FirstOrDefault(p => p.Value == table.Game.AttackingPlayer).Key,
                        PlayerHand = player.Hand.Select(c => new GetStatusModel.CardModel(c)).ToArray(),
                        DeckCardsCount = table.Game.Deck.CardsCount,
                        Trump = table.Game.Deck.TrumpCard != null
                                                                ? new GetStatusModel.CardModel(table.Game.Deck.TrumpCard)
                                                                : null,
                        CardsOnTheTable = table.Game.CardsOnTheTable.Select(c => new GetStatusModel.TableCardModel(c)).ToArray(),
                        Players = table.Game.Players.Where(p => p != player)
                                                          .Select((c, i) => new GetStatusModel.PlayerModel { Index = i, Name = c.Name, CardsCount = c.Hand.Count, WantsToFinishRound = c.WantsToFinishRound })
                                                          .ToArray(),
                        Status = table.Game.GameStatus != null
                                                             ? table.Game.GameStatus.ToString()
                                                             : null,
                        OwnerSecretKey = table.Owner != null
                                                  ? table.PlayersAndTheirSecretKeys.FirstOrDefault(p => p.Value == table.Owner).Key
                                                : null,
                        DefenderSecretKey = table.PlayersAndTheirSecretKeys.FirstOrDefault(p => p.Value == table.Game.DefendingPlayer).Key
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

            var table = TablesWithGames[tableId];
            var player = table == null ? null : table.PlayersAndTheirSecretKeys[playerSecret];

            if (table == null || player == null)
            {
                throw new FoolExceptions("Player or player table is not found");
            }

            var allowedToAddCardsCount = 6 - table.Game.CardsOnTheTable.Count;
            if (allowedToAddCardsCount > 0)
            {
                cardIds = cardIds.Take(allowedToAddCardsCount).ToArray();

                var defendingPlayerCardsCount = table.Game.DefendingPlayer!.Hand.Count;
                if (defendingPlayerCardsCount < cardIds.Length)
                {
                    throw new FoolExceptions("You cant attack with more cards than the defending player has");
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
            else
            {
                throw new FoolExceptions("There can only be 6 attacking cards on the table");
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
            // Here we should check if there are 6 attacking cards on the table
            // if there are, finish the round
            // otherwise, start the timer for 10seconds and dont finish the round
            // during this time other players can attack 
            // once time is up, finish the round

            var table = TablesWithGames[tableId];
            var player = table.PlayersAndTheirSecretKeys[playerSecret];

            if (table == null || player == null)
            {
                throw new FoolExceptions("Player or player table is not found");
            }

            if (table.Game.DefendingPlayer == player)
            {
                if (table.RoundWasStoppedAt != null)
                {
                    throw new FoolExceptions("Round is in the process of stopping!");
                }
                else
                {
                    table.RoundWasStoppedAt = DateTime.UtcNow;
                }
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

        public void CheckIfRoundWasStopped()
        {
            //Todo thread safety to be added
            foreach (var table in TablesWithGames.Values)
            {
                if (table.RoundWasStoppedAt != null)
                {
                    // basically here we are checking if the 5 seconds have passed since the round was stopped
                    var shouldFinishAt = table.RoundWasStoppedAt.Value.AddSeconds(5);
                    var amountOfTimeRemaining = (DateTime.UtcNow - table.RoundWasStoppedAt.Value).TotalSeconds;
                    if (DateTime.UtcNow >= shouldFinishAt)
                    {
                        table.RoundWasStoppedAt = null;
                        table.Game.FinishTheRound();

                        _notificationService.SendSurrenderFinishedAsync();
                    }
                    else
                    {
                        _notificationService.SendTimePassedAsync(Math.Round(amountOfTimeRemaining));
                    }
                }
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
