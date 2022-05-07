using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlerManager : MonoBehaviour, IGoap
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
		worldData.Add(new KeyValuePair<string, bool>("hasExpansion", (HasExpansion())));
		worldData.Add(new KeyValuePair<string, bool>("OnEnemyTerritory", (OnEnemyTerritory())));

		return worldData;
	}

	public void planFailed(HashSet<KeyValuePair<string, bool>> failedGoal)
	{
		Debug.Log("SHIT");
	}

	public void planFound(HashSet<KeyValuePair<string, bool>> goal, Queue<GoapAction> actions)
	{
		// Yay we found a plan for our goal
		//Debug.Log("<color=green>Plan found</color> " + GoapAgent.prettyPrint(actions));
	}

	public void actionsFinished()
	{
		// Everything is done, we completed our actions for this gool. Hooray!
		//Debug.Log("<color=blue>Actions completed</color>");
	}

	public void planAborted(GoapAction aborter)
	{
		// An action bailed out of the plan. State has been reset to plan again.
		// Take note of what happened and make sure if you run the same goal again
		// that it can succeed.
		//Debug.Log("<color=red>Plan Aborted</color> " + GoapAgent.prettyPrint(aborter));
	}

	public bool moveAgent(GoapAction nextAction, Map grid)
	{
		if (transform.GetComponent<HexUnit>().Actions <= 0 || nextAction.target == null)
			return false;

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

	bool HasExpansion()
	{
		if (FindBestCell(transform.gameObject.GetComponent<HexUnit>()) != null)
			return true;
		else
			return false;

	}

	HexCell FindBestCell(HexUnit unit)
	{
		if (GetClosestCity(unit) == null || GetClosestTile(unit).coordinates.DistanceTo(unit.Location.coordinates) > GetClosestCity(unit).coordinates.DistanceTo(unit.Location.coordinates))
			return GetClosestTile(unit);
		else
			return GetClosestCity(unit);
	}

	HexCell GetClosestTile(HexUnit unit)
	{
		float minDis = Mathf.Infinity;

		HexCell closest = null;

		foreach (HexCell cell in unit.ParentEmpire.GetComponent<Empire>().empireCells)
		{
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				if (cell.GetNeighbor(d) != null && cell.GetNeighbor(d).owner == null && unit.ParentEmpire.GetComponent<Empire>().exploredCells.Contains(cell.GetNeighbor(d)) && cell.Unit == null)
				{
					if (unit.Location.coordinates.DistanceTo(cell.GetNeighbor(d).coordinates) < minDis)
					{
						minDis = unit.Location.coordinates.DistanceTo(cell.GetNeighbor(d).coordinates);
						closest = cell.GetNeighbor(d);
					}
				}
			}
		}

		return closest;
	}

	HexCell GetClosestCity(HexUnit unit)
	{
		float minDis = Mathf.Infinity;

		HexCell closest = null;

		foreach (HexCell city in transform.gameObject.GetComponent<GoapAgent>().map.cities)
		{
			if (city.owner == null && unit.Location.coordinates.DistanceTo(city.coordinates) < minDis)
			{
				minDis = unit.Location.coordinates.DistanceTo(city.coordinates);
				closest = city;
			}

		}

		return closest;
	}
}
