using Microsoft.AspNetCore.Mvc;

namespace Fool.CardGame.Web.Models
{
    public class AttackingModel : AuthModel
    {
        [FromQuery(Name = "cardIds")]
        public int[] CardIds { get; set; }
    }
}
