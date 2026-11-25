using System.Collections.Generic;

public class DominoBoard
{
    private List<DominoBoardTile> placedTiles = new List<DominoBoardTile>();
    private OrganismType leftOpenEnd;
    private OrganismType rightOpenEnd;
    public List<DominoBoardTile> PlacedTiles => placedTiles;
    public int TileCount => placedTiles.Count;
    public OrganismType LeftOpenEnd => leftOpenEnd;
    public OrganismType RightOpenEnd => rightOpenEnd;
    public bool IsEmpty => placedTiles.Count == 0;

    public DominoBoardMoveResult TryPlaceTile(DominoTile tile, DominoRules rules)
    {
        if (tile == null)
        {
            return new DominoBoardMoveResult(false, false, false, "No se ha seleccionado ninguna ficha.");
        }

        if (rules == null)
        {
            return new DominoBoardMoveResult(false, false, false, "No se han configurado las reglas del tablero.");
        }

        if (IsEmpty)
        {
            placedTiles.Add(new DominoBoardTile(tile, false));

            leftOpenEnd = tile.leftOrganism;
            rightOpenEnd = tile.rightOrganism;

            return new DominoBoardMoveResult(true, false, false, "Primera ficha colocada correctamente.");
        }

        DominoMoveResult rightResult = rules.ValidateMove(tile, rightOpenEnd);

        if (rightResult.IsValid)
        {
            bool reverseVisual = !rightResult.ConnectsUsingLeftSide;
            placedTiles.Add(new DominoBoardTile(tile, reverseVisual));
            rightOpenEnd = rightResult.NewOpenEnd;
            return new DominoBoardMoveResult(true, false, reverseVisual, rightResult.Explanation);
        }

        DominoMoveResult leftResult = rules.ValidateMove(tile, leftOpenEnd);

        if (leftResult.IsValid)
        {
            bool reverseVisual = leftResult.ConnectsUsingLeftSide;
            placedTiles.Insert(0, new DominoBoardTile(tile, reverseVisual));
            leftOpenEnd = leftResult.NewOpenEnd;
            return new DominoBoardMoveResult(true, true, reverseVisual, leftResult.Explanation);
        }

        return new DominoBoardMoveResult(false, false, false, "La ficha no puede conectarse con ninguno de los extremos del tablero.");
    }

    public void ClearBoard()
    {
        placedTiles.Clear();
        leftOpenEnd = default;
        rightOpenEnd = default;
    }
}
