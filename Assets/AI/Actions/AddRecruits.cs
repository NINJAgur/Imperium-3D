using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRecruits : GoapAction
{
	private bool done = false;

	public AddRecruits()
	{
		addPrecondition("hasRecruit", true);
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
		
		if (agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().manpower >= 500)
			return true;
		else
			return false;
	}

	public override bool perform(GameObject agent)
	{
		if (agent.GetComponent<HexUnit>().Actions > 0)
		{
			agent.GetComponent<HexUnit>().Attack += 250;
			agent.GetComponent<HexUnit>().Defense += 250;
			agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().manpower -= 500;
			done = true;
			return true;
		}
		else
			return false;
	}

	
}
