using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureEnemyTerritory : GoapAction
{
	private bool done = false;

	public CaptureEnemyTerritory()
	{
		addPrecondition("OnEnemyTerritory", true);
		addEffect("Expansionism", true);
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
		target = OnEnemyTerritory();

		if (target != null)
			return true;
		else
			return false;
	}

	public override bool perform(GameObject agent)
	{
		if (agent.GetComponent<HexUnit>().Actions > 0)
		{
			agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().empireCells.Remove(agent.GetComponent<HexUnit>().Location);

			if (agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().farms.Contains(agent.GetComponent<HexUnit>().Location))
			{
				agent.GetComponent<HexUnit>().Location.FarmLevel = 0;
				agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().farms.Remove(agent.GetComponent<HexUnit>().Location);
			}

			if (agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().Urban.Contains(agent.GetComponent<HexUnit>().Location))
			{
				agent.GetComponent<HexUnit>().Location.UrbanLevel = 0;
				agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().Urban.Remove(agent.GetComponent<HexUnit>().Location);
			}

			if (agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().Temples.Contains(agent.GetComponent<HexUnit>().Location))
			{
				agent.GetComponent<HexUnit>().Location.SpecialIndex = 0;
				agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().Temples.Remove(agent.GetComponent<HexUnit>().Location);

			}

			if (agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().Cities.Contains(agent.GetComponent<HexUnit>().Location))
			{
				agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().Cities.Remove(agent.GetComponent<HexUnit>().Location);
				agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().Cities.Add(agent.GetComponent<HexUnit>().Location);
			}

			if (agent.GetComponent<HexUnit>().Location == agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().GetCapital())
			{
				agent.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().status = "Capitulated";
			}

			agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().empireCells.Add(agent.GetComponent<HexUnit>().Location);
			agent.GetComponent<HexUnit>().Location.owner = agent.GetComponent<HexUnit>().ParentEmpire;
			agent.GetComponent<HexUnit>().Actions--;
			done = true;

			return true;
		}
		else
			return false;
	}

	HexCell OnEnemyTerritory()
	{
		if (transform.gameObject.GetComponent<HexUnit>().Location.owner == null)
			return null;
		else
		{
			if (transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AtWar.Contains(transform.gameObject.GetComponent<HexUnit>().Location.owner.GetComponent<Empire>().empireName))
				return transform.gameObject.GetComponent<HexUnit>().Location;
			else
				return null;
		}

		
	}
}
