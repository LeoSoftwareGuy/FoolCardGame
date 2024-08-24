namespace Fool.CardGame.Web.Models
{
    public class JoinTableModel : AuthModel
    {
        public string PlayerName { get; set; }
        public Guid TableGuid { get; set; }
    }
}
