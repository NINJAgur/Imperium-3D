using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DiplomacyTab : MonoBehaviour
{
    public GameObject tabDiplomacy;
    public GameObject tabActions;
    public GameObject buttonEmpire;
    public GameManager manager;

    public GameObject CurrentEmpire;

    private void ClearDipList()
    {
        Transform content = tabDiplomacy.transform.GetChild(3).GetChild(0).GetChild(0);
        if (content.childCount > 0)
            foreach (Transform a in content) Destroy(a.gameObject);
    }

    public void OnDipListUpdate()
    {
        CurrentEmpire = null;
        ClearDipList();

        Transform content = tabDiplomacy.transform.GetChild(3).GetChild(0).GetChild(0);

        foreach (GameObject empire in manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().dipEmpires)
        {
            if (empire.GetComponent<Empire>().status != "Capitulated")
            {
                GameObject newDipButton = Instantiate(buttonEmpire, content);

                newDipButton.transform.Find("Profile").GetComponent<Image>().sprite = manager.GetComponent<GameManager>().empireLogos[empire.GetComponent<Empire>().EmpireIndex];
                newDipButton.transform.Find("Status").GetComponent<Text>().text = "Relations : " + (empire.GetComponent<Empire>().UpdateIntel(manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().empireName).GetScore()).ToString();
                
                newDipButton.GetComponentInChildren<Button>().onClick.AddListener(delegate { ActionsTab(empire); });
            }
        }

    }

    public void ActionsTab(GameObject empire)
    {
        tabDiplomacy.SetActive(false);
        tabActions.SetActive(true);
        CurrentEmpire = empire;
        SetUpTabActions();
    }

    void SetUpTabActions()
    {
        tabActions.transform.GetChild(1).GetComponent<Text>().text = CurrentEmpire.GetComponent<Empire>().empireName;
        tabActions.transform.GetChild(2).GetComponent<Image>().sprite = manager.GetComponent<GameManager>().empireLogos[CurrentEmpire.GetComponent<Empire>().EmpireIndex];
        tabActions.transform.GetChild(3).GetComponent<Text>().text = "Relations : " + (CurrentEmpire.GetComponent<Empire>().UpdateIntel(manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().empireName).GetScore()).ToString();
        if (CurrentEmpire.GetComponent<Empire>().AtWar.Contains(manager.GetPlayerEmpire().GetComponent<Empire>().empireName))
        {
            tabActions.transform.GetChild(4).GetChild(0).GetComponentInChildren<Text>().text = "OFFER PEACE";
            tabActions.transform.GetChild(4).GetChild(1).gameObject.SetActive(false);
            tabActions.transform.GetChild(4).GetChild(2).gameObject.SetActive(false);
            tabActions.transform.GetChild(4).GetChild(3).gameObject.SetActive(false);
            tabActions.transform.GetChild(4).GetChild(4).gameObject.SetActive(false);
        }
        else
        {
            tabActions.transform.GetChild(4).GetChild(0).GetComponentInChildren<Text>().text = "DECLARE WAR";
            tabActions.transform.GetChild(4).GetChild(1).gameObject.SetActive(true);
            tabActions.transform.GetChild(4).GetChild(2).gameObject.SetActive(true);
            tabActions.transform.GetChild(4).GetChild(3).gameObject.SetActive(true);
            tabActions.transform.GetChild(4).GetChild(4).gameObject.SetActive(true);
        }

    }

    public void Status()
    {
        if (CurrentEmpire.GetComponent<Empire>().AtPeace.Contains(manager.GetPlayerEmpire().GetComponent<Empire>().empireName))
        {
            // Declared War

            CurrentEmpire.GetComponent<Empire>().AtWar.Add(manager.GetPlayerEmpire().GetComponent<Empire>().empireName);
            CurrentEmpire.GetComponent<Empire>().PreWarSize = CurrentEmpire.GetComponent<Empire>().empireCells.Count;
            CurrentEmpire.GetComponent<Empire>().AtPeace.Remove(manager.GetPlayerEmpire().GetComponent<Empire>().empireName);

            CurrentEmpire.GetComponent<Empire>().status = "At War";

            manager.GetPlayerEmpire().GetComponent<Empire>().AtWar.Add(CurrentEmpire.GetComponent<Empire>().empireName);
            manager.GetPlayerEmpire().GetComponent<Empire>().PreWarSize = manager.GetPlayerEmpire().GetComponent<Empire>().empireCells.Count;
            manager.GetPlayerEmpire().GetComponent<Empire>().AtPeace.Remove(CurrentEmpire.GetComponent<Empire>().empireName);

            manager.GetPlayerEmpire().GetComponent<Empire>().status = "At War";

        }

        else
        {
            // Offered Peace

            if (CurrentEmpire.GetComponent<Empire>().Capital.owner != CurrentEmpire)
            {
                manager.GetPlayerEmpire().GetComponent<Empire>().Capital = manager.GetPlayerEmpire().GetComponent<Empire>().Cities[Random.Range(0, manager.GetPlayerEmpire().GetComponent<Empire>().Cities.Count - 1)];
            }

            CurrentEmpire.GetComponent<Empire>().AtPeace.Add(manager.GetPlayerEmpire().GetComponent<Empire>().empireName);
            CurrentEmpire.GetComponent<Empire>().AtWar.Remove(manager.GetPlayerEmpire().GetComponent<Empire>().empireName);

            if (CurrentEmpire.GetComponent<Empire>().AtWar.Count == 0)
            {
                CurrentEmpire.GetComponent<Empire>().PreWarSize = 0;
                CurrentEmpire.GetComponent<Empire>().status = "At Peace";
            }

            manager.GetPlayerEmpire().GetComponent<Empire>().AtPeace.Add(CurrentEmpire.GetComponent<Empire>().empireName);
            manager.GetPlayerEmpire().GetComponent<Empire>().AtWar.Remove(CurrentEmpire.GetComponent<Empire>().empireName);

            if (manager.GetPlayerEmpire().GetComponent<Empire>().AtWar.Count > 0)
            {
                manager.GetPlayerEmpire().GetComponent<Empire>().PreWarSize = 0;
                manager.GetPlayerEmpire().GetComponent<Empire>().status = "At Peace";

            }
        }
            
    }

    #region Setters and removers

    public void SetIR()
    {
        if (!CurrentEmpire.GetComponent<Empire>().IRWith.Contains(manager.GetPlayerEmpire().GetComponent<Empire>().empireName))
        {
            CurrentEmpire.GetComponent<Empire>().IRWith.Add(manager.GetPlayerEmpire().GetComponent<Empire>().empireName);
            manager.GetPlayerEmpire().GetComponent<Empire>().IRWith.Add(CurrentEmpire.GetComponent<Empire>().empireName);
        }
            
    }

    public void SetNAP()
    {
        if (!CurrentEmpire.GetComponent<Empire>().NAPWith.Contains(manager.GetPlayerEmpire().GetComponent<Empire>().empireName))
        {
            if (CurrentEmpire.GetComponent<Empire>().UpdateIntel(manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().empireName).GetScore() > 50)
            {
                CurrentEmpire.GetComponent<Empire>().NAPWith.Add(manager.GetPlayerEmpire().GetComponent<Empire>().empireName);
                manager.GetPlayerEmpire().GetComponent<Empire>().NAPWith.Add(CurrentEmpire.GetComponent<Empire>().empireName);
            }
        }
            
    }

    public void SetMA()
    {
        if (!CurrentEmpire.GetComponent<Empire>().MAWith.Contains(manager.GetPlayerEmpire().GetComponent<Empire>().empireName))
        {
            if (CurrentEmpire.GetComponent<Empire>().UpdateIntel(manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().empireName).GetScore() > 30)
            {
                CurrentEmpire.GetComponent<Empire>().MAWith.Add(manager.GetPlayerEmpire().GetComponent<Empire>().empireName);
                manager.GetPlayerEmpire().GetComponent<Empire>().MAWith.Add(CurrentEmpire.GetComponent<Empire>().empireName);
            }

        }

    }

    public void RemoveIR()
    {
        if (CurrentEmpire.GetComponent<Empire>().IRWith.Contains(manager.GetPlayerEmpire().GetComponent<Empire>().empireName))
        {
            CurrentEmpire.GetComponent<Empire>().IRWith.Remove(manager.GetPlayerEmpire().GetComponent<Empire>().empireName);
            manager.GetPlayerEmpire().GetComponent<Empire>().IRWith.Remove(CurrentEmpire.GetComponent<Empire>().empireName);
        }

    }

    public void RemoveNAP()
    {
        if (CurrentEmpire.GetComponent<Empire>().NAPWith.Contains(manager.GetPlayerEmpire().GetComponent<Empire>().empireName))
        {
            CurrentEmpire.GetComponent<Empire>().NAPWith.Remove(manager.GetPlayerEmpire().GetComponent<Empire>().empireName);
            manager.GetPlayerEmpire().GetComponent<Empire>().NAPWith.Remove(CurrentEmpire.GetComponent<Empire>().empireName);
        }

    }

    public void RemoveMA()
    {
        if (CurrentEmpire.GetComponent<Empire>().MAWith.Contains(manager.GetPlayerEmpire().GetComponent<Empire>().empireName))
        {
            CurrentEmpire.GetComponent<Empire>().MAWith.Remove(manager.GetPlayerEmpire().GetComponent<Empire>().empireName);
            manager.GetPlayerEmpire().GetComponent<Empire>().MAWith.Remove(CurrentEmpire.GetComponent<Empire>().empireName);

        }

    }

    #endregion

}
