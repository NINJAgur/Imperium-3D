using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardShip : GoapAction
{
	private bool done = false;

	public BoardShip()
	{
		addPrecondition("canBoard", true);
		addEffect("Colony_spending", true);

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
		target = FindClosestShip();

		if (target != null)
			return true;
		else
			return false;
	}

	public override bool perform(GameObject agent)
	{
		if (agent.GetComponent<HexUnit>().Actions > 0 && target.Unit.currentSoldiersOnBoard + target.Unit.currentWorkersOnBoard < target.Unit.ManpowerCapacity)
		{
			if (agent.GetComponent<HexUnit>().type == "Soldier")
				target.Unit.currentSoldiersOnBoard += 1;
			if (agent.GetComponent<HexUnit>().type == "Worker")
				target.Unit.currentWorkersOnBoard += 1;

			agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().units.Remove(agent.GetComponent<HexUnit>());
			agent.GetComponent<GoapAgent>().map.RemoveUnit(agent.GetComponent<HexUnit>());
			agent.GetComponent<HexUnit>().Actions--;
			done = true;

			return true;
		}
		else
			return false;
	}

	HexCell FindClosestShip()
	{
		float minDis = Mathf.Infinity;

		HexCell closest = null;

		List<HexCell> visible = transform.gameObject.GetComponent<GoapAgent>().map.GetVisibleCells(transform.gameObject.GetComponent<HexUnit>().Location, transform.gameObject.GetComponent<HexUnit>().VisionRange);

		foreach (HexCell cell in visible)
		{
			if (cell.Unit != null && cell.Unit != transform.gameObject.GetComponent<HexUnit>()
				&& cell.Unit.ParentEmpire == transform.gameObject.GetComponent<HexUnit>().ParentEmpire && cell.Unit.type == "Boat" && FindCoast(cell) != null)
			{
				if (cell.coordinates.DistanceTo(transform.gameObject.GetComponent<HexUnit>().Location.coordinates) < minDis)
				{
					closest = cell;
					minDis = cell.coordinates.DistanceTo(transform.gameObject.GetComponent<HexUnit>().Location.coordinates);
				}
			}
		}
		return closest;
	}

	HexCell FindCoast(HexCell cell)
	{
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			if (!cell.GetNeighbor(d).IsUnderwater)
				return cell.GetNeighbor(d);
		}

		return null;
	}
}
