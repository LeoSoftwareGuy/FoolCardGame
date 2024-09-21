using Fool.Core.Exceptions;
using Fool.Core.Models.Cards;
using System.Text;

namespace Fool.Core.Models
{
    public class Player
    {
        private Game _game;
        private bool _wantsToFinishRound;
        public Player()
        {

        }
        public Player(string name, Game game)
        {
            Name = name;
            Hand = new List<Card>();
            _game = game;
            _wantsToFinishRound = false;
        }

        public string Name { get; }
        public List<Card> Hand { get; }
        public bool WantsToFinishRound
        {
            get => _wantsToFinishRound;
            private set => _wantsToFinishRound = value;
        }

        public void WantsToFinishTheRound()
        {
            _wantsToFinishRound = true;
        }

        public void RefreTheRound()
        {
            _wantsToFinishRound = false;
        }

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

        public void FirstAttack(int[] cardIndexes)
        {
            var attackingCards = new List<Card>();
            foreach (var cardIndex in cardIndexes)
            {
                if (cardIndex < 0 || cardIndex >= Hand.Count)
                {
                    throw new FoolExceptions("Card Index can be 0-5");
                }
                attackingCards.Add(Hand[cardIndex]);
            }

            if (attackingCards.GroupBy(c => c.Rank.Value).Count() > 1)
            {
                throw new FoolExceptions("Multiple cards attack can only be with identical Rank cards!");
            }

            _game.FirstAttack(this, attackingCards);
           
        }

        public void Attack(int[] cardIndexes)
        {
            var attackingCards = new List<Card>();
            foreach (var cardIndex in cardIndexes)
            {
                if (cardIndex < 0 || cardIndex >= Hand.Count)
                {
                    throw new FoolExceptions("Card Index can be 0-5");
                }
                attackingCards.Add(Hand[cardIndex]);
            }
            if (attackingCards.GroupBy(c => c.Rank.Value).Count() > 1)
            {
                throw new FoolExceptions("Multiple cards attack can only be with identical Rank cards!");
            }
            _game.Attack(this, attackingCards);
         
        }

        public void Defend(int defendingCardIndex, int attackingCardId)
        {
            if (defendingCardIndex < 0 || defendingCardIndex >= Hand.Count)
            {
                throw new FoolExceptions("Card Index can be 0-5");
            }
            var defendingCard = Hand[defendingCardIndex];
            var cardFromTheTable = _game.CardsOnTheTable[attackingCardId];

            _game.Defend(this, defendingCard, cardFromTheTable.AttackingCard);
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
