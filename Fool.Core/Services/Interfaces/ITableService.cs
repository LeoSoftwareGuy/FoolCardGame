using Fool.Core.Models.Table;

namespace Fool.Core.Services.Interfaces
{
    public interface ITableService
    {
        Dictionary<Guid, Table> TablesWithGames { get; }
        Guid CreateTable();
        void SitToTheTable(string playerSecret, string playerNamer, Guid tableId);
        void Attack(string playerSecret, string playerName, int[] cardIds);
        dynamic GetStatus(string playerSecret);
    }
}