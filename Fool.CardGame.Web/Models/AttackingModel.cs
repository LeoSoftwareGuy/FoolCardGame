namespace Fool.CardGame.Web.Models
{
    public class AttackingModel
    {
        public Guid TableId { get; set; }
        public string PlayerSecret { get; set; }
        public string PlayerName { get; set; }
        public int CardId { get; set; }
    }
}
