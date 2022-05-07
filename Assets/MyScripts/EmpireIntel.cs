using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpireIntel 
{
    public List<HexUnit> knownUnits = new List<HexUnit>();
    public List<HexCell> knownTerritories = new List<HexCell>();

    public List<HexCell> Urban = new List<HexCell>();
    public List<HexCell> farms = new List<HexCell>();
    public List<HexCell> Cities = new List<HexCell>();
    public List<HexCell> Temples = new List<HexCell>();

    public List<HexUnit> knownSoldiers = new List<HexUnit>();
    public List<HexUnit> knownWorkers = new List<HexUnit>();
    public List<HexUnit> knownSettlers = new List<HexUnit>();
    public List<HexUnit> knownCatapults = new List<HexUnit>();

    public List<string> AtWar = new List<string>();
    public List<string> AtPeace = new List<string>();
    public List<string> AlliedWith = new List<string>();
    public List<string> NAPWith = new List<string>();
    public List<string> MAWith = new List<string>();

    public int TotalResearchScore;
    public string empireName;

    public int[] resources = new int[6] { 0, 0, 0, 0, 0, 0 };


    int manpower;
    int treasury;
    int stability;

    /// <summary>
    /// 
    ///     Opinion    | Value Meaning of -1.0 |  Meaning of +1.0
    ///   _____________|_______________________|_________________
    ///    Scariness   |    Unthreatening      |    Terrifying
    /// Trustworthiness|      Deceitful        |      Honest
    ///   Aggression   |       Pacifist        |     Warmonger
    /// 
    /// </summary>

    private int score;

    public int Scariness;
    public int Trustworthiness;
    public int Aggression;

    #region RELATIONS

    public int GetScore()
    {
        score = (Scariness + Trustworthiness + Aggression) / 3;

        return score;
    }

    public void calculateRelations(Empire empire)
    {
        CalculateScariness(empire);
        CalculateTrustworthiness(empire);
        CalculateAggression(empire);
    }

    void CalculateScariness(Empire empire)
    {
        Scariness += (knownSoldiers.Count - empire.soldiers) * -5;
        
        int sumStrength = 0;

        foreach (EmpireIntel intel in empire.empireIntelList)
        {
            sumStrength += intel.knownSoldiers.Count + intel.knownCatapults.Count;

            if (intel.GetScore() < -20 && intel.AlliedWith.Contains(empireName))
                Scariness -= 10;

            if (intel.GetScore() < -20 && intel.MAWith.Contains(empireName))
                Scariness -= 5;
        }

        if (sumStrength / empire.empireIntelList.Count > knownSoldiers.Count)
            Scariness -= 30;

        if (AlliedWith.Contains(empire.empireName))
            Scariness += 100;

        if (NAPWith.Contains(empire.empireName))
            Scariness += 50;

        if (MAWith.Contains(empire.empireName))
            Scariness += 10;
    }

    void CalculateTrustworthiness(Empire empire)
    {
        int sumNAP = 0;
        int sumMA = 0;
        int sumAllied = 0;

        foreach (EmpireIntel intel in empire.empireIntelList)
        {
            sumNAP += intel.NAPWith.Count;
            sumMA += intel.MAWith.Count;
            sumAllied += intel.AlliedWith.Count;
        }

        if (sumNAP / empire.empireIntelList.Count > NAPWith.Count)
            Trustworthiness -= sumNAP / empire.empireIntelList.Count * 10;
        else
            Trustworthiness += sumNAP / empire.empireIntelList.Count * 10;

        if (sumMA / empire.empireIntelList.Count > MAWith.Count)
            Trustworthiness -= sumMA / empire.empireIntelList.Count * 10;
        else
            Trustworthiness += sumMA / empire.empireIntelList.Count * 10;

        if (sumAllied / empire.empireIntelList.Count > AlliedWith.Count)
            Trustworthiness -= sumAllied / empire.empireIntelList.Count * 30;
        else
            Trustworthiness += sumAllied / empire.empireIntelList.Count * 30;
    }

    void CalculateAggression(Empire empire)
    {
        if (AlliedWith.Contains(empire.empireName))
            Aggression += 100;
        else
            if (MAWith.Contains(empire.empireName))
                Aggression += 50;

        foreach (HexUnit soldier in knownSoldiers)
        {
            float distance = soldier.Location.coordinates.DistanceTo(empire.GetCapital().coordinates) ;
            if (distance < 5)
                Aggression -= (int)Mathf.Exp(-(distance) + 4);

            if (distance > 10)
                Aggression += (int)Mathf.Exp(-(distance) + 4);
        }

        foreach (string name in AtWar)
        {
            if (empire.AlliedWith.Contains(name))
                Aggression -= 50;

            if (empire.MAWith.Contains(name))
                Aggression -= 10;

            if (empire.AtWar.Contains(name))
                Aggression += 30;
        }

    }

    #endregion

    public void CalculateStats()
    {
        treasury += ApproxIncome();
        manpower += ApproxManpower();
        stability += (manpower + treasury - (knownTerritories.Count + Random.Range(0, knownTerritories.Count / 6)) * 1000) / 1000;

        foreach (HexCell cell in knownTerritories)
        {
            if (cell.FarmLevel != 0 && !farms.Contains(cell))
                farms.Add(cell);

            if (cell.SpecialIndex == 1 && !Cities.Contains(cell))
                Cities.Add(cell);

            if (cell.UrbanLevel != 0 && !Urban.Contains(cell))
                Urban.Add(cell);
        }

        foreach (HexUnit unit in knownUnits)
        {
            if (unit.type == "Soldier" && !knownSoldiers.Contains(unit))
                knownSoldiers.Add(unit);

            if (unit.type == "Worker" && !knownWorkers.Contains(unit))
                knownWorkers.Add(unit);

            if (unit.type == "Catapult" && !knownCatapults.Contains(unit))
                knownCatapults.Add(unit);

            if (unit.type == "Settler" && !knownSettlers.Contains(unit))
                knownSettlers.Add(unit);
        }

    }

    public int ApproxIncome()
    {
        return 50 * (knownTerritories.Count + Random.Range(0, knownTerritories.Count / 6)) + Urban.Count * 100;
    }

    public int ApproxManpower()
    {
        return 50 * (knownTerritories.Count + Random.Range(0, knownTerritories.Count / 6)) + farms.Count * 100; ;
    }

    public int ApproxStability()
    {
        return stability;
        
    }
}
