using Microsoft.AspNetCore.Mvc;

namespace Fool.CardGame.Web.Models
{
    public class AttackingModel
    {
        //public Guid TableId { get; set; }
        public string PlayerSecret { get; set; }
        public string PlayerName { get; set; }

        [FromQuery(Name = "cardIds")]
        public int[] CardIds { get; set; }
    }
}
