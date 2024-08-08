using Fool.Core.Exceptions;
using Fool.Core.Models.Cards;
using System.Text;

namespace Fool.Core.Models
{
    public class Player
    {
        private Game _game;
        public Player()
        {

        }
        public Player(string name, Game game)
        {
            Name = name;
            Hand = new List<Card>();
            _game = game;
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

        public void FirstAttack(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= Hand.Count)
            {
                throw new FoolExceptions("Card Index can be 0-5");
            }
            var card = Hand[cardIndex];
           
            _game.FirstAttack(this, card);
            Hand.Remove(card);
        }

        public void Attack(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= Hand.Count)
            {
                throw new FoolExceptions("Card Index can be 0-5");
            }
            var card = Hand[cardIndex];
            _game.Attack(this, card);
            Hand.Remove(card);
        }

        public void Defend(int defendingCardIndex, int attackingCardIndex)
        {
            if (defendingCardIndex < 0 || defendingCardIndex >= Hand.Count)
            {
                throw new FoolExceptions("Card Index can be 0-5");
            }
            var defendingCard = Hand[defendingCardIndex];
            var cardFromTheTable = _game.CardsOnTheTable[attackingCardIndex];
          
            _game.Defend(this, defendingCard, cardFromTheTable.AttackingCard);
            Hand.Remove(defendingCard);
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
