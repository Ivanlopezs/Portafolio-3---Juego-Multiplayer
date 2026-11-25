using System.Collections.Generic;
using UnityEngine;

public class DominoBoardManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DominoTileFactory tileFactory;
    [SerializeField] private DominoTileView tilePrefab;
    [SerializeField] private Transform boardTilesContainer;

    [Header("Testing")]
    [SerializeField] private int testTileID = 1;

    private DominoBoard board = new DominoBoard();
    private DominoRules rules;

    private List<DominoTile> testTiles = new List<DominoTile>();
    private HashSet<int> usedTileIDs = new HashSet<int>();
    public DominoBoard Board => board;

    private void Awake()
    {
        IOrganismCompatibilityStrategy strategy = new EcosystemCompatibilityStrategy();
        rules = new DominoRules(strategy);
    }

    public void InitializeBoard()
    {
        board.ClearBoard();
        usedTileIDs.Clear();
        testTiles = tileFactory.GenerateTiles();
        RefreshBoardVisuals();
        Debug.Log("Tablero preparado correctamente.");
    }

    public DominoBoardMoveResult TryPlaceTile(DominoTile tile)
    {
        if (tile == null)
        {
            return new DominoBoardMoveResult(false, false, false, "La ficha seleccionada no existe.");
        }

        if (usedTileIDs.Contains(tile.tileID))
        {
            return new DominoBoardMoveResult(false, false, false, "Esta ficha ya fue colocada en el tablero.");
        }

        DominoBoardMoveResult result = board.TryPlaceTile(tile, rules);

        if (!result.IsValid)
        {
            Debug.Log("Jugada rechazada: " + result.Explanation);
            return result;
        }

        usedTileIDs.Add(tile.tileID);
        RefreshBoardVisuals();
        Debug.Log("Jugada aceptada: " + result.Explanation);
        Debug.Log("Fichas en el tablero: " + board.TileCount);
        Debug.Log("Extremo izquierdo: " + board.LeftOpenEnd);
        Debug.Log("Extremo derecho: " + board.RightOpenEnd);

        return result;
    }

    private void RefreshBoardVisuals()
    {

        foreach (Transform child in boardTilesContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (DominoBoardTile boardTile in board.PlacedTiles)
        {
            DominoTile tile = boardTile.Tile;
            OrganismData leftData = tileFactory.GetOrganismData(tile.leftOrganism);
            OrganismData rightData = tileFactory.GetOrganismData(tile.rightOrganism);
            DominoTileView newView = Instantiate(tilePrefab, boardTilesContainer);
            newView.Setup(tile, leftData, rightData, boardTile.ReverseVisual);
        }
    }

    [ContextMenu("Initialize Test Board")]
    private void TestInitializeBoard()
    {
        InitializeBoard();
    }

    [ContextMenu("Place Selected Test Tile")]
    private void TestPlaceTile()
    {
        DominoTile selectedTile = testTiles.Find(tile => tile.tileID == testTileID);

        if (selectedTile == null)
        {
            Debug.LogError("No se encontró la ficha con ID: " + testTileID + ". Primero inicializa el tablero.");
            return;
        }

        TryPlaceTile(selectedTile);
    }
}
