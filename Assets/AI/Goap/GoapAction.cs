using UnityEngine;
using System.Collections.Generic;

public abstract class GoapAction : MonoBehaviour {


	private HashSet<KeyValuePair<string, bool>> preconditions;
	private HashSet<KeyValuePair<string,bool>> effects;

	private bool inRange = false;

	/* The cost of performing the action. 
	 * Figure out a weight that suits the action. 
	 * Changing it will affect what actions are chosen during planning.*/
	public float cost = 1f;

	/**
	 * An action often has to perform on an object. This is that object. Can be null. */
	public HexCell target;

    public bool finishedAction = false;

	public GoapAction() {
		preconditions = new HashSet<KeyValuePair<string, bool>> ();
		effects = new HashSet<KeyValuePair<string, bool>> ();
	}

	public void doReset() {
		inRange = false;
		target = null;
		reset ();
	}

	/**
	 * Reset any variables that need to be reset before planning happens again.
	 */
	public abstract void reset();

	/**
	 * Is the action done?
	 */
	public abstract bool isDone();

	/**
	 * Procedurally check if this action can run. Not all actions
	 * will need this, but some might.
	 */
	public abstract bool checkProceduralPrecondition(GameObject agent);

	/**
	 * Run the action.
	 * Returns True if the action performed successfully or false
	 * if something happened and it can no longer perform. In this case
	 * the action queue should clear out and the goal cannot be reached.
	 */
	public abstract bool perform(GameObject agent);

	/**
	 * Does this action need to be within range of a target game object?
	 * If not then the moveTo state will not need to run for this action.
	 */
	public abstract bool requiresInRange ();
	

	/**
	 * Are we in range of the target?
	 * The MoveTo state will set this and it gets reset each time this action is performed.
	 */
	public bool isInRange () {
		return inRange;
	}
	
	public void setInRange(bool inRange) {
		this.inRange = inRange;
	}


	public void addPrecondition(string key, bool value) {
		preconditions.Add (new KeyValuePair<string, bool>(key, value) );
	}


	public void removePrecondition(string key) {
		KeyValuePair<string, bool> remove = default(KeyValuePair<string, bool>);
		foreach (KeyValuePair<string, bool> kvp in preconditions) {
			if (kvp.Key.Equals (key)) 
				remove = kvp;
		}
		if ( !default(KeyValuePair<string, bool>).Equals(remove) )
			preconditions.Remove (remove);
	}


	public void addEffect(string key, bool value) {
		effects.Add (new KeyValuePair<string, bool>(key, value) );
	}


	public void removeEffect(string key) {
		KeyValuePair<string, bool> remove = default(KeyValuePair<string, bool>);
		foreach (KeyValuePair<string, bool> kvp in effects) {
			if (kvp.Key.Equals (key)) 
				remove = kvp;
		}
		if ( !default(KeyValuePair<string, bool>).Equals(remove) )
			effects.Remove (remove);
	}

	
	public HashSet<KeyValuePair<string, bool>> Preconditions {
		get {
			return preconditions;
		}
	}

	public HashSet<KeyValuePair<string, bool>> Effects {
		get {
			return effects;
		}
	}
}