using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_OVERSEER 
{
    public float ConstructionSpending; 
    public float Expansionism;         
    public float TechRace;             
    public float Threat_concern;
    public float Knowledge;

    public GameObject AI_gameManager;
    public Empire AI_currenEmpire;
    public Map AI_Grid;
   
    /// <summary>
    ///
    /// Construction spending - Affects how much resources are invested in developing tiles.
    /// Expansionism - Affects recruitment of soldiers, calculated by identifying nearby capture target i.e open tiles and free cities
    /// Knowledge - Affects recruitment of scouts, is calculated based on known empires and tiles (from empireIntel)
    /// Tech race - Affects the willingness of the empire to invest in devlopment of technologies.
    /// Threat concern – Affects how much an empire is likely to declare wars and be generally hostile to others.
    /// 
    /// </summary>

    public void InitData(GameObject manager, Empire empire, Map grid)
    {
        AI_gameManager = manager;
        AI_currenEmpire = empire;
        AI_Grid = grid;

        OverView();
    }

    public void OverView()
    {
        OverViewCompetition();
        OverViewOurEmpire();

        
        AI_RESOURCES AI = new AI_RESOURCES();

        int SendTotalScore = 0;

        List<Task> SortedList = GenerateTaskList().OrderByDescending(o => o.taskScore).ToList();

        foreach(Task t in SortedList)
        {
            Debug.Log(t.TaskName + " " + t.taskScore);
        }

        Queue<Task> TASKS = new Queue<Task>();

        foreach(Task t in SortedList)
        {
            SendTotalScore += (int)t.taskScore;
            TASKS.Enqueue(t);
        }

        AI.gameManager = AI_gameManager;
        AI.TotalScore = SendTotalScore;
        AI.HexGrid = AI_Grid;
        AI.current_empire = AI_currenEmpire;
        AI.BuildTasks(TASKS);

    }

    List<Task> GenerateTaskList()
    {
        List<Task> Perks = new List<Task>();
        
        Perks.Add(new Task("ConstructionSpending", ConstructionSpending));
        Perks.Add(new Task("Expansionism", Expansionism));
        Perks.Add(new Task("Knowledge", Knowledge));
        Perks.Add(new Task("TechRace", TechRace));
        Perks.Add(new Task("Threat_concern", Threat_concern));

        return Perks;
    }

    #region other empires

    void OverViewCompetition()
    {

        foreach (EmpireIntel rival in AI_currenEmpire.GetComponent<Empire>().empireIntelList)
        {
            rival.CalculateStats();
            checkUnits(rival);
            checkStats(rival);
        }
    }

    void checkUnits(EmpireIntel empire)
    {
        foreach (HexUnit soldier in empire.knownSoldiers)
        {
            if (AI_currenEmpire.AtWar.Contains(empire.empireName))
            {
                if (AI_currenEmpire.empireCells.Contains(soldier.Location))
                    Threat_concern += 50;
                else
                {
                    Threat_concern += 10;
                    ConstructionSpending += 5;
                }
                    
            }
            else
            {
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    if (soldier.Location.GetNeighbor(d).owner == null)
                    {
                        Expansionism += 1;
                    }
                }
                
                Threat_concern += GetClosestTileToUnit(soldier);
            }
        }


        foreach (HexUnit settler in empire.knownSettlers)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                if (settler.Location.GetNeighbor(d) != null && settler.Location.GetNeighbor(d).owner == null && settler.Location.GetNeighbor(d).SpecialIndex == 1)
                    Expansionism += 10;
                
            }
        }

        Expansionism += (empire.knownSettlers.Count - AI_currenEmpire.settlers) * 5;
        Threat_concern += (empire.knownSoldiers.Count - AI_currenEmpire.soldiers) * 5;
    }
    
    void checkStats(EmpireIntel empire)
    {
        ConstructionSpending += empire.farms.Count - AI_currenEmpire.farms.Count + empire.Urban.Count - AI_currenEmpire.Urban.Count + empire.Cities.Count - AI_currenEmpire.Cities.Count + empire.Temples.Count - AI_currenEmpire.Temples.Count;

        TechRace += 10 * (empire.TotalResearchScore - AI_currenEmpire.TotalResearchScore);
    }

    float GetClosestTileToUnit(HexUnit unit)
    {
        float min_distance = Mathf.Infinity;

        foreach (HexCell cell in AI_currenEmpire.empireCells)
        {
            if (min_distance > cell.coordinates.DistanceTo(unit.Location.coordinates))
            {
                min_distance = cell.coordinates.DistanceTo(unit.Location.coordinates);
                
            }
            
        }

        if (min_distance > 5)
            return 0;
        else
            return Mathf.Exp(-min_distance / 4 + 4);

    }

    #endregion

    #region Current Empire

    void OverViewOurEmpire()
    {
        Knowledge += ( 2700 - AI_currenEmpire.exploredCells.Count ) / (AI_currenEmpire.empireCells.Count * 2);
        CheckEmpireTiles();
        checkCloseDestinations();
        checkNavalDestinations();

        float ResearchBoost = Mathf.Exp(-AI_currenEmpire.empireIntelList.Count + 3);

        TechRace += ResearchBoost;

    }

    void CheckEmpireTiles()
    {
        foreach (HexCell cell in AI_currenEmpire.empireCells)
        {
            if (!AI_currenEmpire.farms.Contains(cell) && !AI_currenEmpire.Urban.Contains(cell) && !AI_currenEmpire.Cities.Contains(cell) && !AI_currenEmpire.Temples.Contains(cell))
            {
                ConstructionSpending += 5;
            }
        }
    }

    void checkCloseDestinations()
    {
        foreach (HexCell city in AI_Grid.cities)
        {
            if (city.owner == null)
                Expansionism += GetClosestTileToCity(city);
        }
    }

    float GetClosestTileToCity(HexCell target)
    {
        float min_distance = Mathf.Infinity;

        foreach (HexCell cell in AI_currenEmpire.empireCells)
        {
            if (!AI_currenEmpire.Cities.Contains(cell) && min_distance > cell.coordinates.DistanceTo(target.coordinates))
            {
                min_distance = cell.coordinates.DistanceTo(target.coordinates);

            }

        }

        if (min_distance > 5)
            return 0;
        else
            return Mathf.Exp(-min_distance / 4 + 5);

    }

    void checkNavalDestinations()
    {
        foreach (HexCell city in AI_Grid.cities)
        {
            if (city.owner == null && AI_currenEmpire.exploredCells.Contains(city))
            {
                Expansionism += 10;
            }
        }
    }

    #endregion

}
