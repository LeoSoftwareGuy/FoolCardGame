using Fool.Core.Models.Table;
using Fool.CardGame.Web.Models;

namespace Fool.Core.Services.Interfaces
{
    public interface ITableService
    {
        Dictionary<Guid, Table> TablesWithGames { get; }
        Guid CreateTable();
        void SitToTheTable(string playerSecret, string playerNamer, Guid tableId);
        void Attack(Guid tableId, string playerSecret, int[] cardIds);
        void StartGame(Guid tableId, string playerSecret);
        void SurrenderCurrentRound(Guid tableId, string playerSecret);
        void EndCurrentRound(Guid tableId, string playerSecret);
        void CheckIfRoundWasStopped();
        void Defend(Guid tableId, string playerSecret, int defendingCardIndex, int attackingCardIndex);
        GetStatusModel GetStatus(string playerSecret);
    }
}