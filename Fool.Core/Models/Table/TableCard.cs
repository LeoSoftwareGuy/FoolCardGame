using Fool.Core.Exceptions;
using Fool.Core.Models.Cards;

namespace Fool.Core.Models.Table
{
    public class TableCard
    {
        public TableCard(Card trumpCard, Card card)
        {
            AttackingCard = card;
            TrumpCard = trumpCard;
        }

        public Card AttackingCard { get; }
        public Card? DefendingCard { get; private set; }
        public Card TrumpCard { get; }

        public void Defend(Card defenceCard)
        {
            if (DefendingCard != null)
            {
                throw new FoolExceptions("The same Defending card cant be used twice");
            }
            //Same suit
            if (AttackingCard.Suit.Name.Equals(defenceCard.Suit.Name))
            {
                //
                if (defenceCard.Rank.Value > AttackingCard.Rank.Value)
                {
                    DefendingCard = defenceCard;
                }
                else
                {
                    throw new FoolExceptions("Defending Cards Rank is smaller then Attacking, same Suits");
                }
            }
            else
            {
                if (isTrumpCard(defenceCard))
                {
                    DefendingCard = defenceCard;
                }
                else if (isTrumpCard(AttackingCard))
                {
                    throw new FoolExceptions("Attacking Card is Trump, but Defending is not");
                }
                else
                {
                    throw new FoolExceptions("Defending Card cant have different from Attacking Card suit and not be Trump");
                }
            }
        }


        public bool isTrumpCard(Card card)
        {
            return card.Suit.Name.Equals(TrumpCard.Suit.Name);
        }
    }
}
