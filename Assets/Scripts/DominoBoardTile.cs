using System;

[Serializable]
public class DominoBoardTile
{
    public DominoTile Tile { get; private set; }

    public bool ReverseVisual { get; private set; }

    public DominoBoardTile(DominoTile tile, bool reverseVisual)
    {
        Tile = tile;
        ReverseVisual = reverseVisual;
    }
}
