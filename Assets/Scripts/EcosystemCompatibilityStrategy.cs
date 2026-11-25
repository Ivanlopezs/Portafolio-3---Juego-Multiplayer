using System.Collections.Generic;

public class EcosystemCompatibilityStrategy : IOrganismCompatibilityStrategy
{
    private Dictionary<OrganismType, HashSet<OrganismType>> compatibilityRules;

    public EcosystemCompatibilityStrategy()
    {
        compatibilityRules = new Dictionary<OrganismType, HashSet<OrganismType>>();

        foreach (OrganismType organism in System.Enum.GetValues(typeof(OrganismType)))
        {
            compatibilityRules.Add(organism, new HashSet<OrganismType>());
        }

        AddRelationship(OrganismType.Grass, OrganismType.Grasshopper);
        AddRelationship(OrganismType.Grass, OrganismType.Rabbit);
        AddRelationship(OrganismType.Grass, OrganismType.Mouse);
        AddRelationship(OrganismType.Grasshopper, OrganismType.Frog);
        AddRelationship(OrganismType.Grasshopper, OrganismType.Eagle);
        AddRelationship(OrganismType.Rabbit, OrganismType.Snake);
        AddRelationship(OrganismType.Rabbit, OrganismType.Eagle);
        AddRelationship(OrganismType.Mouse, OrganismType.Snake);
        AddRelationship(OrganismType.Mouse, OrganismType.Eagle);
        AddRelationship(OrganismType.Frog, OrganismType.Snake);
        AddRelationship(OrganismType.Frog, OrganismType.Eagle);
        AddRelationship(OrganismType.Snake, OrganismType.Eagle);
    }

    private void AddRelationship(OrganismType first, OrganismType second)
    {
        compatibilityRules[first].Add(second);
        compatibilityRules[second].Add(first);
    }

    public bool CanConnect(OrganismType firstOrganism, OrganismType secondOrganism)
    {
        if (!compatibilityRules.ContainsKey(firstOrganism) || !compatibilityRules.ContainsKey(secondOrganism))
        {
            return false;
        }

        return compatibilityRules[firstOrganism].Contains(secondOrganism);
    }

    public string GetExplanation(OrganismType firstOrganism, OrganismType secondOrganism)
    {
        if (CanConnect(firstOrganism, secondOrganism))
        {
            return GetValidRelationshipExplanation(firstOrganism, secondOrganism);
        }

        if (firstOrganism == OrganismType.Grass && secondOrganism == OrganismType.Frog || firstOrganism == OrganismType.Frog && secondOrganism == OrganismType.Grass)
        {
            return "Conexión inválida: la rana se alimenta " +
                   "de insectos, no directamente de hierba.";
        }

        return "Conexión inválida: no existe una relación " +
               "alimenticia directa entre estos organismos.";
    }

    private string GetValidRelationshipExplanation(OrganismType first, OrganismType second)
    {
        if ((first == OrganismType.Grass && second == OrganismType.Grasshopper) || (first == OrganismType.Grasshopper && second == OrganismType.Grass))
        {
            return "Conexión válida: el saltamontes se alimenta de hierba.";
        }

        if ((first == OrganismType.Grass && second == OrganismType.Rabbit) || (first == OrganismType.Rabbit && second == OrganismType.Grass))
        {
            return "Conexión válida: el conejo se alimenta de hierba.";
        }

        if ((first == OrganismType.Grass && second == OrganismType.Mouse) || (first == OrganismType.Mouse && second == OrganismType.Grass))
        {
            return "Conexión válida: el ratón puede alimentarse de materia vegetal.";
        }

        if ((first == OrganismType.Grasshopper && second == OrganismType.Frog) || (first == OrganismType.Frog && second == OrganismType.Grasshopper))
        {
            return "Conexión válida: la rana se alimenta de insectos.";
        }

        if ((first == OrganismType.Grasshopper && second == OrganismType.Eagle) || (first == OrganismType.Eagle && second == OrganismType.Grasshopper))
        {
            return "Conexión válida: el águila puede alimentarse del saltamontes.";
        }

        if ((first == OrganismType.Rabbit && second == OrganismType.Snake) || (first == OrganismType.Snake && second == OrganismType.Rabbit))
        {
            return "Conexión válida: la serpiente puede alimentarse del conejo.";
        }

        if ((first == OrganismType.Rabbit && second == OrganismType.Eagle) || (first == OrganismType.Eagle && second == OrganismType.Rabbit))
        {
            return "Conexión válida: el águila puede alimentarse del conejo.";
        }

        if ((first == OrganismType.Mouse && second == OrganismType.Snake) || (first == OrganismType.Snake && second == OrganismType.Mouse))
        {
            return "Conexión válida: la serpiente puede alimentarse del ratón.";
        }

        if ((first == OrganismType.Mouse && second == OrganismType.Eagle) || (first == OrganismType.Eagle && second == OrganismType.Mouse))
        {
            return "Conexión válida: el águila puede alimentarse del ratón.";
        }

        if ((first == OrganismType.Frog && second == OrganismType.Snake) || (first == OrganismType.Snake && second == OrganismType.Frog))
        {
            return "Conexión válida: la serpiente puede alimentarse de la rana.";
        }

        if ((first == OrganismType.Frog && second == OrganismType.Eagle) || (first == OrganismType.Eagle && second == OrganismType.Frog))
        {
            return "Conexión válida: el águila puede alimentarse de la rana.";
        }

        if ((first == OrganismType.Snake && second == OrganismType.Eagle) || (first == OrganismType.Eagle && second == OrganismType.Snake))
        {
            return "Conexión válida: el águila puede alimentarse de la serpiente.";
        }

        return "Conexión válida.";
    }
}

