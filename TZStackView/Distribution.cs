namespace TZStackView
{
    /// <summary>
    /// Distribution. The layout along the stacking axis.
    /// All <see cref="UIKit.UIStackViewDistribution"> enum values fit first and last 
    /// arranged subviews tightly to the container, except for 
    /// <see cref="UIKit.UIStackViewDistribution.FillEqually"/>, fit all items to 
    /// <see cref="UIKit.UIView.IntrinsicContentSize"/> when possible.
    /// </summary>
    public enum Distribution
    {
        /// <summary>
        /// When items do not fit (overflow) or fill (underflow) the space available
        /// adjustments occur according to CompressionResistance or hugging
        /// priorities of items, or when that is ambiguous, according to arrangement order.
        /// </summary>
        Fill = 0,

        /// <summary>
        /// Items are all the same size.
        /// When space allows, this will be the size of the item with the largest
        /// <see cref="UIKit.UIView.IntrinsicContentSize"/> (along the axis of the stack).
        /// Overflow or underflow adjustments are distributed equally among the items.
        /// </summary>
        FillEqualy = 1,

        /// <summary>
        /// Overflow or underflow adjustments are distributed among the items proportional
        /// to their <see cref="UIKit.UIView.IntrinsicContentSize"/>.
        /// </summary>
        FillProportionally = 2,

        /// <summary>
        /// Additional underflow spacing is divided equally in the spaces between the items.
        /// Overflow squeezing is controlled by compressionResistance priorities followed by
        /// arrangement order.
        /// </summary>
        EqualSpacing = 3,

        /// <summary>
        /// Equal center-to-center spacing of the items is maintained as much
        /// as possible while still maintaining a minimum edge-to-edge spacing within the
        /// allowed area.
        /// Additional underflow spacing is divided equally in the spacing. Overflow
        /// squeezing is distributed first according to CompressionResistance priorities
        /// of items, then according to subview order while maintaining the configured
        /// (edge-to-edge) spacing as a minimum.
        /// </summary>
        EqualCentering = 4
    }
}
