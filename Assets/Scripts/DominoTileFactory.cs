using System.Collections.Generic;
using UnityEngine;

public class DominoTileFactory : MonoBehaviour
{
    [Header("Organisms")]
    [SerializeField] private OrganismData[] organisms;

    [Header("Visual Preview")]
    [SerializeField] private DominoTileView tilePrefab;
    [SerializeField] private Transform previewParent;

    public List<DominoTile> GenerateTiles()
    {
        List<DominoTile> tiles = new List<DominoTile>();

        if (!ValidateOrganisms())
            return tiles;

        int tileID = 0;

        for (int i = 0; i < organisms.Length; i++)
        {
            for (int j = i; j < organisms.Length; j++)
            {
                DominoTile tile = new DominoTile(tileID, organisms[i].organismType, organisms[j].organismType);
                tiles.Add(tile);
                tileID++;
            }
        }

        Debug.Log("Total de fichas generadas: " + tiles.Count);
        return tiles;
    }

    private bool ValidateOrganisms()
    {
        int expectedCount = System.Enum.GetValues(
            typeof(OrganismType)).Length;

        if (organisms == null || organisms.Length != expectedCount)
        {
            Debug.LogError("Debes asignar exactamente siete organismos.");
            return false;
        }

        HashSet<OrganismType> registeredOrganisms = new HashSet<OrganismType>();

        foreach (OrganismData organism in organisms)
        {
            if (organism == null)
            {
                Debug.LogError("Existe un organismo sin asignar.");
                return false;
            }

            if (!registeredOrganisms.Add(organism.organismType))
            {
                Debug.LogError("Existe un organismo duplicado: " + organism.organismType);
                return false;
            }

            if (organism.organismSprite == null)
            {
                Debug.LogError("Falta la imagen de: " + organism.organismName);
                return false;
            }
        }

        return true;
    }

    private OrganismData FindOrganismData(OrganismType type)
    {
        foreach (OrganismData organism in organisms)
        {
            if (organism.organismType == type)
                return organism;
        }

        return null;
    }


    public OrganismData GetOrganismData(OrganismType type)
    {
        return FindOrganismData(type);
    }


    [ContextMenu("Generate Tile Preview")]
    public void GeneratePreview()
    {
        if (tilePrefab == null || previewParent == null)
        {
            Debug.LogError("Asigna el prefab y el contenedor de las fichas.");
            return;
        }

        List<DominoTile> tiles = GenerateTiles();

        if (tiles.Count == 0)
            return;

        foreach (DominoTile tile in tiles)
        {
            OrganismData leftData = FindOrganismData(tile.leftOrganism);

            OrganismData rightData = FindOrganismData(tile.rightOrganism);

            DominoTileView newTile = Instantiate(tilePrefab, previewParent);

            newTile.Setup(tile, leftData, rightData);
        }

        Debug.Log("Vista previa de las 28 fichas creada.");
    }
}
