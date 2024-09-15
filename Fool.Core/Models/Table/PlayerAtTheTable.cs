namespace Fool.Core.Models.Table
{
    // need this to track the player and his activity
    public class PlayerAtTheTable
    {
        public Player Player { get; set; }
        public DateTime? WasLastActiveAt { get; set; }
        public string SecretKey { get; set; }
    }
}
