public interface IOrganismCompatibilityStrategy
{
    bool CanConnect(OrganismType firstOrganism, OrganismType secondOrganism);
    string GetExplanation(OrganismType firstOrganism, OrganismType secondOrganism);
}

