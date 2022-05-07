using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchTab : MonoBehaviour
{

    GameObject CurrentTabResearch;
    ResearchSubTab CurrentSubTabResearch;

    public GameManager manager;
    
    public List<GameObject> ResearchTabs;

    int[] timers = new int[4] {0, 3, 5, 7 };

    public void SetUp()
    {
        updateCurrentActive();

    }

    public List<GameObject> CreateButtonList(GameObject parent)
    {
        List<GameObject> buttonList = new List<GameObject>();

        foreach(RectTransform button in parent.transform)
        {
            buttonList.Add(button.gameObject);
        }

        return buttonList;
    }

    void FindActive()
    {
        foreach (GameObject a in ResearchTabs)
        {
            if (a.activeSelf)
            {
                CurrentTabResearch = a;
            }
                
        }
    }

    public void updateCurrentActive()
    {
        FindActive();

        foreach (ResearchSubTab tab in manager.GetPlayerEmpire().GetComponent<Empire>().SubTabs)
        {
            if (tab.TabName == CurrentTabResearch.name)
            {
                CurrentSubTabResearch = tab;
                CurrentSubTabResearch.UpdateLists();
            }
        }
    }

    public void updateL()
    {
        if ((manager.GetComponent<GameManager>().Turn - CurrentSubTabResearch.ActiveTurn) == timers[CurrentSubTabResearch.indexL])
        {
            CurrentSubTabResearch.ActiveTurn = manager.GetComponent<GameManager>().Turn;

            CurrentSubTabResearch.Tree = "Left";

            if (CurrentSubTabResearch.indexL == 0)
            {
                CurrentSubTabResearch.ResetTree(CurrentSubTabResearch.rightTree);
                CurrentSubTabResearch.rightTree.Clear();

                if (CurrentSubTabResearch.middleTree != null)
                {
                    CurrentSubTabResearch.ResetTree(CurrentSubTabResearch.middleTree);
                    CurrentSubTabResearch.middleTree.Clear();
                }

            }

            if (CurrentSubTabResearch.indexL <= 3)
            {
                CurrentSubTabResearch.indexL++;
            }

            updateCurrentActive();
        }
            
        
    }

    public void updateM()
    {
        if ((manager.GetComponent<GameManager>().Turn - CurrentSubTabResearch.ActiveTurn) == timers[CurrentSubTabResearch.indexM])
        {
            CurrentSubTabResearch.ActiveTurn = manager.GetComponent<GameManager>().Turn;

            CurrentSubTabResearch.Tree = "Middle";

            if (CurrentSubTabResearch.indexM == 0)
            {
                CurrentSubTabResearch.ResetTree(CurrentSubTabResearch.rightTree);
                CurrentSubTabResearch.rightTree.Clear();
                CurrentSubTabResearch.ResetTree(CurrentSubTabResearch.LeftTree);
                CurrentSubTabResearch.LeftTree.Clear();
            }

            if (CurrentSubTabResearch.indexM <= 3)
            {
                CurrentSubTabResearch.indexM++;
            }

            updateCurrentActive();
        }
            
    }

    public void updateR()
    {
        if ((manager.GetComponent<GameManager>().Turn - CurrentSubTabResearch.ActiveTurn) == timers[CurrentSubTabResearch.indexR])
        {
            CurrentSubTabResearch.ActiveTurn = manager.GetComponent<GameManager>().Turn;

            CurrentSubTabResearch.Tree = "Right";

            if (CurrentSubTabResearch.indexR == 0)
            {
                CurrentSubTabResearch.ResetTree(CurrentSubTabResearch.LeftTree);
                CurrentSubTabResearch.LeftTree.Clear();
                if (CurrentSubTabResearch.middleTree != null)
                {
                    CurrentSubTabResearch.ResetTree(CurrentSubTabResearch.middleTree);
                    CurrentSubTabResearch.middleTree.Clear();
                }

            }

            if (CurrentSubTabResearch.indexR <= 3)
            {
                CurrentSubTabResearch.indexR++;
            }

            updateCurrentActive();
        }        
    }
}
