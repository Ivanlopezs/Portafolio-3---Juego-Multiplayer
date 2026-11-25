public class DominoBoardMoveResult
{
    public bool IsValid { get; private set; }
    public bool PlacedAtLeft { get; private set; }
    public bool ReverseVisual { get; private set; }
    public string Explanation { get; private set; }

    public DominoBoardMoveResult(bool isValid, bool placedAtLeft, bool reverseVisual, string explanation)
    {
        IsValid = isValid;
        PlacedAtLeft = placedAtLeft;
        ReverseVisual = reverseVisual;
        Explanation = explanation;
    }
}
