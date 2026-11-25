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

    public DominoBoardMoveResult TryPlaceTile(
        DominoTile tile,
        DominoRules rules,
        DominoPlacementSide side)
    {
        if (tile == null)
        {
            return new DominoBoardMoveResult(
                false,
                false,
                false,
                "La ficha seleccionada es nula."
            );
        }

        if (rules == null)
        {
            return new DominoBoardMoveResult(
                false,
                false,
                false,
                "No se encontraron las reglas del dominó."
            );
        }

        // Si el tablero está vacío, colocar la primera ficha.
        if (IsEmpty)
        {
            placedTiles.Add(new DominoBoardTile(tile, false));

            leftOpenEnd = tile.leftOrganism;
            rightOpenEnd = tile.rightOrganism;

            return new DominoBoardMoveResult(
                true,
                false,
                false,
                "La primera ficha se colocó correctamente."
            );
        }

        // Intentar colocar la ficha en el extremo derecho.
        if (side == DominoPlacementSide.Right)
        {
            DominoMoveResult result = rules.ValidateMove(
                tile,
                rightOpenEnd
            );

            if (!result.IsValid)
            {
                return new DominoBoardMoveResult(
                    false,
                    false,
                    false,
                    result.Explanation
                );
            }

            bool reverseVisual = !result.ConnectsUsingLeftSide;

            placedTiles.Add(
                new DominoBoardTile(tile, reverseVisual)
            );

            rightOpenEnd = result.NewOpenEnd;

            return new DominoBoardMoveResult(
                true,
                false,
                reverseVisual,
                result.Explanation
            );
        }

        // Intentar colocar la ficha en el extremo izquierdo.
        if (side == DominoPlacementSide.Left)
        {
            DominoMoveResult result = rules.ValidateMove(
                tile,
                leftOpenEnd
            );

            if (!result.IsValid)
            {
                return new DominoBoardMoveResult(
                    false,
                    true,
                    false,
                    result.Explanation
                );
            }

            bool reverseVisual = result.ConnectsUsingLeftSide;

            placedTiles.Insert(
                0,
                new DominoBoardTile(tile, reverseVisual)
            );

            leftOpenEnd = result.NewOpenEnd;

            return new DominoBoardMoveResult(
                true,
                true,
                reverseVisual,
                result.Explanation
            );
        }

        return new DominoBoardMoveResult(
            false,
            false,
            false,
            "El lado de colocación seleccionado no es válido."
        );
    }

    public void ClearBoard()
    {
        placedTiles.Clear();

        leftOpenEnd = default;
        rightOpenEnd = default;
    }
}

