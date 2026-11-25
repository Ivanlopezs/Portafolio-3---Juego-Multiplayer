using System.Collections.Generic;
using UnityEngine;

public class PlayerHandUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform handContainer;
    [SerializeField] private DominoTileView tilePrefab;
    [SerializeField] private DominoTileFactory tileFactory;
    [SerializeField] private DominoTurnManager turnManager;

    private List<DominoTileView> displayedTiles = new List<DominoTileView>();

    public void ShowHand(PlayerHand hand)
    {
        ClearHand();

        if (hand == null)
        {
            Debug.LogError("No se recibió una mano válida.");
            return;
        }

        foreach (DominoTile tile in hand.Tiles)
        {
            OrganismData leftData = tileFactory.GetOrganismData(tile.leftOrganism);

            OrganismData rightData = tileFactory.GetOrganismData(tile.rightOrganism);

            if (leftData == null || rightData == null)
            {
                Debug.LogError(
                    "No se encontraron los datos de los organismos."
                );

                continue;
            }

            DominoTileView tileView = Instantiate(tilePrefab, handContainer);
            tileView.Setup(tile, leftData, rightData);

            tileView.SetClickAction(
                selectedTile => turnManager.SelectTile(selectedTile)
            );

            displayedTiles.Add(tileView);
        }
    }

    public void ClearHand()
    {
        foreach (DominoTileView tileView in displayedTiles)
        {
            if (tileView != null)
                Destroy(tileView.gameObject);
        }

        displayedTiles.Clear();
    }
}
