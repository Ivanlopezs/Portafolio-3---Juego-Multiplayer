using System.Collections.Generic;
using UnityEngine;

public class DominoDealer
{
    public List<DominoTile> Deal(List<DominoTile> allTiles, List<PlayerData> players, int tilesPerPlayer)
    {
        if (allTiles == null || players == null)
        {
            Debug.LogError("La lista de fichas o jugadores es nula.");
            return new List<DominoTile>();
        }

        if (players.Count < 2 || players.Count > 4)
        {
            Debug.LogError("La partida debe tener entre 2 y 4 jugadores.");
            return new List<DominoTile>();
        }

        if (tilesPerPlayer * players.Count > allTiles.Count)
        {
            Debug.LogError("No hay suficientes fichas para repartir.");
            return new List<DominoTile>();
        }

        List<DominoTile> shuffledTiles = new List<DominoTile>(allTiles);

        Shuffle(shuffledTiles);

        foreach (PlayerData player in players)
        {
            player.hand.ClearHand();
        }

        int currentIndex = 0;

        for (int i = 0; i < tilesPerPlayer; i++)
        {
            foreach (PlayerData player in players)
            {
                player.hand.AddTile(shuffledTiles[currentIndex]);
                currentIndex++;
            }
        }

        List<DominoTile> reserve = new List<DominoTile>();

        for (int i = currentIndex; i < shuffledTiles.Count; i++)
        {
            reserve.Add(shuffledTiles[i]);
        }

        Debug.Log("Reparto completado correctamente.");
        return reserve;
    }

    private void Shuffle(List<DominoTile> tiles)
    {
        for (int i = tiles.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            DominoTile temporary = tiles[i];
            tiles[i] = tiles[randomIndex];
            tiles[randomIndex] = temporary;
        }
    }
}
