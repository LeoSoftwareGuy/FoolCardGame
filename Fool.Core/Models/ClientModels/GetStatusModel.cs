using Fool.Core.Models.Cards;
using Fool.Core.Models.Table;

namespace Fool.CardGame.Web.Models
{
    public class GetStatusModel
    {
        public TableModel? Table { get; set; }
        public TableModel[]? Tables { get; set; }

        public class TableModel
        {
            public Guid Id { get; set; }
            public int DeckCardsCount { get; set; }
            public CardModel[]? PlayerHand { get; set; }
            public CardModel? Trump { get; set; }
            public TableCardModel[]? CardsOnTheTable { get; set; }
            public PlayerModel[]? Players { get; set; }
            public int MyIndex { get; set; }
            public int ActivePlayerIndex { get; set; }
            public string? Status { get; set; }
            public string? OwnerSecretKey { get; set; }
        }

        public class CardModel
        {
            public CardModel(Card businessCard)
            {
                Rank = businessCard.Rank.Value;
                Suit = businessCard.Suit.IconChar;
            }
            public int Rank { get; set; }
            public char Suit { get; set; }
        }

        public class TableCardModel
        {
            public TableCardModel(TableCard businessTableCard)
            {
                AttackingCard = new CardModel(businessTableCard.AttackingCard);
                DefendingCard = businessTableCard.DefendingCard == null ? null
                                                                        : new CardModel(businessTableCard.DefendingCard);
            }
            public CardModel AttackingCard { get; set; }
            public CardModel? DefendingCard { get; set; }
        }

        public class PlayerModel
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public int CardsCount { get; set; }
        }

    }
}
