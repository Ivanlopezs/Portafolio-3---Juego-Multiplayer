using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerHand
{
    [SerializeField]
    private List<DominoTile> tiles = new List<DominoTile>();
    public List<DominoTile> Tiles => tiles;
    public int TileCount => tiles.Count;

    public void AddTile(DominoTile tile)
    {
        if (tile == null)
            return;

        tiles.Add(tile);
    }

    public bool RemoveTile(DominoTile tile)
    {
        if (tile == null)
            return false;

        return tiles.Remove(tile);
    }

    public bool HasTiles()
    {
        return tiles.Count > 0;
    }

    public void ClearHand()
    {
        tiles.Clear();
    }
}