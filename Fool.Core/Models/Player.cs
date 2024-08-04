using Fool.Core.Exceptions;
using Fool.Core.Models.Cards;
using System.Text;

namespace Fool.Core.Models
{
    public class Player
    {
        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
        }

        public string Name { get; }
        public List<Card> Hand { get; }

        public void TakeCard(Card card)
        {
            Hand.Add(card);
        }

        public void TakeCards(List<Card> cards)
        {
            Hand.AddRange(cards);
        }

        public void DropHand()
        {
            Hand.Clear();
        }

        public void PlayCard(Card card)
        {
            if (Hand.Contains(card))
            {

            }
            else
            {
                throw new FoolExceptions("Cant play card which is not in my hand");
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Player: {Name}");
            sb.AppendLine("Hand:");
            foreach (var card in Hand)
            {
                sb.AppendLine(card.ToString());
            }

            return sb.ToString();
        }
    }
}
