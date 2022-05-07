using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scout : GoapAction
{
	private bool done = false;

	public Scout()
	{
		addPrecondition("hasScout", true);
		addEffect("Knowledge", true);

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
		target = FindCell(agent.GetComponent<HexUnit>());

		if (target != null)
			return true;
		else
			return false;
	}

	public override bool perform(GameObject agent)
	{
		done = true;
		return true;

	}

	HexCell FindCell(HexUnit unit)
	{
		unit.visibile = unit.gameObject.GetComponent<GoapAgent>().map.GetVisibleCells(unit.Location, unit.VisionRange);

		bool found = false;

		HexCell cellTo = null;

		while (!found)
		{
			cellTo = unit.visibile[Random.Range(0, unit.visibile.Count - 1)];

			if ((cellTo.owner == null || HasCityNearby(cellTo)) && cellTo != unit.Location)
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
