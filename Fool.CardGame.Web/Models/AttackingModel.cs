using Microsoft.AspNetCore.Mvc;

namespace Fool.CardGame.Web.Models
{
    public class AttackingModel : AuthModel
    {
        public Guid TableId { get; set; }

        [FromQuery(Name = "cardIds")]
        public int[] CardIds { get; set; }
    }
}
