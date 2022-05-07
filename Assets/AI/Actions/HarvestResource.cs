using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestResource : GoapAction
{
    private bool done = false;
    public HarvestResource()
    {
        addPrecondition("hasResource", true);
        addEffect("ConstructionSpending", true);
    }


    public override void reset()
    {
        done = false;
        target = null;
    }

    public override bool isDone()
    {
        return done;
    }

    public override bool requiresInRange()
    {
        return true;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        target = FindClosestResource(agent.GetComponent<HexUnit>());

        if (target != null)
            return true;
        else
            return false;
    }

    public override bool perform(GameObject agent)
    {
        if (agent.GetComponent<HexUnit>().Actions > 0)
        {
            agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().resources[agent.GetComponent<HexUnit>().Location.ResourceIndex]++;
            agent.GetComponent<HexUnit>().Actions--;
            done = true;
            return true;
        }
        else
            return false;
    }

    HexCell FindClosestResource(HexUnit unit)
    {
        float minDistance = Mathf.Infinity;

        HexCell optimalCell = null;

        foreach (HexCell cell in unit.ParentEmpire.GetComponent<Empire>().exploredCells)
        {
            if (cell.IsResource)
            {
                unit.gameObject.GetComponent<GoapAgent>().map.FindPath(unit.Location, cell, unit);

                if (unit.gameObject.GetComponent<GoapAgent>().map.HasPath && minDistance > unit.Location.coordinates.DistanceTo(cell.coordinates))
                {
                    minDistance = unit.Location.coordinates.DistanceTo(cell.coordinates);
                    optimalCell = cell;
                }
            }
        }

        return optimalCell;
    }

}
