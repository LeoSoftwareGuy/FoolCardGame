namespace Fool.Core.Models.Table
{
    /// <summary>
    /// Same as Game Class but with addition of PlayersSecretKeys prop for authorization purposes
    /// </summary>
    public class Table
    {
        public Guid Id { get; set; }
        public Game Game { get; set; }
        public List<PlayerAtTheTable> PlayersAtTheTable { get; set; }
        public Player Owner { get; set; }

       
        public DateTime? RoundWasStoppedAt { get; set; }

        public void SetTimerForAttackingPlayersAction()
        {
            PlayersAtTheTable.FirstOrDefault(x=>x.Player == Game.AttackingPlayer)!.WasLastActiveAt = DateTime.UtcNow;
        }
        public void SetTimerForDefendingPlayersAction()
        {
            PlayersAtTheTable.FirstOrDefault(x => x.Player == Game.AttackingPlayer)!.WasLastActiveAt = null; // not waiting for the attacking player anymore
            PlayersAtTheTable.FirstOrDefault(x => x.Player == Game.DefendingPlayer)!.WasLastActiveAt = DateTime.UtcNow;
        }

        public void ClearAllTimers()
        {
            PlayersAtTheTable.FirstOrDefault(x => x.Player == Game.AttackingPlayer)!.WasLastActiveAt = null; 
            PlayersAtTheTable.FirstOrDefault(x => x.Player == Game.DefendingPlayer)!.WasLastActiveAt = null;
        }
    }
}
