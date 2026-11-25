using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DominoTileView : MonoBehaviour
{
    [Header("Left Side")]
    [SerializeField] private Image leftOrganismImage;
    [SerializeField] private TMP_Text leftOrganismName;

    [Header("Right Side")]
    [SerializeField] private Image rightOrganismImage;
    [SerializeField] private TMP_Text rightOrganismName;

    [Header("Interaction")]
    [SerializeField] private Button tileButton;

    public DominoTile TileData { get; private set; }

    public void Setup(DominoTile tile, OrganismData leftData, OrganismData rightData, bool reverseVisual = false)
    {
        TileData = tile;

        if (tile == null || leftData == null || rightData == null)
        {
            Debug.LogError("No se pudo configurar la vista de la ficha.");
            return;
        }

        if (reverseVisual)
        {
            leftOrganismImage.sprite = rightData.organismSprite;
            leftOrganismName.text = rightData.organismName;

            rightOrganismImage.sprite = leftData.organismSprite;
            rightOrganismName.text = leftData.organismName;
        }
        else
        {
            leftOrganismImage.sprite = leftData.organismSprite;
            leftOrganismName.text = leftData.organismName;

            rightOrganismImage.sprite = rightData.organismSprite;
            rightOrganismName.text = rightData.organismName;
        }
    }

    public void SetClickAction(Action<DominoTile> onClick)
    {
        if (tileButton == null)
        {
            Debug.LogError("No se asignó el Button de la ficha.");
            return;
        }

        tileButton.onClick.RemoveAllListeners();

        tileButton.onClick.AddListener(() =>
        {
            onClick?.Invoke(TileData);
        });
}
}
