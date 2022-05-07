using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expand : GoapAction
{
	private bool done = false;

	public Expand()
	{
		addPrecondition("hasExpansion", true); 
		addEffect("Expansionism", true);
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
		target = FindBestCell(transform.gameObject.GetComponent<HexUnit>());

		if (target != null)
			return true;
		else
			return false;
	}
	
	public override bool perform(GameObject agent)
	{
		if (agent.GetComponent<HexUnit>().Actions > 0)
        {
			Debug.Log(agent.GetComponent<HexUnit>().Location.coordinates);
			agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().empireCells.Add(agent.GetComponent<HexUnit>().Location);
			agent.GetComponent<HexUnit>().Location.owner = agent.GetComponent<HexUnit>().ParentEmpire;

			if (agent.GetComponent<HexUnit>().Location.SpecialIndex == 1)
			{
				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
				{
					if (agent.GetComponent<HexUnit>().Location.GetNeighbor(d) != null && agent.GetComponent<HexUnit>().Location.GetNeighbor(d).owner == null)
					{
						agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().empireCells.Add(agent.GetComponent<HexUnit>().Location.GetNeighbor(d));
						agent.GetComponent<HexUnit>().Location.GetNeighbor(d).owner = agent.GetComponent<HexUnit>().ParentEmpire;
					}
				}
				agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().Cities.Add(agent.GetComponent<HexUnit>().Location);
				agent.GetComponent<GoapAgent>().map.RemoveUnit(agent.GetComponent<HexUnit>());

			}

			if (agent != null)
				agent.GetComponent<HexUnit>().Actions--;
			
			done = true;
			return true;
		}
		else
			return false;
		
	}


	HexCell FindBestCell(HexUnit unit)
	{
		if (GetClosestCity(unit) == null || (GetClosestTile(unit).coordinates.DistanceTo(unit.Location.coordinates) > GetClosestCity(unit).coordinates.DistanceTo(unit.Location.coordinates)))
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
				if (cell.GetNeighbor(d) != null && cell.GetNeighbor(d).owner == null && unit.ParentEmpire.GetComponent<Empire>().exploredCells.Contains(cell.GetNeighbor(d)) && cell.Unit == null )
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
