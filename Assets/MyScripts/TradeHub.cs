using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeHub : MonoBehaviour
{
    
    public Slider[] slidersR;
    public Slider[] slidersC;

    public GameObject resources;
    public GameObject currencies;

    public GameObject resources_ai;
    public GameObject currencies_ai;

    public Text title;

    public GameObject tradeBtn;
    public Text ErrorTrd;

    int[] RequestResources = new int[6] { 0, 0, 0, 0, 0, 0 };

    int reqCounter = 0;
    int demandCounter = 0;

    public GameObject manager;
    public GameObject CurrentEmpire;

   public void UpdateLabels()
    {
        for (int i = 0; i < resources.transform.childCount; i++)
        {
            resources.transform.GetChild(i).GetComponentInChildren<Text>().text = ((int)slidersR[i].value).ToString();
        }

        for (int i = 0; i < currencies.transform.childCount; i++)
        {
            currencies.transform.GetChild(i).GetComponentInChildren<Text>().text = ((int)slidersC[i].value).ToString();
        }
    }

    public void SetGoods()
    {
        title.text = "Trade with " + CurrentEmpire.GetComponent<Empire>().empireName;

        int i = 1;
        foreach (Slider s in slidersR)
        {
            if (CurrentEmpire.GetComponent<Empire>().resources[i] > 0)
            {
                s.transform.parent.gameObject.SetActive(true);
                s.maxValue = CurrentEmpire.GetComponent<Empire>().resources[i];
                s.value = 0;
            }

            i++;
        }

        if (CurrentEmpire.GetComponent<Empire>().treasury > 0)
        {
            slidersC[0].transform.parent.gameObject.SetActive(true);
            slidersC[0].maxValue = CurrentEmpire.GetComponent<Empire>().treasury;
            slidersC[0].value = 0;
        }

        if (CurrentEmpire.GetComponent<Empire>().manpower > 0)
        {
            slidersC[1].transform.parent.gameObject.SetActive(true);
            slidersC[1].maxValue = CurrentEmpire.GetComponent<Empire>().manpower;
            slidersC[1].value = 0;
        }
    }

    public void Evaluate()
    {
        reqCounter = 0;
        demandCounter = 0;

        foreach (Slider s in slidersR)
        {
            if (s.value > 0)
                reqCounter++;
        }

        foreach (Slider s in slidersC)
        {
            if (s.value > 0)
                reqCounter++;
            
        }
        buildResourceArray();

        if (checkAvailableResources() && manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().treasury > 
            (reqCounter - demandCounter) * 200 && manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().manpower > (reqCounter - demandCounter) * 200)
        {
            tradeBtn.SetActive(true);

            for (int i = 0; i < resources_ai.transform.childCount; i++)
            {
                resources_ai.transform.GetChild(i).GetComponentInChildren<Text>().text = RequestResources[i + 1].ToString();
            }

            if (reqCounter > demandCounter)
            {
                currencies_ai.transform.GetChild(0).GetComponentInChildren<Text>().text = ((reqCounter - demandCounter) * 200).ToString();
                currencies_ai.transform.GetChild(1).GetComponentInChildren<Text>().text = ((reqCounter - demandCounter) * 200).ToString();
            }
            else
            {
                if (manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().treasury >= 1000 && manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().manpower >= 1000)
                {
                    currencies_ai.transform.GetChild(0).GetComponentInChildren<Text>().text = "1000";
                    currencies_ai.transform.GetChild(1).GetComponentInChildren<Text>().text = "1000";
                }
                else
                    ErrorTrd.gameObject.SetActive(true);

            }
        }
        else
            ErrorTrd.gameObject.SetActive(true);
        

    }

    public void Trade()
    {
        int i = 1;
        foreach (Slider s in slidersR)
        {
            CurrentEmpire.GetComponent<Empire>().resources[i] -= (int)s.value;
            manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().resources[i] += (int)s.value;

            CurrentEmpire.GetComponent<Empire>().resources[i] += RequestResources[i];
            manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().resources[i] -= RequestResources[i];

            i++;
        }

        CurrentEmpire.GetComponent<Empire>().treasury -= slidersC[0].value;
        manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().treasury += slidersC[0].value;

        CurrentEmpire.GetComponent<Empire>().manpower -= slidersC[1].value;
        manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().manpower += slidersC[1].value;

        if (manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().treasury > (reqCounter - demandCounter) * 200 && manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().manpower > (reqCounter - demandCounter) * 200)
        {
            if (reqCounter > demandCounter)
            {
                CurrentEmpire.GetComponent<Empire>().treasury += (reqCounter - demandCounter) * 200;
                manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().treasury -= (reqCounter - demandCounter) * 200;
                CurrentEmpire.GetComponent<Empire>().manpower += (reqCounter - demandCounter) * 200;
                manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().manpower -= (reqCounter - demandCounter) * 200;
            }
            else
            {
                CurrentEmpire.GetComponent<Empire>().treasury += 1000;
                manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().treasury -= 1000;
                CurrentEmpire.GetComponent<Empire>().manpower += 1000;
                manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().manpower -= 1000;
            }
        }
        

        manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().updateLabels();

        ResetTrade();

    }

    void buildResourceArray()
    {
        int i = 1;
        for (int j = 1; j< CurrentEmpire.GetComponent<Empire>().resources.Length; j++)
        {
            if (CurrentEmpire.GetComponent<Empire>().resources[j] == 0 && manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().resources[i] > 0)
            {
                RequestResources[i] = manager.GetComponent<GameManager>().GetPlayerEmpire().GetComponent<Empire>().resources[i];
                demandCounter++;
            }
            i++;
        }
    }

    bool checkAvailableResources()
    {
        if (slidersC[0].value / slidersC[0].maxValue < reqCounter / 5)
            return false;

        if (slidersC[1].value / slidersC[1].maxValue < reqCounter / 5)
            return false;

        int i = 1;
        foreach (Slider s in slidersR)
        {
            if (s.value > CurrentEmpire.GetComponent<Empire>().resources[i] / 2)
                return false;
            
            i++;
        }

        return true;
    }

    public void ResetTrade()
    {
        reqCounter = 0;
        demandCounter = 0;

        foreach (Slider s in slidersR)
        {
            s.value = 0;
        }

        slidersC[0].value = 0;
        slidersC[1].value = 0;

        ErrorTrd.gameObject.SetActive(false);
        tradeBtn.SetActive(false);

        GameObject.Find("TradeHub").transform.GetChild(0).gameObject.SetActive(false);
        Time.timeScale = 1f;

    }

}
