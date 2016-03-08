namespace TZStackView
{
    public enum Alignment : byte
    {
        Fill = 1,
        Center = 1 << 1,
        Leading = 1 << 2,
        Top = 1 << 3,
        Trailing = 1 << 4,
        Bottom = 1 << 5,
        FirstBaseline = 1 << 6
    }
}
