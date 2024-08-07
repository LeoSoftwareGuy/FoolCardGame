using Fool.Core.Exceptions;
using Fool.Core.Models.Cards;
using System.Text;

namespace Fool.Core.Models
{
    public class Player
    {
        public Player()
        {
            
        }
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

        public Card PlayCard(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= Hand.Count)
            {
                throw new FoolExceptions("Card Index can be 0-5");
            }
            var card = Hand[cardIndex];
            Hand.Remove(card);
            return card;
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
