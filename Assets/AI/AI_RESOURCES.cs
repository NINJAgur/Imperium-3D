using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_RESOURCES
{
    Queue<Task> TASKS = new Queue<Task>();
    Queue<Task> builder = new Queue<Task>();
    public Task currentTask;

    public int TotalScore;
    public Empire current_empire;
	public GameObject gameManager;
	public Map HexGrid;

    Queue<List<HexUnit>> TASK_LISTS;

    public float treasury;
    public float manpower;
    List<HexUnit> soldiers = new List<HexUnit>();
    List<HexUnit> workers = new List<HexUnit>();
    List<HexUnit> ranged = new List<HexUnit>();
    List<HexUnit> settlers = new List<HexUnit>();

    public void BuildTasks(Queue<Task> t)
	{
        builder = CreateTemp(t);
        TASK_LISTS = BuildLists(builder);

        current_empire.GetComponent<AI_DIP>().Build(current_empire, TASKS.Peek().TaskName);
        current_empire.GetComponent<AI_DIP>().Init();
        StartWorking();
    }

    Queue<Task> CreateTemp(Queue<Task> t)
    {
        Queue<Task> temp = new Queue<Task>();

        while (t.Count > 0)
        {
            TASKS.Enqueue(t.Peek());
            temp.Enqueue(t.Peek());

            t.Dequeue();
        }

        return temp;
    }

	void StartWorking()
    {		
		while (TASKS.Count > 0)
        {
			currentTask = TASKS.Peek();

            if (currentTask.taskScore <= 0)
            {
                TASKS.Dequeue();
                continue;
            }

            HashSet<KeyValuePair<string, bool>> goal = new HashSet<KeyValuePair<string, bool>>();
            goal.Add(new KeyValuePair<string, bool>(currentTask.TaskName, true));

            Debug.Log(currentTask.TaskName + " "+ TASK_LISTS.Peek().Count);

            foreach (HexUnit unit in TASK_LISTS.Peek())
            {
                unit.gameObject.GetComponent<GoapAgent>().Activate(HexGrid, goal);
            }

            TASK_LISTS.Dequeue();
			TASKS.Dequeue();
		}

        HashSet<KeyValuePair<string, bool>> scouting = new HashSet<KeyValuePair<string, bool>>();
        scouting.Add(new KeyValuePair<string, bool>("Knowledge", true));

        foreach (HexUnit unit in current_empire.units)
        {
            if (unit.Actions == 3 && unit.type == "Soldier")
                unit.gameObject.GetComponent<GoapAgent>().Activate(HexGrid, scouting);
        }

        Debug.Log("-------------------------------------");
	}


    Queue<List<HexUnit>> BuildLists(Queue<Task> t)
    {
        Queue<List<HexUnit>> lists = new Queue<List<HexUnit>>();

        float totalTreasury = current_empire.treasury;
        float totalManpower = current_empire.manpower;

        while (t.Count > 0)
        {
            List<HexUnit> current = new List<HexUnit>();

            treasury = (t.Peek().taskScore / TotalScore) * totalTreasury;
            manpower = (t.Peek().taskScore / TotalScore) * totalManpower;

            int TreasuryAvailable;
            int ManpowerAvailable;
            int TimesPurchase;

            if (t.Peek().taskScore <= 0)
            {
                t.Dequeue();
                continue;
            }

            if (t.Peek().TaskName == "ConstructionSpending")
            {
                TreasuryAvailable = (int)treasury / 1000;
                ManpowerAvailable = (int)manpower / 1000;

                TimesPurchase = Mathf.Min(ManpowerAvailable, TreasuryAvailable);

                for (int i = 0; i < TimesPurchase; i++)
                    BuyWorker();

            }

            if (t.Peek().TaskName == "Expansionism")
            {
                TreasuryAvailable = (int)treasury / 500;
                ManpowerAvailable = (int)manpower / 2000;

                TimesPurchase = Mathf.Min(ManpowerAvailable, TreasuryAvailable);

                for (int i = 0; i < TimesPurchase; i++)
                    BuySettler();

                TreasuryAvailable = (int)treasury / 1000;
                ManpowerAvailable = (int)manpower / 1000;

                TimesPurchase = Mathf.Min(ManpowerAvailable, TreasuryAvailable);

                for (int i = 0; i < TimesPurchase; i++)
                    BuySoldier();

            }

            if (t.Peek().TaskName == "Threat_concern")
            {
                TreasuryAvailable = (int)treasury / 1500;
                ManpowerAvailable = (int)manpower / 1000;

                TimesPurchase = Mathf.Min(ManpowerAvailable, TreasuryAvailable);

                for (int i = 0; i < TimesPurchase; i++)
                    BuyCatapult();

                TreasuryAvailable = (int)treasury / 1000;
                ManpowerAvailable = (int)manpower / 1000;

                TimesPurchase = Mathf.Min(ManpowerAvailable, TreasuryAvailable);

                for (int i = 0; i < TimesPurchase; i++)
                    BuySoldier();
            }

            if (t.Peek().TaskName == "Knowledge")
            {
                TreasuryAvailable = (int)treasury / 1000;
                ManpowerAvailable = (int)manpower / 1000;

                TimesPurchase = Mathf.Min(ManpowerAvailable, TreasuryAvailable);

                for (int i = 0; i < TimesPurchase; i++)
                    BuySoldier();
            }

            if (t.Peek().TaskName == "TechRace")
            {
                PurchaseResearch();
            }


            UpdateUnits();

            int amountSoldiers = AssignUnits(t.Peek())[0];
            int amountSettlers = AssignUnits(t.Peek())[1];
            int amountWorkers = AssignUnits(t.Peek())[2];
            int amountCatapults = AssignUnits(t.Peek())[3];

            for (int i = 0; i < amountSettlers; i++)
            {
                current.Add(settlers[i]);
            }

            for (int i = 0; i < amountSoldiers; i++)
            {
                current.Add(soldiers[i]);
            }

            for (int i = 0; i < amountWorkers; i++)
            {
                current.Add(workers[i]);
            }

            for (int i = 0; i < amountCatapults; i++)
            {
                current.Add(ranged[i]);
            }

            lists.Enqueue(current);

            soldiers.Clear();
            workers.Clear();
            settlers.Clear();
            ranged.Clear();

            t.Dequeue();

        }

        return lists;
    }

    int[] AssignUnits(Task t)
    {
        int[] arr = new int[4];

        if (t.TaskName == "Knowledge" || t.TaskName == "Threat_concern")
        {
            arr[0] = (int)((t.taskScore / (FindTask("Knowledge", TASKS).taskScore + FindTask("Threat_concern", TASKS).taskScore + FindTask("Expansionism", TASKS).taskScore)) * soldiers.Count);
            arr[3] = (int)((t.taskScore / (FindTask("Knowledge", TASKS).taskScore + FindTask("Threat_concern", TASKS).taskScore)) * ranged.Count);
        }

        if (t.TaskName == "Expansionism")
        {
            arr[1] = settlers.Count;
            arr[0] = (int)((t.taskScore / (FindTask("Knowledge", TASKS).taskScore + FindTask("Threat_concern", TASKS).taskScore + FindTask("Expansionism", TASKS).taskScore)) * soldiers.Count);
        }

        if (t.TaskName == "ConstructionSpending" )
            arr[2] = workers.Count;

        return arr;
    }

    void UpdateUnits()
    {
        foreach (HexUnit unit in current_empire.units)
        {
            if (unit.Actions > 0)
            {
                if (unit.type == "Soldier")
                    soldiers.Add(unit);

                if (unit.type == "Worker")
                    workers.Add(unit);

                if (unit.type == "Catapult")
                    ranged.Add(unit);

                if (unit.type == "Settler")
                    settlers.Add(unit);
            }
        }
    }


    void BuySoldier()
    {
        treasury -= 1000;
        manpower -= 1000;
        gameManager.GetComponent<GameManager>().manager.GetComponent<UIManager>().currentSpawn = current_empire.Cities[Random.Range(0, current_empire.Cities.Count - 1)];
        gameManager.GetComponent<GameManager>().manager.GetComponent<UIManager>().CreateUnit();
    }

   void BuyWorker()
    {
        treasury -= 1000;
        manpower -= 1000;
        gameManager.GetComponent<GameManager>().manager.GetComponent<UIManager>().currentSpawn = current_empire.Cities[Random.Range(0, current_empire.Cities.Count - 1)];
        gameManager.GetComponent<GameManager>().manager.GetComponent<UIManager>().CreateWorker();
    }

    void BuySettler()
    {
        treasury -= 500;
        manpower -= 2000;
        gameManager.GetComponent<GameManager>().manager.GetComponent<UIManager>().currentSpawn = current_empire.Cities[Random.Range(0, current_empire.Cities.Count - 1)];
        gameManager.GetComponent<GameManager>().manager.GetComponent<UIManager>().CreateSettler();
    }

    void BuyCatapult()
    {
        treasury -= 1500;
        manpower -= 1000;
        gameManager.GetComponent<GameManager>().manager.GetComponent<UIManager>().currentSpawn = current_empire.Cities[Random.Range(0, current_empire.Cities.Count - 1)];
        gameManager.GetComponent<GameManager>().manager.GetComponent<UIManager>().CreateCatapult();
    }

    public int[] timers = new int[4] { 0, 3, 5, 7 };

    Task FindTask(string name, Queue<Task> queue)
    {
        foreach (Task t in queue)
        {
            if (t.TaskName == name)
                return t;
        }

        return null;
    }

    public void PurchaseResearch()
    {
        List<ResearchSubTab> availableTabs = new List<ResearchSubTab>();

        foreach (ResearchSubTab tab in current_empire.SubTabs)
        {
            if ((gameManager.GetComponent<GameManager>().Turn - tab.ActiveTurn) == timers[tab.indexL] ||
                tab.middleTree != null && (gameManager.GetComponent<GameManager>().Turn - tab.ActiveTurn) == timers[tab.indexM] ||
                (gameManager.GetComponent<GameManager>().Turn - tab.ActiveTurn) == timers[tab.indexR])
                availableTabs.Add(tab);
        }

        foreach (ResearchSubTab tab in availableTabs)
        {
            if (treasury >= 500)
            {
                if (tab.Tree == "Left")
                {
                    tab.indexL += 1;
                    current_empire.treasury -= 500;
                    treasury -= 500;
                    return;
                }

                if (tab.middleTree != null && tab.Tree == "Middle")
                {
                    tab.indexM += 1;
                    current_empire.treasury -= 500;
                    treasury -= 500;
                    return;
                }

                if (tab.Tree == "Right")
                {
                    tab.indexR += 1;
                    current_empire.treasury -= 500;
                    treasury -= 500;
                    return;
                }
            }
        }
    }
}

