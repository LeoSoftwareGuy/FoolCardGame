using Fool.Core.Models;

namespace Fool.Core.Models.Cards
{
    public class CardDeckGenerator
    {
        public virtual List<Card> GenerateDeck()
        {
            return CardsHolder.GetCards()
               .Select(x => new { Order = Globals.Random.Next(), Card = x })
               .OrderBy(x => x.Order)
               .Select(x => x.Card).ToList();
        }
    }
}
