using System.Collections.Generic;
using UnityEngine;

public class DominoGameSetup : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField, Range(2, 4)] private int playerCount = 2;

    [Header("References")]
    [SerializeField] private DominoTileFactory tileFactory;

    private List<PlayerData> players = new List<PlayerData>();
    private List<DominoTile> reserve = new List<DominoTile>();
    private DominoDealer dealer = new DominoDealer();

    public List<PlayerData> Players => players;
    public List<DominoTile> Reserve => reserve;

    
    public void InitializeGame()
    {
        playerCount = GameSettings.PlayerCount;
        InitializeGameData();
    }

    public void InitializeOnlineGame(int onlinePlayerCount)
    {
        if (onlinePlayerCount < 2 || onlinePlayerCount > 4)
        {
            Debug.LogError("La cantidad de jugadores online debe estar entre 2 y 4.");
            return;
        }

        playerCount = onlinePlayerCount;
        InitializeGameData();
    }

    private void InitializeGameData()
    {
        List<DominoTile> allTiles = tileFactory.GenerateTiles();

        if (allTiles.Count != 28)
        {
            Debug.LogError("No se generaron las 28 fichas necesarias.");
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
            players.Add(new PlayerData(i, "Jugador " + (i + 1)));
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
        Debug.Log("========== CONFIGURACIÓN DE PARTIDA ==========");

        foreach (PlayerData player in players)
        {
            Debug.Log(
                player.playerName +
                " recibió " +
                player.hand.TileCount +
                " fichas."
            );
        }

        Debug.Log("Fichas restantes en la reserva: " + reserve.Count);
        Debug.Log("==============================================");
    }

    public void PrepareOnlinePlayers(int onlinePlayerCount)
    {
        if (onlinePlayerCount < 2 || onlinePlayerCount > 4)
        {
            Debug.LogError("La cantidad de jugadores online debe estar entre 2 y 4.");
            return;
        }

        playerCount = onlinePlayerCount;
        CreatePlayers();
        reserve.Clear();

        Debug.Log("Datos de jugadores online preparados.");
    }

    public void SetOnlinePlayerHand(int playerIndex, List<DominoTile> receivedTiles)
    {
        if (playerIndex < 0 || playerIndex >= players.Count)
        {
            Debug.LogError("Índice de jugador inválido.");
            return;
        }

        PlayerHand hand = players[playerIndex].hand;
        hand.ClearHand();

        foreach (DominoTile tile in receivedTiles)
        {
            hand.AddTile(tile);
        }

        Debug.Log("Mano recibida para " + players[playerIndex].playerName
                + ". Cantidad de fichas: " + hand.TileCount);
    }

    [ContextMenu("Initialize Game")]
    private void TestInitializeGame()
    {
        InitializeGame();
    }
}
