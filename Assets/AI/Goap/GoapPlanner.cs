using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Plans what actions can be completed in order to fulfill a goal state.
 */
public class GoapPlanner
{
    /**
	 * Plan what sequence of actions can fulfill the goal.
	 * Returns null if a plan could not be found, or a list of the actions
	 * that must be performed, in order, to fulfill the goal.
	 */
    /**
         * Plan what sequence of actions can fulfill the goal.
         * Returns null if a plan could not be found, or a list of the actions
         * that must be performed, in order, to fulfill the goal.
         */
    public Queue<GoapAction> plan(GameObject agent, HashSet<GoapAction> availableActions, HashSet<KeyValuePair<string, bool>> worldState,  HashSet<KeyValuePair<string, bool>> goal)
    {
        foreach (GoapAction a in availableActions)
        {
            a.doReset();
        }

        // check what actions can run using their checkProceduralPrecondition
        HashSet<GoapAction> usableActions = new HashSet<GoapAction>();
        foreach (GoapAction a in availableActions)
        {
            if (a.checkProceduralPrecondition(agent))
                usableActions.Add(a);
        }

        // we now have all actions that can run, stored in usableActions


        // build up the tree and record the leaf nodes that provide a solution to the goal.
        List<Node> leaves = new List<Node>();

        // build graph
        Node start = new Node(null, 0, goal, null);
        HashSet<KeyValuePair<string, bool>> end = worldState;
        bool success = buildGraph(end, start, leaves, usableActions, goal);

        if (!success)
        {
            // oh no, we didn't get a plan
            // HERE! 
            Console.WriteLine("NO PLAN for ");
            return null;
        }

        // get the cheapest leaf
        Node cheapest = null;
        foreach (Node leaf in leaves)
        {
            if (cheapest == null)
                cheapest = leaf;
            else
            {
                if (leaf.runningCost < cheapest.runningCost)
                    cheapest = leaf;
            }
        }

        // get its node and work back through the parents
        List<GoapAction> result = new List<GoapAction>();
        Node n = cheapest;
        while (n != null)
        {
            if (n.action != null)
            {
                result.Add(n.action); // add the action at the end of the list
            }
            n = n.parent;
        }
        // we now have this action list in correct order

        Queue<GoapAction> queue = new Queue<GoapAction>();
        foreach (GoapAction a in result)
        {
            queue.Enqueue(a);
        }

        // hooray we have a plan!
        return queue;
    }

    /**
     * Returns true if at least one solution was found.
     * The possible paths are stored in the leaves list. Each leaf has a
     * 'runningCost' value where the lowest cost will be the best action
     * sequence.
     */
    private bool buildGraph(HashSet<KeyValuePair<string, bool>> end, Node parent, List<Node> leaves, HashSet<GoapAction> usableActions, HashSet<KeyValuePair<string, bool>> goal)
    {
        bool foundOne = false;

        // go through each action available at this node and see if we can use it here
        foreach (GoapAction action in usableActions)
        {

            // if the parent state has the conditions for this action's preconditions, we can use it here
            if (inState(action.Effects, parent.state))
            {

                // apply the action's effects to the parent state
                HashSet<KeyValuePair<string, bool>> currentState = populateState(parent.state, action.Effects, action.Preconditions);
                //Debug.Log("Current State"+ prettyPrint(currentState));


                Node node = new Node(parent, parent.runningCost + action.cost, currentState, action);

                if (inState(currentState, end))
                {
                    // we found a solution!
                    leaves.Add(node);
                    foundOne = true;
                }
                else
                {
                    // not at a solution yet, so test all the remaining actions and branch out the tree
                    HashSet<GoapAction> subset = actionSubset(usableActions, action);
                    bool found = buildGraph(end, node, leaves, subset, goal);
                    if (found)
                        foundOne = true;
                }
            }
        }

        return foundOne;
    }

    /**
     * Create a subset of the actions excluding the removeMe one. Creates a new set.
     */
    private HashSet<GoapAction> actionSubset(HashSet<GoapAction> actions, GoapAction removeMe)
    {
        HashSet<GoapAction> subset = new HashSet<GoapAction>();
        foreach (GoapAction a in actions)
        {
            if (!a.Equals(removeMe))
                subset.Add(a);
        }
        return subset;
    }

    /**
     * Check that all items in 'test' are in 'state'. If just one does not match or is not there
     * then this returns false.
     */
    private bool inState(HashSet<KeyValuePair<string, bool>> test, HashSet<KeyValuePair<string, bool>> state)
    {
        bool allMatch = true;
        foreach (KeyValuePair<string, bool> t in test)
        {
            bool match = false;
            foreach (KeyValuePair<string, bool> s in state)
            {
                if (s.Equals(t))
                {
                    match = true;
                    break;
                }
            }
            if (!match)
                allMatch = false;
        }
        return allMatch;
    }

    /**
     * Apply the stateAdd to the currentState
     */
    private HashSet<KeyValuePair<string, bool>> populateState(HashSet<KeyValuePair<string, bool>> currentState, HashSet<KeyValuePair<string, bool>> stateRemove, HashSet<KeyValuePair<string, bool>> stateAdd)
    {
        HashSet<KeyValuePair<string, bool>> state = new HashSet<KeyValuePair<string, bool>>();
        // copy the KVPs over as new objects
        foreach (KeyValuePair<string, bool> s in currentState)
        {
            state.Add(new KeyValuePair<string, bool>(s.Key, s.Value));
        }

        foreach (KeyValuePair<string, bool> change in stateRemove)
        {
            // if the key exists in the current state, remove it
            bool exists = false;

            foreach (KeyValuePair<string, bool> s in state)
            {
                if (s.Equals(change))
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
            {
                state.RemoveWhere((KeyValuePair<string, bool> kvp) => { return kvp.Key.Equals(change.Key); });
            }
        }

        foreach (KeyValuePair<string, bool> change in stateAdd)
        {
            // if the key exists in the current state, update the Value
            bool exists = false;

            foreach (KeyValuePair<string, bool> s in state)
            {
                if (s.Equals(change))
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
            {
                state.RemoveWhere((KeyValuePair<string, bool> kvp) => { return kvp.Key.Equals(change.Key); });
                KeyValuePair<string, bool> updated = new KeyValuePair<string, bool>(change.Key, change.Value);
                state.Add(updated);
            }
            // if it does not exist in the current state, add it
            else
            {
                state.Add(new KeyValuePair<string, bool>(change.Key, change.Value));
            }
        }
        return state;
    }

    /**
     * Used for building up the graph and holding the running costs of actions.
     */
    private class Node
    {
        public Node parent;
        public float runningCost;
        public HashSet<KeyValuePair<string, bool>> state;
        public GoapAction action;

        public Node(Node parent, float runningCost, HashSet<KeyValuePair<string, bool>> state, GoapAction action)
        {
            this.parent = parent;
            this.runningCost = runningCost;
            this.state = state;
            this.action = action;
        }
    }

    public static string prettyPrint(HashSet<KeyValuePair<string, bool>> state)
    {
        String s = "";
        foreach (KeyValuePair<string, bool> kvp in state)
        {
            s += kvp.Key + ":" + kvp.Value.ToString();
            s += ", ";
        }
        return s;
    }

}


