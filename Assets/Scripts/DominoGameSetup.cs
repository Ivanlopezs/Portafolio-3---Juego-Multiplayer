using System.Collections.Generic;
using UnityEngine;

public class DominoGameSetup : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField, Range(2, 4)]
    private int playerCount = 2;

    [Header("References")]
    [SerializeField]
    private DominoTileFactory tileFactory;
    private List<PlayerData> players = new List<PlayerData>();
    private List<DominoTile> reserve = new List<DominoTile>();
    private DominoDealer dealer = new DominoDealer();
    public List<PlayerData> Players => players;
    public List<DominoTile> Reserve => reserve;

    public void InitializeGame()
    {
        List<DominoTile> allTiles = tileFactory.GenerateTiles();

        if (allTiles.Count != 28)
        {
            Debug.LogError("No se pudo preparar la partida. " + "Se necesitan exactamente 28 fichas.");
            return;
        }

        CreatePlayers();

        int tilesPerPlayer = GetTilesPerPlayer();

        reserve = dealer.Deal(allTiles, players, tilesPerPlayer);

        PrintGameSetup();
    }

    private void CreatePlayers()
    {
        players.Clear();

        for (int i = 0; i < playerCount; i++)
        {
            PlayerData player = new PlayerData(i, "Jugador " + (i + 1));
            players.Add(player);
        }
    }

    private int GetTilesPerPlayer()
    {
        if (playerCount == 2)
            return 7;

        return 5;
    }

    private void PrintGameSetup()
    {
        Debug.Log("========== PARTIDA PREPARADA ==========");
        Debug.Log("Cantidad de jugadores: " + players.Count);

        foreach (PlayerData player in players)
        {
            Debug.Log(player.playerName + " tiene " + player.hand.TileCount + " fichas.");

            foreach (DominoTile tile in player.hand.Tiles)
            {
                Debug.Log("Ficha ID: " + tile.tileID + " | " + tile.leftOrganism + " - " + tile.rightOrganism);
            }
        }

        Debug.Log("Fichas en la reserva: " + reserve.Count);
        Debug.Log("=======================================");
    }

    [ContextMenu("Initialize Game")]
    private void TestInitializeGame()
    {
        InitializeGame();
    }
}
