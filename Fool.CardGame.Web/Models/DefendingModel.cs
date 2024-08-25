namespace Fool.CardGame.Web.Models
{
    public class DefendingModel : AuthModel
    {
        public Guid TableId { get; set; }
        public int AttackingCardIndex { get; set; }
        public int DefendingCardIndex { get; set; }
    }
}
