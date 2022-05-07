using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Empire : MonoBehaviour
{
    public Map hexGrid;

    // stats
    public float treasury;  // amount of coins empire has to buy units
    public float manpower;  // amount of men empire has to recruit men and add them power

    public int[] resources;

    private float stability;
    private float[] labels;

    // units
    public int soldiers;
    public int workers;
    public int ranged;
    public int settlers;

    public int totalScore = 0;

    // locations data

    public int EmpireIndex;
    public string empireName = "";
    public Color empireColor;
    public GameObject UI;
    public bool activated;
    public HexCell Capital;
    public bool isPlayerEmpire;

    // Diplomacy
    public string status;
    public List<string> AtWar = new List<string>();
    public List<string> AtPeace = new List<string>();
    public List<string> AlliedWith = new List<string>();
    public List<string> NAPWith = new List<string>();
    public List<string> IRWith = new List<string>();
    public List<string> MAWith = new List<string>();

    public int PreWarSize;

    // research
    public List<ResearchSubTab> SubTabs = new List<ResearchSubTab>();

    public List<int> boatrBuffs;
    public string treeBoat;
    public List<int> soliderBuffs;
    public string treeSoldier;
    public List<int> consturctionBuffs;
    public string treeConsturction;
    public List<int> doctrineBuffs;
    public string treeDoctrine;

    public int TotalResearchScore;

    // Tile Counts

    public List<HexCell> empireCells = new List<HexCell>();
    public List<HexCell> Urban = new List<HexCell>();
    public List<HexCell> farms = new List<HexCell>();
    public List<HexCell> Cities = new List<HexCell>();
    public List<HexCell> Temples = new List<HexCell>();

    public List<HexUnit> units = new List<HexUnit>();
    public List<GameObject> dipEmpires = new List<GameObject>();

    // AI 
    public List<string> knownEmpires = new List<string>();
    public List<EmpireIntel> empireIntelList = new List<EmpireIntel>();
    public List<HexCell> exploredCells = new List<HexCell>();

    void Start()
    {
        treasury = 2000f;
        manpower = 8000f;
        stability = 0f;

        TotalResearchScore = 0;

        labels = new float[3] { treasury, manpower, stability };
        resources = new int[6] { 0, 0, 0, 0, 0, 0 };

        CreateBase();
        updateLabels();
        SetupResearch();

    }

    void Update()
    {
        if (status != "Capitulated")
        {
            HighlightEmpire(true);
            Scout();
        }
        else
            HighlightEmpire(false);
    }

    public void AutoExpand()
    {
        List<HexCell> expandableAreas = new List<HexCell>();

        foreach (HexCell cell in empireCells)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                if (cell.GetNeighbor(d) != null && cell.GetNeighbor(d).owner == null)
                {
                    expandableAreas.Add(cell.GetNeighbor(d));
                    if (!exploredCells.Contains(cell.GetNeighbor(d)))
                        exploredCells.Add(cell.GetNeighbor(d));
                }
            }
        }

        HexCell random = expandableAreas[Random.Range(0, expandableAreas.Count - 1)];

        if (random.SpecialIndex == 1)
            Cities.Add(random);

        random.owner = transform.gameObject;
        empireCells.Add(random);


        expandableAreas.Clear();
    }

    void Scout()
    {
        foreach (HexCell cell in exploredCells)
        {
            if (cell.Unit != null && cell.Unit.ParentEmpire.GetComponent<Empire>().empireName != empireName)
            {
                if (!knownEmpires.Contains(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName))
                {
                    dipEmpires.Add(cell.Unit.ParentEmpire);
                    CreateEmpireIntel(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName);
                }

                if (!UpdateIntel(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName).knownUnits.Contains(cell.Unit))
                    UpdateIntel(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName).knownUnits.Add(cell.Unit);

            }

            if (cell.owner != null && cell.owner.GetComponent<Empire>().empireName != empireName)
            {
                if (!knownEmpires.Contains(cell.owner.GetComponent<Empire>().empireName))
                {
                    dipEmpires.Add(cell.owner);
                    CreateEmpireIntel(cell.owner.GetComponent<Empire>().empireName);
                }

                if (!UpdateIntel(cell.owner.GetComponent<Empire>().empireName).knownTerritories.Contains(cell))
                    UpdateIntel(cell.owner.GetComponent<Empire>().empireName).knownTerritories.Add(cell);
            }

        }
    }


    public void ResetEmpire()
    {
        foreach (HexCell cell in empireCells)
        {
            cell.owner = null;
            cell.UrbanLevel = 0;
            cell.FarmLevel = 0;
            if (cell.SpecialIndex != 1)
                cell.SpecialIndex = 0;
        }

        foreach (HexUnit unit in units)
            hexGrid.RemoveUnit(unit);

        empireCells.Clear();
        exploredCells.Clear();

        Urban.Clear();
        farms.Clear();
        Cities.Clear();
        Temples.Clear();
        units.Clear();
        knownEmpires.Clear();
        dipEmpires.Clear();

        AtWar.Clear();
        AtPeace.Clear();
        AlliedWith.Clear();
        NAPWith.Clear();
        IRWith.Clear();
        MAWith.Clear();

        treasury = 0;
        manpower = 0;
        stability = 0;
        soldiers = 0;
        workers = 0;

        PreWarSize = 0;
        TotalResearchScore = 0;
    }

    public void RefreshDiplomacy()
    {
        foreach (GameObject empire in dipEmpires)
        {
            // Remove capitulated empires
            if (empire.GetComponent<Empire>().status == "Capitulated")
            {
                AtWar.Remove(empire.GetComponent<Empire>().empireName);

                if (AtWar.Count == 0)
                    status = "At Peace";

                empireIntelList.Remove(UpdateIntel(empire.GetComponent<Empire>().empireName));
                knownEmpires.Remove(empire.GetComponent<Empire>().empireName);
            }
            else
            {
                UpdateIntel(empire.GetComponent<Empire>().empireName).CalculateStats();
                UpdateIntel(empire.GetComponent<Empire>().empireName).calculateRelations(transform.gameObject.GetComponent<Empire>());
                UpdateIntel(empire.GetComponent<Empire>().empireName).AtPeace = empire.GetComponent<Empire>().AtPeace;
                UpdateIntel(empire.GetComponent<Empire>().empireName).AtWar = empire.GetComponent<Empire>().AtWar;
                UpdateIntel(empire.GetComponent<Empire>().empireName).AlliedWith = empire.GetComponent<Empire>().AlliedWith;
                UpdateIntel(empire.GetComponent<Empire>().empireName).MAWith = empire.GetComponent<Empire>().MAWith;
                UpdateIntel(empire.GetComponent<Empire>().empireName).NAPWith = empire.GetComponent<Empire>().NAPWith;
                UpdateIntel(empire.GetComponent<Empire>().empireName).TotalResearchScore = TotalResearchScore + Random.Range(-1, 2);
                UpdateIntel(empire.GetComponent<Empire>().empireName).resources = empire.GetComponent<Empire>().resources;
            }

        }
    }

    public void CreateEmpireIntel(string name)
    {
        EmpireIntel rival = new EmpireIntel
        {
            empireName = name
        };

        knownEmpires.Add(name);
        empireIntelList.Add(rival);
    }

    public EmpireIntel UpdateIntel(string name)
    {
        foreach (EmpireIntel empire in empireIntelList)
        {
            if (empire.empireName.Equals(name))
            {
                return empire;
            }
        }

        return null;
    }

    public void UpdateResources()
    {
        treasury += 50 * empireCells.Count + Urban.Count * 100;
        manpower += 50 * empireCells.Count + farms.Count * 100;
        stability += (int)(manpower + treasury - empireCells.Count * 1000) / 1000 + Temples.Count;
        ApplyBuffs();

    }

    public void ResetActions()
    {
        foreach (HexUnit unit in units)
        {
            unit.Actions = 3;
        }
    }

    void ApplyBuffs()
    {
        ApplyBuffsToUnits();

        if (treeConsturction == "Left")
            manpower += farms.Count * consturctionBuffs[0];

        if (treeConsturction == "Middle")
            stability += Urban.Count * consturctionBuffs[0];

        if (treeConsturction == "Right")
            stability += Urban.Count * consturctionBuffs[0];


        if (treeDoctrine == "Left")
        {
            stability += (doctrineBuffs[0] / 10) * 2;
            treasury -= 50 * soldiers * doctrineBuffs[1];
        }

        if (treeDoctrine == "Middle")
        {
            manpower += doctrineBuffs[0] * 250;
            stability -= (soldiers + workers + ranged) * doctrineBuffs[1] / 10;
        }

        if (treeDoctrine == "Right")
        {
            stability += doctrineBuffs[0] / 10;
            manpower -= (soldiers + workers + ranged) * doctrineBuffs[1] * 10;
        }

    }

    public void ApplyBuffsToUnits()
    {
        foreach (HexUnit a in units)
        {
            a.ApplyBuffs();
        }
    }

    public void ResetExplored()
    {
        foreach (HexCell cell in exploredCells)
        {
            cell.IsExplored = activated;
        }

        // UPDATING TOTAL SCORE

        totalScore = units.Count + empireCells.Count + TotalResearchScore + empireIntelList.Count + farms.Count + Temples.Count
            + Cities.Count + Urban.Count + AlliedWith.Count + MAWith.Count + NAPWith.Count;

    }

    public void updateLabels()
    {
        labels[0] = treasury;
        labels[1] = manpower;
        labels[2] = stability;
        GameObject stats = GameObject.Find("Stats");

        int i = 0;
        foreach (Transform child in stats.transform)
        {
            child.GetComponentInChildren<Text>().text = labels[i].ToString();
            i++;
        }

        GameObject Rcs = GameObject.Find("Resources").transform.GetChild(1).gameObject;


        i = 1;
        foreach (Transform child in Rcs.transform)
        {
            child.GetComponentInChildren<Text>().text = resources[i].ToString();
            i++;
        }

    }

    void CreateBase()
    {
        empireCells.Add(Capital);
        Cities.Add(Capital);
        Capital.owner = transform.gameObject;
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            if (Capital.GetNeighbor(d) != null)
            {
                empireCells.Add(Capital.GetNeighbor(d));
                Capital.GetNeighbor(d).owner = transform.gameObject;
            }
        }

        foreach (HexCell cell in empireCells)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                if (cell.Explorable)
                {
                    cell.IsExplored = true;
                    exploredCells.Add(cell.GetNeighbor(d));
                }

            }

            exploredCells.Add(cell);
        }

    }

    void HighlightEmpire(bool key)
    {
        foreach (HexCell cell in empireCells)
        {
            if (cell.visibility > 0 && key)
                cell.EnableEmpireHighlight(empireColor);
            else
                cell.DisableEmpireHighlight();
        }
    }

    public HexCell GetCapital()
    {
        return Capital;
    }

    void SetupResearch()
    {
        foreach (GameObject a in UI.GetComponent<ResearchTab>().ResearchTabs)
        {
            ResearchSubTab tab = new ResearchSubTab();

            if (isPlayerEmpire)
            {
                tab.LeftTree = UI.GetComponent<ResearchTab>().CreateButtonList(a.transform.GetChild(1).GetChild(0).gameObject);
                tab.rightTree = UI.GetComponent<ResearchTab>().CreateButtonList(a.transform.GetChild(2).GetChild(0).gameObject);

                if (a.transform.childCount > 4)
                    tab.middleTree = UI.GetComponent<ResearchTab>().CreateButtonList(a.transform.GetChild(3).GetChild(0).gameObject);
            }


            tab.indexL = 0;
            tab.indexM = 0;
            tab.indexR = 0;
            tab.ActiveTurn = 0;
            tab.TabName = a.name;

            SubTabs.Add(tab);

        }
    }

    public void GetBuffs()
    {
        //Debug.Log(SubTabs.Count);
        treeBoat = SubTabs[0].Tree;
        treeSoldier = SubTabs[1].Tree;
        treeConsturction = SubTabs[2].Tree;
        treeDoctrine = SubTabs[3].Tree;

        // fleet
        if (extractBuffs(SubTabs[0]) != null)
            boatrBuffs = extractBuffs(SubTabs[0]);

        // construction
        if (extractBuffs(SubTabs[1]) != null)
            consturctionBuffs = extractBuffs(SubTabs[1]);

        //Army
        if (extractBuffs(SubTabs[2]) != null)
            soliderBuffs = extractBuffs(SubTabs[2]);

        // Doctrine
        if (extractBuffs(SubTabs[3]) != null)
            doctrineBuffs = extractBuffs(SubTabs[3]);

    }

    List<int> extractBuffs(ResearchSubTab tab)
    {
        if (tab.indexL > 0)
        {
            List<int> buff = new List<int>();
            buff.Add(10 * tab.indexL);
            buff.Add(10 * tab.indexL);

            TotalResearchScore += tab.indexL;

            return buff;
        }

        if (tab.indexR > 0)
        {
            List<int> buff = new List<int>();
            buff.Add(10 * tab.indexR);
            buff.Add(10 * tab.indexR);

            TotalResearchScore += tab.indexR;

            return buff;
        }

        if (tab.indexM > 0)
        {
            List<int> buff = new List<int>();
            buff.Add(10 * tab.indexM);
            buff.Add(10 * tab.indexM);

            TotalResearchScore += tab.indexM;

            return buff;
        }

        return null;
    }

}

