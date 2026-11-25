public class DominoMoveResult
{
    public bool IsValid { get; private set; }
    public bool ConnectsUsingLeftSide { get; private set; }
    public OrganismType NewOpenEnd { get; private set; }
    public string Explanation { get; private set; }

    public DominoMoveResult(bool isValid, bool connectsUsingLeftSide, OrganismType newOpenEnd, string explanation)
    {
        IsValid = isValid;
        ConnectsUsingLeftSide = connectsUsingLeftSide;
        NewOpenEnd = newOpenEnd;
        Explanation = explanation;
    }
}
