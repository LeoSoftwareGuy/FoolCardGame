using Fool.Core.Models.Table;
using Fool.CardGame.Web.Models;

namespace Fool.Core.Services.Interfaces
{
    public interface ITableService
    {
        Dictionary<Guid, Table> TablesWithGames { get; }
        Guid CreateTable();
        void SitToTheTable(string playerSecret, string playerNamer, Guid tableId);
        void Attack(string playerSecret, string playerName, int[] cardIds);
        GetStatusModel GetStatus(string playerSecret);
    }
}