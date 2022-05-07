using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngageEnemies : GoapAction
{
	private bool done = false;

	public int Enemies;
	public int Friendlies;
	public int Foreigners;
	public int Allies;
	
	bool CapitalInDanger = false;

	public EngageEnemies()
	{
		addPrecondition("hasEnemies", true);
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
		return true;
	}

	public override bool checkProceduralPrecondition(GameObject agent)
	{
		target = FindEnemyUnit(agent.GetComponent<GoapAgent>().map, agent);

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
			if (Friendlies + Allies > Enemies)
            {
				if (agent.GetComponent<HexUnit>().Attack > target.Unit.Defense)
				{
					transform.GetComponentInChildren<Animation>().clip = transform.GetComponentInChildren<Animation>().GetClip("attack");
					transform.GetComponentInChildren<Animation>().Play();
					transform.GetComponentInChildren<Animation>().clip = transform.GetComponentInChildren<Animation>().GetClip("idle");
					transform.GetComponentInChildren<Animation>().Play();
					agent.GetComponent<GoapAgent>().map.RemoveUnit(target.Unit);
					agent.GetComponent<HexUnit>().Actions--;
				}
				else
				{
					if (target.Unit.Defense > agent.GetComponent<HexUnit>().Attack + agent.GetComponent<HexUnit>().Defense)
					{
						agent.GetComponent<GoapAgent>().map.RemoveUnit(agent.GetComponent<HexUnit>());
					}
					else
					{
						transform.GetComponentInChildren<Animation>().clip = transform.GetComponentInChildren<Animation>().GetClip("attack");
						transform.GetComponentInChildren<Animation>().Play();
						transform.GetComponentInChildren<Animation>().clip = transform.GetComponentInChildren<Animation>().GetClip("idle");
						transform.GetComponentInChildren<Animation>().Play();

						agent.GetComponent<HexUnit>().Attack = (target.Unit.Defense - agent.GetComponent<HexUnit>().Attack + agent.GetComponent<HexUnit>().Defense) / 2;
						agent.GetComponent<HexUnit>().Defense = (target.Unit.Defense - agent.GetComponent<HexUnit>().Attack + agent.GetComponent<HexUnit>().Defense) / 2;
						Retreat(agent);
						agent.GetComponent<HexUnit>().Actions--;
					}

				}

			}
			else
            {
				if (Friendlies + Allies == 0 && CapitalInDanger == false)
                {
					Retreat(agent);
					agent.GetComponent<HexUnit>().Actions--;
				}

            }

			done = true;
			return true;
		}
		else
			return false;
	}

	HexCell FindEnemyUnit(Map Grid, GameObject agent)
	{
		List<HexUnit> enemies = new List<HexUnit>();

		List<HexCell> visible = Grid.GetVisibleCells(transform.gameObject.GetComponent<HexUnit>().Location, transform.gameObject.GetComponent<HexUnit>().VisionRange);

		foreach (HexCell cell in visible)
		{
			if (cell.Unit != null && cell.Unit.ParentEmpire != transform.gameObject.GetComponent<HexUnit>().ParentEmpire)
			{
				if (transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AtWar.Contains(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName) && cell.Unit.type == agent.GetComponent<HexUnit>().type)
					enemies.Add(cell.Unit);
			}
		}

		float minStrength = Mathf.Infinity;

		HexCell optimalTargetLocation = null;

		foreach (HexUnit unit in enemies)
        {
			if (unit.Attack + unit.Defense < minStrength)
            {
				minStrength = unit.Attack + unit.Defense;
				optimalTargetLocation = unit.Location;
            }
        }

		return optimalTargetLocation;
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
