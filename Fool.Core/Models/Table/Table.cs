namespace Fool.Core.Models.Table
{
    /// <summary>
    /// Same as Game Class but with addition of PlayersSecretKeys prop for authorization purposes
    /// </summary>
    public class Table
    {
        public Guid Id { get; set; }
        public Game Game { get; set; }
        public Dictionary<string, Player> PlayersAndTheirSecretKeys { get; set; }
    }
}
