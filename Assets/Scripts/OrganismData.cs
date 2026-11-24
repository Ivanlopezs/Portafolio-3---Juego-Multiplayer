using UnityEngine;

[CreateAssetMenu(fileName = "NewOrganism", menuName = "Domino/Ecosystem/Organism")]
public class OrganismData : ScriptableObject
{
    [Header("Organism Information")]
    public OrganismType organismType;
    public string organismName;
    
    [TextArea(2, 4)]
    public string description;

    [Header("Visual")]
    public Sprite organismSprite;
}
