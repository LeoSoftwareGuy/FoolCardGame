namespace Fool.Core.Models.Cards
{
    public class CardRank
    {
        /// <summary>
        /// Card Hierarchy
        /// </summary>
        /// <param name="value"><see cref="Value"/></param>
        /// <param name="name"><see cref="Name"/></param>
        public CardRank(int value, string name)
        {
            Value = value;
            Name = name;
            ShortName = name == "10" ? "10" : name.Substring(0, 1);
        }

        /// <summary>
        /// Card Value
        /// </summary>
        /// <remarks>
        /// The bigger, the better
        /// </remarks>
        public int Value { get; }

        /// <summary>
        /// Card Name.
        /// </summary>
        /// <remarks>
        /// Example: Q-Queen, K-King, A-Ace
        /// </remarks>
        public string Name { get; }

        /// <summary>
        /// Short name of the card.
        /// </summary>
        public string ShortName { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
