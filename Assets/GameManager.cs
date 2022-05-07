using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Map hexGrid;

    static List<string> EmpireNames = new List<string>() { "Rome", "Carthage", "Egypt" , "Ghaul", "Macedon", "Arevaci" };

    GameObject [] empires = new GameObject[EmpireNames.Count];

    public Sprite[] banners = new Sprite[EmpireNames.Count];
    public Sprite[] backgrounds = new Sprite[EmpireNames.Count];
    public Sprite[] UnitProfiles = new Sprite[EmpireNames.Count];
    public Sprite[] empireLogos = new Sprite[EmpireNames.Count];
    public Sprite[] BoatProfiles = new Sprite[EmpireNames.Count];
    public Color[] empireColors = new Color[EmpireNames.Count];

    public Sprite workerProfile;
    public Sprite catapultProfile;
    public Sprite settlerProfile;

    public AudioClip[] Moves = new AudioClip[12];
    public AudioClip[] Selection = new AudioClip[12];
    public AudioClip[] Worker = new AudioClip[2];
    public AudioClip[] Misc = new AudioClip[2];
    public AudioClip[] Abilities = new AudioClip[3];
    public AudioClip[] Clicks = new AudioClip[3];
    public AudioClip SoundTrack;

    public List<GameObject> icons = new List<GameObject>();

    // CITY INDEXES : 22 rome, 10 carthage ,2 egypt, 36 ghaul, 23 macedon, 28 arevaci

    int[] playerCities = { 22, 10, 2 , 36, 23, 28 };

    public GameObject empirePrefab;
    public int year;
    public int Turn;
    public string playerEmpireName;

    public GameObject manager;
    public GameObject capitalIcon;
    public GameObject recruitToggle;

    public GameObject playerReport;
    public GameObject scorePrefab;
    
    private AudioSource[] allAudioSources;

    
    void Awake()
    {
        allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach (AudioSource audioS in allAudioSources)
        {
            audioS.Stop();
        }
    }
    
    void Start()
    {
        playerEmpireName = PlayerEmpireName.playerEmpireName;
        //playerEmpireName = "Rome";
        year = 250;
        Turn = 0;
        updateTurn();
        SetEmpires();
        transform.gameObject.GetComponent<AudioSource>().clip = SoundTrack;
        transform.gameObject.GetComponent<AudioSource>().Play();
    }

    void Update()
    {
        LightEmpire();
        CheckCapitualtion();
        if (!GetPlayerEmpire().GetComponent<Empire>().activated)
            AIturn();
        
    }

    void LightEmpire()
    {
        foreach (HexCell cell in GetPlayerEmpire().GetComponent<Empire>().empireCells)
        {
            //if (cell.visibility == 0)
                hexGrid.IncreaseVisibility(cell, 3);
        }
    }

    void GameReport()
    {
        Time.timeScale = 0f;
        playerReport.gameObject.SetActive(true);
        BuildEmpireList();
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;
        playerReport.gameObject.SetActive(false);
    }

    void BuildEmpireList()
    {
        List<Empire> scoreList = new List<Empire>();

        foreach (GameObject empire in empires)
        {
            scoreList.Add(empire.GetComponent<Empire>());
        }

        scoreList = scoreList.OrderByDescending(o => o.totalScore).ToList();

        Transform content = playerReport.transform.GetChild(3).GetChild(0).GetChild(0);

        foreach(Empire empire in scoreList)
        {
            GameObject EmpireTab = Instantiate(scorePrefab , content);
            EmpireTab.transform.GetChild(2).GetComponent<Image>().sprite = empireLogos[empire.EmpireIndex];
            EmpireTab.transform.GetChild(3).GetComponent<Text>().text = empire.empireName;
            EmpireTab.transform.GetChild(4).GetComponent<Text>().text = empire.empireCells.Count.ToString();
            EmpireTab.transform.GetChild(5).GetComponent<Text>().text = empire.units.Count.ToString();
            EmpireTab.transform.GetChild(6).GetComponent<Text>().text = empire.status;
            EmpireTab.transform.GetChild(7).GetComponent<Text>().text = empire.totalScore.ToString();
        }

        if (year != -250)
        {
            playerReport.transform.GetChild(4).gameObject.SetActive(true);
            playerReport.transform.GetChild(5).gameObject.SetActive(true);
        }
        else
        {
            playerReport.transform.GetChild(4).gameObject.SetActive(false);
            playerReport.transform.GetChild(5).gameObject.SetActive(true);
        }

    }

    void SetEmpires()
    {
        for (int i = 0; i < EmpireNames.Count; i++)
        {
            GameObject empire = Instantiate(empirePrefab);
            empires[i] = empire;
            empires[i].GetComponent<Empire>().empireName = EmpireNames[i];
            empires[i].GetComponent<Empire>().EmpireIndex = i;
            empires[i].GetComponent<Empire>().empireColor = empireColors[i];
            empires[i].GetComponent<Empire>().Capital = hexGrid.cities[playerCities[i]];
            empires[i].GetComponent<Empire>().status = "At Peace";
            empires[i].GetComponent<Empire>().UI = GameObject.Find("UI").gameObject;
            SetPeace(empires[i]);
            if (empires[i].GetComponent<Empire>().empireName == playerEmpireName)
            {
                SetPlayerEmpireUI(empires[i].GetComponent<Empire>().EmpireIndex);
                empires[i].GetComponent<Empire>().activated = true;
                empires[i].GetComponent<Empire>().isPlayerEmpire = true;
                empires[i].GetComponent<AI>().enabled = false;
                manager.GetComponent<UIManager>().selectedUnit = null;

            }
            else
            {
                empires[i].GetComponent<Empire>().activated = false;
            }
            
        }

    }

    void CheckCapitualtion()
    {
        foreach (GameObject empire in empires)
        {
            if (empire.GetComponent<Empire>().AtWar.Count > 0 &&
                (empire.GetComponent<Empire>().empireCells.Count <= 0.3 * empire.GetComponent<Empire>().PreWarSize || empire.GetComponent<Empire>().Capital.owner != empire && empire.GetComponent<Empire>().empireCells.Count <= 0.5))
                empire.GetComponent<Empire>().status = "Capitulated"; 
        }
        
    }

    void SetPeace(GameObject empire)
    {
        foreach (string name in EmpireNames)
        {
            if (name != empire.GetComponent<Empire>().empireName)
            {
                empire.GetComponent<Empire>().AtPeace.Add(name);
            }
        }
    }

    public void NextTurn()
    {
        year -= 5;
        updateTurn();
        GetPlayerEmpire().GetComponent<Empire>().activated = false;
        GetPlayerEmpire().GetComponent<Empire>().ResetExplored();
        manager.GetComponent<UIManager>().currentSpawn = null;
        ActivateUI(false);
    }

    void ActivateUI(bool key)
    {
        manager.transform.GetChild(1).GetChild(2).gameObject.SetActive(key); ;

    }

    void AIturn()
    {
        for (int i = 0; i < 2; i++)//EmpireNames.Count; i++)
        {
            if (empires[i] !=  GetPlayerEmpire() && empires[i].GetComponent<Empire>().status != "Capitulated")
            {
                empires[i].GetComponent<Empire>().ResetActions();
                empires[i].GetComponent<Empire>().UpdateResources();
                empires[i].GetComponent<Empire>().AutoExpand();
                empires[i].GetComponent<Empire>().activated = true;
                empires[i].GetComponent<Empire>().GetBuffs();
                empires[i].GetComponent<Empire>().RefreshDiplomacy();
                empires[i].GetComponent<Empire>().ResetExplored();
                PlayAI(empires[i]);
                empires[i].GetComponent<Empire>().activated = false;
                empires[i].GetComponent<Empire>().ResetExplored();
            }

            if (empires[i].GetComponent<Empire>().status.Equals("Capitulated"))
            {
                empires[i].GetComponent<Empire>().ResetEmpire();
            }
                
        }
        
        GetPlayerEmpire().GetComponent<Empire>().activated = true;
        GetPlayerEmpire().GetComponent<Empire>().AutoExpand();
        GetPlayerEmpire().GetComponent<Empire>().ResetExplored();
        GetPlayerEmpire().GetComponent<Empire>().RefreshDiplomacy();
        ActivateUI(true);
        Turn += 1;
        GetPlayerEmpire().GetComponent<Empire>().ResetActions();
        GetPlayerEmpire().GetComponent<Empire>().GetBuffs();
        GetPlayerEmpire().GetComponent<Empire>().UpdateResources();
        GetPlayerEmpire().GetComponent<Empire>().updateLabels();

        if (year == 0 || year == -250)
            GameReport();
    }
    

    void PlayAI(GameObject empire)
    {
        Debug.Log(empire.GetComponent<Empire>().empireName);
        empire.GetComponent<AI>().currenEmpire = empire.GetComponent<Empire>();
        empire.GetComponent<AI>().gameManager = transform.gameObject;
        empire.GetComponent<AI>().Grid = hexGrid;
        empire.GetComponent<AI>().Init();
    }

    public GameObject GetCurrentActive()
    {
        foreach (GameObject empire in empires)
        {
            if (empire.GetComponent<Empire>().activated)
                return empire;
        }

        return null;
    }
    
    void updateTurn()
    {
        GameObject obj = GameObject.Find("Year");

        if (year > 0)
            obj.GetComponentInChildren<Text>().text = year.ToString() + " BC";
        else
            obj.GetComponentInChildren<Text>().text = (-year).ToString() + " AD";
    }

    public void SetPlayerEmpire(string name)
    {
        playerEmpireName = name;
    }


    public GameObject GetPlayerEmpire()
    {
        return empires[EmpireNames.IndexOf(playerEmpireName)];
    }

    void SetPlayerEmpireUI(int index)
    {
        
        Transform buttons = manager.transform.GetChild(1).GetChild(9).GetChild(0);

        foreach (Transform child in buttons.transform)
        {
            child.GetComponent<Image>().color = empireColors[index];
        }

        GameObject.Find("Bottom").transform.GetChild(0).GetComponent<Image>().color = empireColors[index];
        GameObject.Find("EmpireBanner").GetComponent<Image>().sprite = banners[index];

    }

    public GameObject[] ReturnEmpires()
    {
        return empires;
    }

    public void UpdateIcons()
    {
        if (recruitToggle.GetComponent<Toggle>().isOn)
        {
            foreach (HexCell city in GetPlayerEmpire().GetComponent<Empire>().Cities)
            {
                GameObject capital = Instantiate(capitalIcon);
                Vector3 pos = new Vector3(city.Position.x, 12, city.Position.z);
                capital.transform.position = pos;
                capital.GetComponent<EmpireCity>().city = city;
                icons.Add(capital);

            }
        }

        if(!recruitToggle.GetComponent<Toggle>().isOn)
        {
            manager.GetComponent<UIManager>().currentSpawn = null;

            foreach (GameObject icon in icons)
            {
                Destroy(icon);
            }

            icons.Clear();
        }
        
    }
}
