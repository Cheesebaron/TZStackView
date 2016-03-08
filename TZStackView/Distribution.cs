namespace TZStackView
{
    public enum Distribution : byte
    {
        Fill = 1,
        FillEqualy = 1 << 1,
        FillProportionally = 1 << 2,
        EqualSpacing = 1 << 3,
        EqualCentering = 1 << 4
    }
}
