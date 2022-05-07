using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionList 
{
    
    List<GoapAction> actionsList = new List<GoapAction>();

    /*
    public ActionList()
    {
        // PURCHASE UNITS

        GoapAction BuySoldier = new GoapAction(3f, AI_GROUP.BuySoldier);
        BuySoldier.addPrecondition("canBuySoldier", true);
        BuySoldier.addEffect("hasSoldiers", true);
        actionsList.Add(BuySoldier);


        GoapAction BuyWorker = new GoapAction(2f, AI_GROUP.BuyWorker);
        BuyWorker.addPrecondition("canBuyWorker", true);
        BuyWorker.addEffect("hasWorkers", true);
        actionsList.Add(BuyWorker);

        GoapAction BuyShip = new GoapAction(4f, AI_GROUP.BuyShip);
        BuyShip.addPrecondition("canBuyShip", true);
        BuyShip.addEffect("hasShips", true);
        actionsList.Add(BuyShip);

        // UNIT ABILITIES

        GoapAction SoldierAbility = new GoapAction(2f, AI_GROUP.Expansion);
        SoldierAbility.addPrecondition("hasSoldiers", true);
        SoldierAbility.addEffect("Expansionism", true);
        actionsList.Add(SoldierAbility);

        GoapAction WorkerAbility = new GoapAction(2f, AI_GROUP.Development);
        WorkerAbility.addPrecondition("hasWorkers", true);
        WorkerAbility.addEffect("ConstructionSpending", true);
        actionsList.Add(WorkerAbility);

        GoapAction FindColonies = new GoapAction(1f, AI_GROUP.Colonize);
        FindColonies.addPrecondition("UnitOnBoard", true);
        FindColonies.addEffect("Colony_spending", true);
        actionsList.Add(FindColonies);
   

        // Research

        GoapAction purchaseResearch = new GoapAction(5f, AI_GROUP.PurchaseResearch);
        purchaseResearch.addPrecondition("canBuyResearch", true);
        purchaseResearch.addEffect("TechRace", true);
        actionsList.Add(purchaseResearch);

        // Ship funcs

        GoapAction MoveSoldierToBoat = new GoapAction(1f, AI_GROUP.MoveSToBoat);
        MoveSoldierToBoat.addPrecondition("hasSoldiers", true);
        MoveSoldierToBoat.addEffect("UnitOnBoard", true);
        actionsList.Add(MoveSoldierToBoat);

        GoapAction MoveWorkerToBoat = new GoapAction(2f, AI_GROUP.MoveWToBoat);
        MoveWorkerToBoat.addPrecondition("hasWorkers", true);
        MoveWorkerToBoat.addEffect("UnitOnBoard", true);
        actionsList.Add(MoveWorkerToBoat);

        // Dealing with THREATS

        GoapAction DefenseFleet = new GoapAction(3f, AI_GROUP.DFleet);
        DefenseFleet.addPrecondition("hasEnemies", true);
        DefenseFleet.addPrecondition("hasShips", true);
        DefenseFleet.addEffect("Threat_concern", true);
        actionsList.Add(DefenseFleet);

        GoapAction DefenseForce = new GoapAction(2f, AI_GROUP.DFront);
        DefenseForce.addPrecondition("hasEnemies", true);
        DefenseForce.addPrecondition("hasSoldiers", true);
        DefenseForce.addEffect("Threat_concern", true);
        actionsList.Add(DefenseForce);

        GoapAction OffenseFleet = new GoapAction(3f, AI_GROUP.OffensiveFleet);
        OffenseFleet.addPrecondition("AtWar", true);
        OffenseFleet.addPrecondition("hasShips", true);
        OffenseFleet.addEffect("Threat_concern", true);
        actionsList.Add(OffenseFleet);

        GoapAction OffenseForce = new GoapAction(2f, AI_GROUP.OffensiveFront);
        OffenseForce.addPrecondition("AtWar", true);
        OffenseForce.addPrecondition("hasSoldiers", true);
        OffenseForce.addEffect("Threat_concern", true);
        actionsList.Add(OffenseForce);


    }
    */
    public GoapAction[] ReturnActions()
    {
        GoapAction[] actions = new GoapAction[actionsList.Count];

        int index = 0;

        foreach (GoapAction action in actionsList)
        {
            actions[index] = action;

            index++;
        }

        return actions;
    }

}


