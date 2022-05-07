using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToBorder : GoapAction
{
	private bool done = false;

	public MoveToBorder()
	{
		addPrecondition("hasForeigners", true);
		addEffect("Patrolling", true);

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
		target = FindEmpireBorderCell(agent.GetComponent<HexUnit>());

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

	HexCell FindEmpireBorderCell(HexUnit unit)
	{
		float minDis = Mathf.Infinity;

		HexCell closest = null;

		foreach (HexCell cell in unit.ParentEmpire.GetComponent<Empire>().empireCells)
		{
			if (unit.GetComponent<HexUnit>().type == "Soldier")
			{
				if (cell.Unit == null && !cell.IsUnderwater && isBorder(cell))
				{
					if (unit.Location.coordinates.DistanceTo(cell.coordinates) < minDis)
					{
						closest = cell;
						minDis = unit.Location.coordinates.DistanceTo(cell.coordinates);
					}
				}
			}
			else
			{
				if (cell.Unit == null && cell.IsUnderwater && isBorder(cell))
				{
					if (unit.Location.coordinates.DistanceTo(cell.coordinates) < minDis)
					{
						closest = cell;
						minDis = unit.Location.coordinates.DistanceTo(cell.coordinates);
					}
				}
			}
		}

		return closest;
	}

	bool isBorder(HexCell cell)
	{
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			if (cell.GetNeighbor(d) != null && cell.GetNeighbor(d).owner == null)
				return true;
		}
		return false;
	}

}
