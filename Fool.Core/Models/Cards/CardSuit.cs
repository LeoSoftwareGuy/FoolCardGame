namespace Fool.Core.Models.Cards
{
    public class CardSuit
    {
        /// <summary>
        /// Suit.
        /// </summary>
        /// <param name="value"><see cref="Value"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="iconChar"><see cref="IconChar"/></param>
        public CardSuit(int value, string name, char iconChar)
        {
            Value = value;
            Name = name;
            IconChar = iconChar;
        }

        /// <summary>
        /// Value.
        /// </summary>
        /// <remarks>Example 0 - spade, 1 - spade.</remarks>
        public int Value { get; }

        /// <summary>
        /// Name.
        /// </summary>
        /// <remarks>
        /// Example: Diamonds, Hearts
        /// </remarks>
        public string Name { get; }

        /// <summary>
        /// Short name
        /// </summary>
        /// <remarks>♥/♦/♣/♠</remarks>
        public char IconChar { get; }

        public override string ToString()
        {
            return IconChar.ToString();
        }
    }
}
