using System;

[Serializable]
public class DominoTile
{
    public int tileID;

    public OrganismType leftOrganism;
    public OrganismType rightOrganism;

    public DominoTile(int id, OrganismType left, OrganismType right)
    {
        tileID = id;
        leftOrganism = left;
        rightOrganism = right;
    }

    public bool IsDouble()
    {
        return leftOrganism == rightOrganism;
    }
}
