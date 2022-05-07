using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResearchSubTab
{
    public List<GameObject> LeftTree;
    public int indexL;
    public List<GameObject> middleTree;
    public int indexM;
    public List<GameObject> rightTree;
    public int indexR;

    public int ActiveTurn;
    public string Tree;
    public string TabName;

    public void ResetTree(List<GameObject> Tree)
    {
        foreach (GameObject element in Tree)
        {
            element.GetComponent<Button>().enabled = false;
            element.GetComponent<Image>().color = Color.gray;
        }
    }

    public void UpdateLists()
    {
        if (LeftTree.Count > 0)
        {
            foreach (GameObject element in LeftTree)
            {
                if (LeftTree.IndexOf(element) != indexL)
                {
                    element.GetComponent<Button>().enabled = false;
                    if (indexL == 4)
                        element.GetComponent<Image>().color = Color.green;
                    else
                        element.GetComponent<Image>().color = Color.gray;

                }
                else
                {
                    element.GetComponent<Button>().enabled = true;
                    if (indexL == 4)
                        element.GetComponent<Image>().color = Color.green;
                    else
                        element.GetComponent<Image>().color = Color.white;
                }
            }
        }


        if (middleTree != null)
            if (middleTree.Count > 0)
            {
                foreach (GameObject element in middleTree)
                {
                    if (middleTree.IndexOf(element) != indexM)
                    {
                        element.GetComponent<Button>().enabled = false;
                        if (indexM == 4)
                            element.GetComponent<Image>().color = Color.green;
                        else
                            element.GetComponent<Image>().color = Color.gray;
                    }
                    else
                    {
                        element.GetComponent<Button>().enabled = true;
                        if (indexM == 4)
                            element.GetComponent<Image>().color = Color.green;
                        else
                            element.GetComponent<Image>().color = Color.white;
                    }
                }
            }


        if (rightTree.Count > 0)
        {
            foreach (GameObject element in rightTree)
            {

                if (rightTree.IndexOf(element) != indexR)
                {
                    element.GetComponent<Button>().enabled = false;
                    if (indexR == 4)
                        element.GetComponent<Image>().color = Color.green;
                    else
                        element.GetComponent<Image>().color = Color.gray;
                }
                else
                {
                    element.GetComponent<Button>().enabled = true;
                    if (indexR == 4)
                        element.GetComponent<Image>().color = Color.green;
                    else
                        element.GetComponent<Image>().color = Color.white;
                }
            }

        }

    }
}
