using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : GoapAction
{
	private bool done = false;

	public int Enemies;
	public int Friendlies;
	public int Foreigners;
	public int Allies;

	bool CapitalInDanger = false;

	List<HexUnit> targets = new List<HexUnit>(); 

	public RangedAttack()
	{
		addPrecondition("rangedAttack", true);
		addEffect("Threat_concern", true);

	}

	public override void reset()
	{
		done = false;
        target = null;
		targets.Clear();
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
		target = checkInRange(agent.GetComponent<HexUnit>());

		if (target != null)
			return true;
		else
			return false;
	}

	public override bool perform(GameObject agent)
	{
		checkSurroundings(agent.GetComponent<GoapAgent>().map);

		if (agent.GetComponent<HexUnit>().Actions > 0)
		{
			HexUnit optimalTarget = null;

			float minHealth = Mathf.Infinity;

			foreach (HexUnit unit in targets)
            {
				if (unit.Defense < minHealth)
                {
					minHealth = unit.Defense;
					optimalTarget = unit;
                }
            }

			if (optimalTarget != null)
            {
				if (Friendlies + Allies > Enemies)
				{
					Vector3 a, b, c = agent.GetComponent<HexUnit>().Location.Position;
					a = c;
					b = optimalTarget.Location.Position;
					c = (b + optimalTarget.Location.Position) * 0.5f;
					float t = Time.deltaTime * 4f;
					Vector3 direction = Bezier.GetDerivative(a, b, c, t);
					direction.y = 0f;
					agent.transform.localRotation = Quaternion.LookRotation(direction);

					agent.GetComponentInChildren<Animator>().SetTrigger("Attack");

					if (agent.GetComponent<HexUnit>().Attack > target.Unit.Defense)
					{
						agent.GetComponent<GoapAgent>().map.RemoveUnit(target.Unit);
						agent.GetComponent<HexUnit>().Actions--;
					}
					else
						agent.GetComponent<HexUnit>().Defense = agent.GetComponent<HexUnit>().Attack;
					

					agent.GetComponent<HexUnit>().Actions--;

				}
				else
				{
					if (Friendlies + Allies == 0 && CapitalInDanger == false)
					{
						Retreat(agent);
						agent.GetComponent<HexUnit>().Actions--;
					}

				}
			}

			

			done = true;
			return true;
		}
		else
			return false; 
	
	}

	HexCell checkInRange(HexUnit unit)
	{
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			if (unit.Location.GetNeighbor(d) != null && unit.Location.GetNeighbor(d).Unit != null
				&& unit.ParentEmpire.GetComponent<Empire>().AtWar.Contains(unit.Location.GetNeighbor(d).Unit.ParentEmpire.GetComponent<Empire>().empireName))
            {
				targets.Add(unit.Location.GetNeighbor(d).Unit);
            }

		}

		if (targets.Count > 0)
			return targets[0].Location;
		else
			return null;
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
					if (transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AlliedWith.Contains(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName))
						Allies++;
					else
						Foreigners++;

				}
				else
					Friendlies++;
			}

			if (cell == transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().Capital)
				CapitalInDanger = true;
		}
	}

	void Retreat(GameObject agent)
	{
		HexCell retreat = FindFurthestTile(agent);

		if (retreat != null)
		{
			agent.GetComponent<GoapAgent>().map.FindPath(transform.GetComponent<HexUnit>().Location, retreat, transform.GetComponent<HexUnit>());
			transform.GetComponent<HexUnit>().Travel(agent.GetComponent<GoapAgent>().map.GetPath());
		}
		else
		{
			agent.GetComponent<GoapAgent>().map.RemoveUnit(agent.GetComponent<HexUnit>());
			return;
		}
	}

	HexCell FindFurthestTile(GameObject agent)
	{
		HexCell furthest = null;

		float maxDis = -Mathf.Infinity;

		List<HexCell> visible = agent.GetComponent<GoapAgent>().map.GetVisibleCells(transform.gameObject.GetComponent<HexUnit>().Location, transform.gameObject.GetComponent<HexUnit>().VisionRange);

		foreach (HexCell cell in visible)
		{
			if (agent.GetComponent<HexUnit>().type != "Boat")
			{
				if (!agent.GetComponent<HexUnit>().Location.IsUnderwater && agent.GetComponent<HexUnit>().Location.Unit == null &&
					!agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AtPeace.Contains(cell.owner.GetComponent<Empire>().empireName) &&
					agent.GetComponent<HexUnit>().Location.coordinates.DistanceTo(cell.coordinates) > maxDis)
				{
					furthest = cell;
					maxDis = agent.GetComponent<HexUnit>().Location.coordinates.DistanceTo(cell.coordinates);

				}

			}
			else
			{
				if (agent.GetComponent<HexUnit>().Location.IsUnderwater && agent.GetComponent<HexUnit>().Location.Unit == null &&
					!agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AtPeace.Contains(cell.owner.GetComponent<Empire>().empireName) &&
					agent.GetComponent<HexUnit>().Location.coordinates.DistanceTo(cell.coordinates) > maxDis)
				{
					furthest = cell;
					maxDis = agent.GetComponent<HexUnit>().Location.coordinates.DistanceTo(cell.coordinates);

				}
			}

		}


		return furthest;
	}
}
