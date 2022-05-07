using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colonize : GoapAction
{
	private bool done = false;

	public Colonize()
	{
		addPrecondition("hasColonies", true);
		addPrecondition("hasUnitsOnBoard", true);
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
		target = hasColonies(agent);

		if (target != null)
			return true;
		else
			return false;
	}

	public override bool perform(GameObject agent)
	{
		if (agent.GetComponent<HexUnit>().Actions > 0)
		{

			if (agent.GetComponent<HexUnit>().Actions > 0 && agent.GetComponent<HexUnit>().currentSoldiersOnBoard > 0)
			{
				DeploySoldier(agent);
			}

			if (agent.GetComponent<HexUnit>().Actions > 0 && agent.GetComponent<HexUnit>().currentWorkersOnBoard > 0)
			{
				DeployWorker(agent);
			}

			done = true;
			return true;
		}
		else
			return false;
	}

	HexCell hasColonies(GameObject agent)
	{
		HexCell closest = null;
		float minDis = Mathf.Infinity;

		foreach (HexCell city in agent.GetComponent<GoapAgent>().map.cities)
		{
			if (city.owner == null && city.IsExplored && FindCoast(city) != null)
			{
				if (agent.GetComponent<HexUnit>().Location.coordinates.DistanceTo(city.coordinates) < minDis)
                {
					minDis = agent.GetComponent<HexUnit>().Location.coordinates.DistanceTo(city.coordinates);
					closest = city;

				}
			}
		}
		return closest;
	}

	HexCell FindCoast(HexCell cell)
	{
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			if (cell.GetNeighbor(d) != null && cell.GetNeighbor(d).IsUnderwater)
				return cell.GetNeighbor(d);
		}

		return null;
	}


	void DeploySoldier(GameObject agent)
	{
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			if (agent.GetComponent<HexUnit>().Location.GetNeighbor(d) != null && !agent.GetComponent<HexUnit>().Location.GetNeighbor(d).IsUnderwater && !agent.GetComponent<HexUnit>().Location.GetNeighbor(d).Unit)
			{
				agent.GetComponent<GoapAgent>().map.AddUnit(Instantiate(HexUnit.unitPrefab), agent.GetComponent<HexUnit>().Location.GetNeighbor(d), Random.Range(0f, 360f), agent.GetComponent<HexUnit>().ParentEmpire, "Soldier");
				agent.GetComponent<HexUnit>().Actions--;
				agent.GetComponent<HexUnit>().currentSoldiersOnBoard--;
				break;
			}
		}
	}

	void DeployWorker(GameObject agent)
	{
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			if (agent.GetComponent<HexUnit>().Location.GetNeighbor(d) != null && !agent.GetComponent<HexUnit>().Location.GetNeighbor(d).IsUnderwater && !agent.GetComponent<HexUnit>().Location.GetNeighbor(d).Unit)
			{
				agent.GetComponent<GoapAgent>().map.AddUnit(Instantiate(HexUnit.workerPrefab), agent.GetComponent<HexUnit>().Location.GetNeighbor(d), Random.Range(0f, 360f), agent.GetComponent<HexUnit>().ParentEmpire, "Worker");
				agent.GetComponent<HexUnit>().Actions--;
				agent.GetComponent<HexUnit>().currentWorkersOnBoard--;
				break;
			}
		}
	}
}
