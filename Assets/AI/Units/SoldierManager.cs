using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierManager : MonoBehaviour, IGoap
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
		worldData.Add(new KeyValuePair<string, bool>("hasScout", Scout() != null));
		worldData.Add(new KeyValuePair<string, bool>("hasRecruit", transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().manpower >= 500));
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
		//.Log("<color=blue>Actions completed</color>");
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

		//Debug.Log(transform.gameObject.GetComponent<HexUnit>().Location.coordinates);
		//Debug.Log(nextAction.target.coordinates);

		// move towards the NextAction's target
		grid.FindPath(transform.gameObject.GetComponent<HexUnit>().Location, nextAction.target, transform.gameObject.GetComponent<HexUnit>());


		if (grid.GetPath() == null)
			return false;

		// we are at the target location, we are done
		transform.GetComponent<HexUnit>().Travel(grid.GetPath());

		nextAction.setInRange(true);
		transform.GetComponent<HexUnit>().Actions--;

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

	HexCell Scout()
    {
		transform.gameObject.GetComponent<HexUnit>().visibile = Grid.GetVisibleCells(transform.gameObject.GetComponent<HexUnit>().Location, transform.gameObject.GetComponent<HexUnit>().VisionRange);

		bool found = false;

		HexCell cellTo = null;

		while (!found)
        {
			cellTo = transform.gameObject.GetComponent<HexUnit>().visibile[Random.Range(0, transform.gameObject.GetComponent<HexUnit>().visibile.Count - 1)];

			if (cellTo.owner == null || HasCityNearby(cellTo))
				found = true;
		}

		return cellTo;	
	}

	bool HasCityNearby(HexCell cell)
	{
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			if (cell.GetNeighbor(d) != null && cell.owner == null && cell.SpecialIndex == 1)
				return true;
		}

		return false;
	}
	
}
