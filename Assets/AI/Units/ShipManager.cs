using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour , IGoap
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
		worldData.Add(new KeyValuePair<string, bool>("hasColonies", (hasColonies())));
		worldData.Add(new KeyValuePair<string, bool>("OnEnemyTerritory", (OnEnemyTerritory())));
		worldData.Add(new KeyValuePair<string, bool>("hasUnitsOnBoard", (transform.GetComponent<HexUnit>().currentSoldiersOnBoard > 0 || transform.GetComponent<HexUnit>().currentWorkersOnBoard > 0)));

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

		// move towards the NextAction's target
		grid.FindPath(transform.GetComponent<HexUnit>().Location, nextAction.target, transform.GetComponent<HexUnit>());

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
		if (transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AtWar.Contains(transform.gameObject.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().empireName))
			return true;
		else
			return false;
	}

	bool hasColonies()
    {
		foreach (HexCell city in Grid.cities)
        {
			if (city.owner == null && FindCoast(city) != null)
            {
				return true;
			}
				
        }

		return false;
    }

	HexCell FindCoast(HexCell cell)
	{
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			if (cell.GetNeighbor(d).IsUnderwater)
				return cell.GetNeighbor(d);
		}

		return null;
	}

}
