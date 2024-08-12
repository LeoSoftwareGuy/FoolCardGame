namespace Fool.CardGame.Web.Models
{
    public class JoinTableModel
    {
        public string PlayerSecret { get; set; }
        public string PlayerName { get; set; }
        public Guid TableGuid { get; set; }
    }
}
