using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour, IGoap
{
	public int Enemies;
	public int Foreigners;

	Map Grid;

	/**
	 * Key-Value data that will feed the GOAP actions and system while planning.
	 */
	public HashSet<KeyValuePair<string, bool>> getWorldState(Map map)
	{
		HashSet<KeyValuePair<string, bool>> worldData = new HashSet<KeyValuePair<string, bool>>();

		Grid = map;

		checkSurroundings(Grid);

		worldData.Add(new KeyValuePair<string, bool>("hasEnemies", (Enemies > 0)));
		worldData.Add(new KeyValuePair<string, bool>("hasForeigners", (Foreigners > 0)));
		worldData.Add(new KeyValuePair<string, bool>("hasBuild", hasBuild()));
		worldData.Add(new KeyValuePair<string, bool>("hasResource", hasResource()));
		worldData.Add(new KeyValuePair<string, bool>("OnEnemyTerritory", OnEnemyTerritory()));

		return worldData;
	}

	public void planFailed(HashSet<KeyValuePair<string, bool>> failedGoal)
	{
		Debug.Log("SHIT");
	}

	public void planFound(HashSet<KeyValuePair<string, bool>> goal, Queue<GoapAction> actions)
	{
		// Yay we found a plan for our goal
		Debug.Log("<color=green>Plan found</color> " + GoapAgent.prettyPrint(actions));
	}

	public void actionsFinished()
	{
		// Everything is done, we completed our actions for this gool. Hooray!
		Debug.Log("<color=blue>Actions completed</color>");
	}

	public void planAborted(GoapAction aborter)
	{
		// An action bailed out of the plan. State has been reset to plan again.
		// Take note of what happened and make sure if you run the same goal again
		// that it can succeed.
		Debug.Log("<color=red>Plan Aborted</color> " + GoapAgent.prettyPrint(aborter));
	}

	public bool moveAgent(GoapAction nextAction, Map grid)
	{
		if (transform.GetComponent<HexUnit>().Actions <= 0 || nextAction.target == null)
			return false;

		if (transform.GetComponent<HexUnit>().Location == nextAction.target)
        {
			nextAction.setInRange(true);
			return true;
		}

		// move towards the NextAction's target
		grid.FindPath(transform.gameObject.GetComponent<HexUnit>().Location, nextAction.target, transform.gameObject.GetComponent<HexUnit>());

		if (grid.GetPath() == null)
			return false;

		// we are at the target location, we are done
		transform.GetComponent<HexUnit>().Travel(grid.GetPath());
		nextAction.setInRange(true);
		return true;

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
					if (!transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AlliedWith.Contains(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName))
						Foreigners++;
				}
				
			}
		}
	}

	bool OnEnemyTerritory()
	{
		if (transform.gameObject.GetComponent<HexUnit>().Location.owner == null)
			return false;
		else
		{
			if (transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AtWar.Contains(transform.gameObject.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().empireName))
				return true;
			else
				return false;
		}
	}

	bool hasBuild()
    {
		if (FindClosestEmpireCell(transform.gameObject.GetComponent<HexUnit>()))
		{
			return true;
		}
		else
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

	bool hasResource()
    {
		if (FindClosestResource(transform.gameObject.GetComponent<HexUnit>()) != null)
			return true;
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
				if (minDistance > unit.Location.coordinates.DistanceTo(cell.coordinates))
                {
					minDistance = unit.Location.coordinates.DistanceTo(cell.coordinates);
					optimalCell = cell;				
				}
            }
        }

		return optimalCell;
    }
}
