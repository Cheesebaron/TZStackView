namespace TZStackView
{
    /// <summary>
    /// Alignment, the layout transverse to the stacking axis.
    /// </summary>
    public enum Alignment
    {
        /// <summary>
        /// Align the leading and trailing edges of vertically stacked items
        /// or the top and bottom edges of horizontally stacked items tightly to the container.
        /// </summary>
        Fill = 0,

        /// <summary>
        /// Align the leading edges of vertically stacked items
        /// or the top edges of horizontally stacked items tightly to the relevant edge
        /// of the container.
        /// </summary>
        Leading = 1,

        /// <summary>
        /// Align the leading edges of vertically stacked items
        /// or the top edges of horizontally stacked items tightly to the relevant edge
        /// of the container.
        /// </summary>
        Top = Leading,

        FirstBaseline = 2,

        /// <summary>
        /// Center the items in a vertical stack horizontally
        /// or the items in a horizontal stack vertically
        /// </summary>
        Center = 3,

        /// <summary>
        /// Align the trailing edges of vertically stacked items
        /// or the bottom edges of horizontally stacked items tightly to the relevant
        /// edge of the container
        /// </summary>
        Trailing = 4,

        /// <summary>
        /// Align the trailing edges of vertically stacked items
        /// or the bottom edges of horizontally stacked items tightly to the relevant
        /// edge of the container
        /// </summary>
        Bottom = Trailing
    }
}
