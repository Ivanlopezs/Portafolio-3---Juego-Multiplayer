public class DominoRules
{
    private IOrganismCompatibilityStrategy compatibilityStrategy;

    public DominoRules(IOrganismCompatibilityStrategy strategy)
    {
        compatibilityStrategy = strategy;
    }

    public DominoMoveResult ValidateMove(DominoTile tile, OrganismType boardOpenEnd)
    {
        if (tile == null)
        {
            return new DominoMoveResult(false, false, boardOpenEnd, "No se ha seleccionado ninguna ficha.");
        }

        if (compatibilityStrategy.CanConnect(boardOpenEnd, tile.leftOrganism))
        {
            return new DominoMoveResult(true, true, tile.rightOrganism, compatibilityStrategy.GetExplanation(boardOpenEnd, tile.leftOrganism));
        }

        if (compatibilityStrategy.CanConnect(boardOpenEnd, tile.rightOrganism))
        {
            return new DominoMoveResult(true, false, tile.leftOrganism, compatibilityStrategy.GetExplanation(boardOpenEnd, tile.rightOrganism));
        }

        return new DominoMoveResult(false, false, boardOpenEnd, "La ficha no puede conectarse con el extremo actual del tablero.");
    }
}
