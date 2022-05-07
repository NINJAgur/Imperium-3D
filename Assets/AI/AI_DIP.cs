using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class AI_DIP : MonoBehaviour
{
    Empire currentEmpire;
    /// </summary>
    /// 
    /// strategic value
    /// stance value
    /// diplomatic value
    ///     
    /// </summary>

    float strategicValue;
    float stanceValue;
    float diplomaticValue;

    float UpdatedScore;

    GameObject TradeHub;
    public GameManager manager;
    GameObject playerNotification;

    string TopTask;

    public void Build(Empire empire, string task)
    {
        currentEmpire = empire;

        playerNotification = GameObject.Find("PlayerMessage");
        TradeHub = GameObject.Find("TradeHub");
        strategicValue = 1f;
        stanceValue = 1f;
        diplomaticValue = 1f;

        TopTask = task;
    }

    public void Init()
    {
        foreach (EmpireIntel empire in currentEmpire.empireIntelList)
        {
            empire.calculateRelations(currentEmpire);
            CalculateUtilityValues(empire);

            UpdatedScore = (empire.Scariness * strategicValue + empire.Trustworthiness * stanceValue + empire.Aggression * diplomaticValue) / 3;

            Debug.Log(UpdatedScore);

            DipDecisions(currentEmpire.dipEmpires.Where(obj => obj.GetComponent<Empire>().empireName == empire.empireName).SingleOrDefault());
            InnitTrade(currentEmpire.dipEmpires.Where(obj => obj.GetComponent<Empire>().empireName == empire.empireName).SingleOrDefault());
        }
    }

    void DipDecisions(GameObject empire)
    {
        // Offers

        if (UpdatedScore > 30 && !currentEmpire.MAWith.Contains(empire.GetComponent<Empire>().empireName))
        {
            if (!empire.GetComponent<Empire>().isPlayerEmpire)
                empire.GetComponent<AI_DIP>().HandleOffers("MA", currentEmpire);
            else
                DiplomacyMenu("MA", empire, currentEmpire);
        }

        if (UpdatedScore > 50 && !currentEmpire.NAPWith.Contains(empire.GetComponent<Empire>().empireName))
            if (!empire.GetComponent<Empire>().isPlayerEmpire)
                empire.GetComponent<AI_DIP>().HandleOffers("NAP", currentEmpire);
            else
                DiplomacyMenu("NAP", empire, currentEmpire);

        if (UpdatedScore > 80 && !currentEmpire.AlliedWith.Contains(empire.GetComponent<Empire>().empireName))
            if (!empire.GetComponent<Empire>().isPlayerEmpire)
                empire.GetComponent<AI_DIP>().HandleOffers("Alliance", currentEmpire);
            else
                DiplomacyMenu("Alliance", empire, currentEmpire);

        if (UpdatedScore < -100 && !currentEmpire.AtWar.Contains(empire.GetComponent<Empire>().empireName))
            if (!empire.GetComponent<Empire>().isPlayerEmpire)
                empire.GetComponent<AI_DIP>().HandleOffers("War", currentEmpire);
            else
                DiplomacyMenu("War", empire, currentEmpire);

        // Cancellations

        if (UpdatedScore < -10 && currentEmpire.GetComponent<Empire>().MAWith.Contains(empire.GetComponent<Empire>().empireName))
            if (!empire.GetComponent<Empire>().isPlayerEmpire)
                empire.GetComponent<AI_DIP>().HandleOffers("CancelMA", currentEmpire);
            else
                DiplomacyMenu("CancelMA", empire, currentEmpire);

        if (UpdatedScore < 20 && currentEmpire.GetComponent<Empire>().NAPWith.Contains(empire.GetComponent<Empire>().empireName))
            if (!empire.GetComponent<Empire>().isPlayerEmpire)
                empire.GetComponent<AI_DIP>().HandleOffers("CancelNAP", currentEmpire);
            else
                DiplomacyMenu("CancelNAP", empire, currentEmpire);

        if (UpdatedScore < 50 && currentEmpire.GetComponent<Empire>().AlliedWith.Contains(empire.GetComponent<Empire>().empireName))
            if (!empire.GetComponent<Empire>().isPlayerEmpire)
                empire.GetComponent<AI_DIP>().HandleOffers("CancelAlliance", currentEmpire);
            else
                DiplomacyMenu("CancelAlliance", empire, currentEmpire);

        if (currentEmpire.AtWar.Contains(empire.GetComponent<Empire>().empireName) && ((currentEmpire.GetCapital().owner != currentEmpire && currentEmpire.empireCells.Count < currentEmpire.PreWarSize * 0.5) || (currentEmpire.empireCells.Count < currentEmpire.PreWarSize * 0.3) ))
             if (!empire.GetComponent<Empire>().isPlayerEmpire)
                empire.GetComponent<AI_DIP>().HandleOffers("Peace", currentEmpire);
             else
                DiplomacyMenu("Peace", empire, currentEmpire);
    }

    public void HandleOffers(string offer, Empire empire)
    {
        transform.gameObject.GetComponent<Empire>().UpdateIntel(empire.empireName).calculateRelations(currentEmpire);

        if (offer.Contains("MA"))
        {
            if (offer.Contains("Cancel"))
            {
                empire.MAWith.Remove(transform.gameObject.GetComponent<Empire>().empireName);
                transform.gameObject.GetComponent<Empire>().MAWith.Remove(empire.empireName);
            }
            else
            {
                if (transform.gameObject.GetComponent<Empire>().UpdateIntel(empire.empireName).GetScore() > 30)
                {
                    empire.MAWith.Add(transform.gameObject.GetComponent<Empire>().empireName);
                    transform.gameObject.GetComponent<Empire>().MAWith.Add(empire.empireName);
                }
            }
        }

        if (offer.Contains("NAP"))
        {
            if (offer.Contains("Cancel"))
            {
                empire.NAPWith.Remove(transform.gameObject.GetComponent<Empire>().empireName);
                transform.gameObject.GetComponent<Empire>().NAPWith.Remove(empire.empireName);
            }
            else
            {
                if (transform.gameObject.GetComponent<Empire>().UpdateIntel(empire.empireName).GetScore() > 50)
                {
                    empire.NAPWith.Add(transform.gameObject.GetComponent<Empire>().empireName);
                    transform.gameObject.GetComponent<Empire>().NAPWith.Add(empire.empireName);
                }
            }
        }

        if (offer.Contains("Alliance"))
        {
            if (offer.Contains("Cancel"))
            {
                empire.AlliedWith.Remove(transform.gameObject.GetComponent<Empire>().empireName);
                transform.gameObject.GetComponent<Empire>().AlliedWith.Remove(empire.empireName);
            }
            else
            {
                if (transform.gameObject.GetComponent<Empire>().UpdateIntel(empire.empireName).GetScore() > 80)
                {
                    foreach (string name in empire.AlliedWith)
                    {
                        GameObject ally = empire.dipEmpires.Where(obj => obj.GetComponent<Empire>().empireName == name).SingleOrDefault();

                        ally.GetComponent<Empire>().AlliedWith.Add(currentEmpire.empireName);
                        transform.gameObject.GetComponent<Empire>().AlliedWith.Add(name);
                    }

                    empire.AlliedWith.Add(transform.gameObject.GetComponent<Empire>().empireName);
                    transform.gameObject.GetComponent<Empire>().AlliedWith.Add(empire.empireName);
                }
            }
        }

        if (offer == "War")
        {
            empire.AtWar.Add(transform.gameObject.GetComponent<Empire>().empireName);
            transform.gameObject.GetComponent<Empire>().AtWar.Add(empire.empireName);

            foreach (string name in empire.AlliedWith)
            {
                GameObject ally = empire.dipEmpires.Where(obj => obj.GetComponent<Empire>().empireName == name).SingleOrDefault();

                ally.GetComponent<Empire>().AtWar.Add(currentEmpire.empireName);
                transform.gameObject.GetComponent<Empire>().AtWar.Add(name);
            }

        }

        if (offer == "Peace" && currentEmpire.empireCells.Count < 1.2 * currentEmpire.PreWarSize && ( strategicValue < 10 || diplomaticValue > 30))
        {
            empire.AtPeace.Add(transform.gameObject.GetComponent<Empire>().empireName);
            transform.gameObject.GetComponent<Empire>().AtPeace.Add(empire.empireName);

            foreach (string name in empire.AlliedWith)
            {
                GameObject ally = empire.dipEmpires.Where(obj => obj.GetComponent<Empire>().empireName == name).SingleOrDefault();

                ally.GetComponent<Empire>().AtPeace.Add(currentEmpire.empireName);
                transform.gameObject.GetComponent<Empire>().AtPeace.Add(name);
            }
        }

    }

    void DiplomacyMenu(string offer, GameObject player, Empire ai)
    {
        playerNotification.transform.GetChild(0).gameObject.SetActive(true);
        Time.timeScale = 0f;

        if (offer.Contains("MA"))
        {
            if (offer.Contains("Cancel"))
            {
                playerNotification.transform.GetChild(0).GetChild(13).GetComponent<Text>().text = currentEmpire.GetComponent<Empire>().empireName + " Cancels their Military Access with us";
                playerNotification.transform.GetChild(0).GetChild(14).gameObject.SetActive(true);
                player.GetComponent<Empire>().MAWith.Remove(ai.empireName);
                ai.MAWith.Remove(player.GetComponent<Empire>().empireName);
            }
            else
            {
                playerNotification.transform.GetChild(0).GetChild(13).GetComponent<Text>().text = currentEmpire.GetComponent<Empire>().empireName + " offers to sign Military Access with us";
                playerNotification.transform.GetChild(0).GetChild(9).gameObject.SetActive(true);
                playerNotification.transform.GetChild(0).GetChild(10).gameObject.SetActive(true);
                playerNotification.transform.GetChild(0).GetChild(9).gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { Accept(offer, player, ai); });
                playerNotification.transform.GetChild(0).GetChild(10).gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { Decline(player, ai); });

            }
        }

        if (offer.Contains("NAP"))
        {
            if (offer.Contains("Cancel"))
            {
                playerNotification.transform.GetChild(0).GetChild(13).GetComponent<Text>().text = currentEmpire.GetComponent<Empire>().empireName + " Cancels their Non- Agression Pact with us";
                playerNotification.transform.GetChild(0).GetChild(14).gameObject.SetActive(true);
                player.GetComponent<Empire>().NAPWith.Remove(ai.empireName);
                ai.NAPWith.Remove(player.GetComponent<Empire>().empireName);
            }
            else
            {
                playerNotification.transform.GetChild(0).GetChild(13).GetComponent<Text>().text = currentEmpire.GetComponent<Empire>().empireName + " offers to sign Non- Agression Pact with us";
                playerNotification.transform.GetChild(0).GetChild(9).gameObject.SetActive(true);
                playerNotification.transform.GetChild(0).GetChild(10).gameObject.SetActive(true);
                playerNotification.transform.GetChild(0).GetChild(9).gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { Accept(offer, player, ai); });
                playerNotification.transform.GetChild(0).GetChild(10).gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { Decline(player, ai); });
            }

        }

        if (offer.Contains("Alliance"))
        {
            if (offer.Contains("Cancel"))
            {
                playerNotification.transform.GetChild(0).GetChild(13).GetComponent<Text>().text = currentEmpire.GetComponent<Empire>().empireName + " Cancels their Alliance with us";
                playerNotification.transform.GetChild(0).GetChild(14).gameObject.SetActive(true);

                player.GetComponent<Empire>().AlliedWith.Remove(ai.empireName);
                ai.AlliedWith.Remove(player.GetComponent<Empire>().empireName);

            }
            else
            {
                playerNotification.transform.GetChild(0).GetChild(13).GetComponent<Text>().text = currentEmpire.GetComponent<Empire>().empireName + " offers to sign Alliance with us";
                playerNotification.transform.GetChild(0).GetChild(9).gameObject.SetActive(true);
                playerNotification.transform.GetChild(0).GetChild(10).gameObject.SetActive(true);
                playerNotification.transform.GetChild(0).GetChild(9).gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { Accept(offer, player, ai); });
                playerNotification.transform.GetChild(0).GetChild(10).gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { Decline(player, ai); });
            }
        }

        if (offer == "War")
        {
            playerNotification.transform.GetChild(0).GetChild(13).GetComponent<Text>().text = currentEmpire.GetComponent<Empire>().empireName + "  declares WAR on us";
            playerNotification.transform.GetChild(0).GetChild(14).gameObject.SetActive(true);

            player.GetComponent<Empire>().AtWar.Add(ai.empireName);
            ai.AtWar.Add(player.GetComponent<Empire>().empireName);


            foreach (string name in player.GetComponent<Empire>().AlliedWith)
            {
                GameObject ally = player.GetComponent<Empire>().dipEmpires.Where(obj => obj.GetComponent<Empire>().empireName == name).SingleOrDefault();

                ally.GetComponent<Empire>().AtWar.Add(currentEmpire.empireName);
                transform.gameObject.GetComponent<Empire>().AtWar.Add(name);
            }
        }

        if (offer == "Peace")
        {
            playerNotification.transform.GetChild(0).GetChild(13).GetComponent<Text>().text = currentEmpire.GetComponent<Empire>().empireName + "  offers to sign Peace with us";
            playerNotification.transform.GetChild(0).GetChild(9).gameObject.SetActive(true);
            playerNotification.transform.GetChild(0).GetChild(10).gameObject.SetActive(true);
            playerNotification.transform.GetChild(0).GetChild(9).gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { Accept(offer, player, ai); });
            playerNotification.transform.GetChild(0).GetChild(10).gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { Decline(player, ai); });

        }
    }

    public void Accept(string offer, GameObject player, Empire ai)
    {

        if (offer == "MA")
        {
            player.GetComponent<Empire>().MAWith.Add(ai.empireName);
            ai.MAWith.Add(player.GetComponent<Empire>().empireName);
        }

        if (offer == "NAP")
        {
            player.GetComponent<Empire>().NAPWith.Add(ai.empireName);
            ai.NAPWith.Add(player.GetComponent<Empire>().empireName);
        }

        if (offer == "Alliance")
        {
            foreach (string name in player.GetComponent<Empire>().AlliedWith)
            {
                GameObject ally = player.GetComponent<Empire>().dipEmpires.Where(obj => obj.GetComponent<Empire>().empireName == name).SingleOrDefault();

                ally.GetComponent<Empire>().AlliedWith.Add(currentEmpire.empireName);
                transform.gameObject.GetComponent<Empire>().AlliedWith.Add(name);
            }
            player.GetComponent<Empire>().AlliedWith.Add(ai.empireName);
            ai.AlliedWith.Add(player.GetComponent<Empire>().empireName);
        }

        if (offer == "Peace")
        {
            player.GetComponent<Empire>().AtPeace.Add(ai.empireName);
            ai.AtPeace.Add(player.GetComponent<Empire>().empireName);

            foreach (string name in player.GetComponent<Empire>().AlliedWith)
            {
                GameObject ally = player.GetComponent<Empire>().dipEmpires.Where(obj => obj.GetComponent<Empire>().empireName == name).SingleOrDefault();

                ally.GetComponent<Empire>().AtPeace.Add(currentEmpire.empireName);
                transform.gameObject.GetComponent<Empire>().AtPeace.Add(name);
            }
        }

        playerNotification.transform.GetChild(0).GetChild(9).gameObject.SetActive(false);
        playerNotification.transform.GetChild(0).GetChild(10).gameObject.SetActive(false);
        playerNotification.transform.GetChild(0).GetChild(14).gameObject.SetActive(false);
        playerNotification.transform.GetChild(0).gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Decline(GameObject player, Empire ai)
    {
        player.GetComponent<Empire>().UpdateIntel(ai.empireName).Aggression -= 10;
        ai.UpdateIntel(player.GetComponent<Empire>().empireName).Aggression -= 10;

        playerNotification.transform.GetChild(0).GetChild(9).gameObject.SetActive(false);
        playerNotification.transform.GetChild(0).GetChild(10).gameObject.SetActive(false);
        playerNotification.transform.GetChild(0).GetChild(14).gameObject.SetActive(false);
        playerNotification.transform.GetChild(0).gameObject.SetActive(false); 
        Time.timeScale = 1f;


    }

    void InnitTrade(GameObject empire)
    {
        if (currentEmpire.AlliedWith.Contains(empire.GetComponent<Empire>().empireName) || currentEmpire.NAPWith.Contains(empire.GetComponent<Empire>().empireName))
        {
            if (empire.GetComponent<Empire>().isPlayerEmpire)
            {
                TradeHub.transform.GetChild(0).gameObject.SetActive(true);
                Time.timeScale = 0f;

                TradeHub.GetComponent<TradeHub>().manager = GameObject.Find("GameManager");
                TradeHub.GetComponent<TradeHub>().CurrentEmpire = currentEmpire.gameObject;
                TradeHub.GetComponent<TradeHub>().SetGoods();
                TradeHub.GetComponent<TradeHub>().Evaluate();

            }
            else
            {
                List<int> neededResources_trader1 = new List<int>(); // needed resources for current empire

                for (int i = 1; i < currentEmpire.resources.Length; i++)
                {
                    if (i == 0 && currentEmpire.UpdateIntel(empire.GetComponent<Empire>().empireName).resources[i] > 0)
                        neededResources_trader1.Add(i);
                }

                List<int> neededResources_trader2 = new List<int>(); // needed resources for oponent empire

                for (int i = 1; i < empire.GetComponent<Empire>().resources.Length; i++)
                {
                    if (i == 0 && currentEmpire.UpdateIntel(currentEmpire.empireName).resources[i] > 0)
                        neededResources_trader2.Add(i);
                }

                if (neededResources_trader1.Count == neededResources_trader2.Count)
                {
                    foreach (int resource in neededResources_trader1)
                    {
                        currentEmpire.resources[resource] ++;
                        empire.GetComponent<Empire>().resources[resource] --;
                    }

                    foreach (int resource in neededResources_trader2)
                    {
                        currentEmpire.resources[resource]--;
                        empire.GetComponent<Empire>().resources[resource]++;
                    }
                }
            }
        }
    }

    void CalculateUtilityValues(EmpireIntel empire)
    {
        // STRATEGIC VALUE CALCULATION

        foreach (string name in empire.AlliedWith)
        {
            if (currentEmpire.UpdateIntel(name) != null && currentEmpire.UpdateIntel(name).knownUnits.Count < currentEmpire.units.Count)
                if (currentEmpire.UpdateIntel(name).GetScore() > 0)
                    strategicValue += Mathf.Sign(empire.GetScore()) * 0.1f;
                else
                    strategicValue += Mathf.Sign(empire.GetScore()) * 0.3f;
            else
                strategicValue -= Mathf.Sign(empire.GetScore()) * 0.1f;
        }

        foreach (string name in empire.NAPWith)
        {
            if (currentEmpire.UpdateIntel(name) != null && currentEmpire.UpdateIntel(name).knownUnits.Count < currentEmpire.units.Count)
                if (currentEmpire.UpdateIntel(name).GetScore() > 0)
                    strategicValue += Mathf.Sign(empire.GetScore()) * 0.1f;
                else
                    strategicValue -= Mathf.Sign(empire.GetScore()) * 0.1f;
        }

        foreach (string name in empire.MAWith)
        {
            if (currentEmpire.UpdateIntel(name) != null && currentEmpire.UpdateIntel(name).knownUnits.Count < currentEmpire.units.Count)
                if (currentEmpire.UpdateIntel(name).GetScore() < 0)
                    strategicValue += Mathf.Sign(empire.GetScore()) * 0.1f;
                else
                    strategicValue -= Mathf.Sign(empire.GetScore()) * 0.1f;
        }

        for (int i = 1; i < empire.resources.Length; i++)
            if (empire.resources[i] > 0 && currentEmpire.resources[i].Equals(0))
                stanceValue += Mathf.Sign(empire.GetScore()) * 0.1f;
            else
                strategicValue -= Mathf.Sign(empire.GetScore()) * 0.1f;

        // STANCE VALUE CALCULATION

        if (TopTask == "Expansionism")
        {
            stanceValue = (empire.knownSoldiers.Count - currentEmpire.soldiers) * 0.1f * Mathf.Sign(empire.GetScore());
            int counter = 0;

            foreach (HexUnit settler in empire.knownSettlers)
            {
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                    if (settler.Location.GetNeighbor(d) != null && settler.Location.GetNeighbor(d).SpecialIndex == 1 && settler.Location.GetNeighbor(d).owner == null)
                        counter++;
            }

            stanceValue = counter * 0.1f * Mathf.Sign(empire.GetScore());
        }

        if (TopTask == "Threat_concern")
        {
            stanceValue = empire.AtWar.Count * 0.3f * Mathf.Sign(empire.GetScore());
        }

        if (TopTask == "ConstructionSpending")
        {
            stanceValue = (empire.knownWorkers.Count - currentEmpire.workers + empire.Urban.Count - currentEmpire.Urban.Count + empire.farms.Count - currentEmpire.farms.Count) * 0.1f * Mathf.Sign(empire.GetScore());
        }

        // DIPLOMATIC VALUE CALCULATION

        foreach (EmpireIntel e in currentEmpire.empireIntelList)
        {
            if (e != empire)
            {
                e.calculateRelations(currentEmpire.dipEmpires.Where(obj => obj.GetComponent<Empire>().empireName == e.empireName).SingleOrDefault().GetComponent<Empire>());
                if (e.GetScore() > 0)
                    diplomaticValue += 0.1f;
                else
                    diplomaticValue -= 0.1f;
            }
        }
    }
}
