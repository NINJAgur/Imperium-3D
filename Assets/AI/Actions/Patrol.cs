using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : GoapAction
{
	private bool done = false;

	public Patrol()
	{
		addPrecondition("Patrolling", true);
		addEffect("Threat_concern", true);

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
		return false;
	}

	public override bool checkProceduralPrecondition(GameObject agent)
	{
		target = FindNextBorderCell(agent.GetComponent<HexUnit>());

		if (target != null)
			return true;
		else
			return false;
	}

	public override bool perform(GameObject agent)
	{
		if (agent.GetComponent<HexUnit>().Actions > 0)
		{
			agent.GetComponent<GoapAgent>().map.FindPath(transform.GetComponent<HexUnit>().Location, target, transform.GetComponent<HexUnit>());
			transform.GetComponent<HexUnit>().Travel(agent.GetComponent<GoapAgent>().map.GetPath());
			agent.GetComponent<HexUnit>().Actions--;

			done = true;
			return true;

		}
		else
			return false;

	}

	HexCell FindNextBorderCell(HexUnit unit)
    {
		List<HexCell> borderCells = new List<HexCell>();

		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			if (unit.Location.GetNeighbor(d) != null && unit.Location.GetNeighbor(d).owner == null && isBorder(unit.Location.GetNeighbor(d)))
				borderCells.Add(unit.Location.GetNeighbor(d));
		}

		if (borderCells.Count > 0)
			return borderCells[Random.Range(0, borderCells.Count)];
		else
			return null;
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
