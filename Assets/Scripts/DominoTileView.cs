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

    public void Setup(DominoTile tile, OrganismData leftData, OrganismData rightData)
    {
        leftOrganismImage.sprite = leftData.organismSprite;
        leftOrganismName.text = leftData.organismName;

        rightOrganismImage.sprite = rightData.organismSprite;
        rightOrganismName.text = rightData.organismName;
    }
}
