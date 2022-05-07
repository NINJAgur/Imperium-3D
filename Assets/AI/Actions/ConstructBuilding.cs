using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructBuilding : GoapAction
{
    private bool done = false;

    public int Enemies;
    public int Friendlies;
    public int Foreigners;
    public int Allies;

    public ConstructBuilding()
    {
        addPrecondition("hasBuild", true);
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
        target = FindClosestEmpireCell(agent.GetComponent<HexUnit>());

        if (target != null)
            return true;
        else
            return false;
    }

    public override bool perform(GameObject agent)
    {
        checkSurroundings(agent.GetComponent<GoapAgent>().map);

        if (agent.GetComponent<HexUnit>().Actions > 0)
        {
            Empire currentEmpire = agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>();

            if ((Enemies + Foreigners > Allies + Friendlies) && currentEmpire.manpower >= 500)
            {
                agent.GetComponent<HexUnit>().Location.PlantLevel = 0;
                agent.GetComponent<HexUnit>().Location.Walled = true;
                agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().manpower -= 500;
                agent.GetComponent<HexUnit>().Actions--;
                done = true;
                return true;
            }

            if (10 * currentEmpire.empireCells.Count + currentEmpire.Urban.Count * 50 < currentEmpire.units.Count * 1000 && currentEmpire.manpower >= 1500)
            {
                agent.GetComponent<HexUnit>().Location.UrbanLevel = 2;
                agent.GetComponent<HexUnit>().Location.PlantLevel = 1;
                agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().Urban.Add(agent.GetComponent<HexUnit>().Location);
                agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().manpower -= 1500;
                agent.GetComponent<HexUnit>().Actions--;
                done = true;
                return true;
            }

            if (10 * currentEmpire.empireCells.Count + currentEmpire.farms.Count * 50 < currentEmpire.units.Count * 1000 && currentEmpire.manpower >= 1500)
            {
                agent.GetComponent<HexUnit>().Location.FarmLevel = 1;
                agent.GetComponent<HexUnit>().Location.PlantLevel = 1;
                agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().farms.Add(agent.GetComponent<HexUnit>().Location);
                agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().manpower -= 1000;
                agent.GetComponent<HexUnit>().Actions--;
                done = true;
                return true;
            }

            if ((int)(currentEmpire.manpower + currentEmpire.treasury - currentEmpire.empireCells.Count * 1000) / 1000 > 5 && currentEmpire.manpower >= 3000)
            {
                agent.GetComponent<HexUnit>().Location.SpecialIndex = 2;
                agent.GetComponent<HexUnit>().Location.PlantLevel = 0;
                agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().Temples.Add(agent.GetComponent<HexUnit>().Location);
                agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().manpower -= 3000;
                agent.GetComponent<HexUnit>().Actions--;
                done = true;
                return true;
            }

        }
        return false;
    }

    HexCell FindClosestEmpireCell(HexUnit unit)
    {
        float minDis = Mathf.Infinity;

        HexCell closest = null;

        foreach (HexCell cell in unit.ParentEmpire.GetComponent<Empire>().empireCells)
        {
            if (cell.Unit == null && !cell.IsUnderwater && !unit.ParentEmpire.GetComponent<Empire>().farms.Contains(cell) && !unit.ParentEmpire.GetComponent<Empire>().Urban.Contains(cell)
                && !unit.ParentEmpire.GetComponent<Empire>().Cities.Contains(cell) && !unit.ParentEmpire.GetComponent<Empire>().Temples.Contains(cell))
            {
                if (unit.Location.coordinates.DistanceTo(cell.coordinates) < minDis)
                {
                    closest = cell;
                    minDis = unit.Location.coordinates.DistanceTo(cell.coordinates);
                }
            }
        }

        return closest;
    }

    void checkSurroundings(Map Grid)
    {
        List<HexCell> visible = Grid.GetVisibleCells(transform.gameObject.GetComponent<HexUnit>().Location, transform.gameObject.GetComponent<HexUnit>().VisionRange);

        foreach (HexCell cell in visible)
        {
            if (cell.Unit != null && cell.Unit != transform.gameObject.GetComponent<HexUnit>())
            {
                if (cell.Unit.ParentEmpire != transform.gameObject.GetComponent<HexUnit>().ParentEmpire)
                {
                    if (transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AtWar.Contains(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName))
                        Enemies++;
                    if (transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AlliedWith.Contains(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName))
                        Allies++;
                    else
                        Foreigners++;

                }
                else
                    Friendlies++;
            }
        }
    }
}
