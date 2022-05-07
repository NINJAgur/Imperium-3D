using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public Map grid;
	public HexMapCamera cam;
	HexCell currentCell;
	
	public HexUnit selectedUnit;

	public GameObject deploymentTab;
	public GameObject ConstructionMenuTab;
	public GameManager gameManager;
	public HexCell currentSpawn;

	bool CatapultAttack = false;

	void Update()
	{
		if (GetCellUnderCursor() != null && GetCellUnderCursor().Unit != null && GetCellUnderCursor().IsVisible)
        {
			transform.Find("UnitProfile").gameObject.SetActive(true);

			if (GetCellUnderCursor().Unit.Embarked)
				transform.Find("UnitProfile").GetChild(0).GetChild(2).GetChild(0).GetComponent<Image>().sprite = gameManager.BoatProfiles[GetCellUnderCursor().Unit.ParentEmpire.GetComponent<Empire>().EmpireIndex];
			else
            {
				if (GetCellUnderCursor().Unit.type == "Soldier")
					transform.Find("UnitProfile").GetChild(0).GetChild(2).GetChild(0).gameObject.GetComponent<Image>().sprite = gameManager.UnitProfiles[GetCellUnderCursor().Unit.ParentEmpire.GetComponent<Empire>().EmpireIndex];

				if (GetCellUnderCursor().Unit.type == "Worker")
					transform.Find("UnitProfile").GetChild(0).GetChild(2).GetChild(0).gameObject.GetComponent<Image>().sprite = gameManager.workerProfile;

				if (GetCellUnderCursor().Unit.type == "Catapult")
					transform.Find("UnitProfile").GetChild(0).GetChild(2).GetChild(0).gameObject.GetComponent<Image>().sprite = gameManager.catapultProfile;

				if (GetCellUnderCursor().Unit.type == "Settler")
					transform.Find("UnitProfile").GetChild(0).GetChild(2).GetChild(0).gameObject.GetComponent<Image>().sprite = gameManager.settlerProfile;
			}

			transform.Find("UnitProfile").GetChild(0).GetChild(4).GetComponent<Text>().text = GetCellUnderCursor().Unit.ParentEmpire.GetComponent<Empire>().empireName + " " + GetCellUnderCursor().Unit.type;
			transform.Find("UnitProfile").GetChild(0).GetChild(5).GetChild(0).GetChild(0).GetComponent<Text>().text = GetCellUnderCursor().Unit.Attack.ToString();
			transform.Find("UnitProfile").GetChild(0).GetChild(6).GetChild(0).GetChild(0).GetComponent<Text>().text = GetCellUnderCursor().Unit.Defense.ToString();

		}
		else
        {
			transform.Find("UnitProfile").gameObject.SetActive(false);
		}

		if (!EventSystem.current.IsPointerOverGameObject())
		{
			if (Input.GetMouseButtonDown(0))
			{
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(ray, out hit))
				{
					if (hit.transform.name == "capitalIcon(Clone)")
					{
						if (currentSpawn != hit.transform.gameObject.GetComponent<EmpireCity>().city)
						{
							currentSpawn = hit.transform.gameObject.GetComponent<EmpireCity>().city;
							hit.transform.gameObject.GetComponent<Animator>().SetTrigger("Active");
						}
						else
						{
							currentSpawn = null;
						}
					}
				}

				DoSelection();
			}
			else if (selectedUnit)
			{
				if (Input.GetMouseButtonDown(2) && selectedUnit.Actions > 0)
                {
					if (selectedUnit.type == "Soldier")
                    {
						if (selectedUnit.Location.owner != null && checkCapture())
						{
							selectedUnit.ParentEmpire.GetComponent<Empire>().empireCells.Add(selectedUnit.Location);
							selectedUnit.Location.owner = selectedUnit.ParentEmpire;
							selectedUnit.Actions--;
							selectedUnit.transform.gameObject.GetComponent<AudioSource>().clip = gameManager.GetComponent<GameManager>().Abilities[0];
							selectedUnit.transform.gameObject.GetComponent<AudioSource>().Play();
						}
						else
                        {
							if (selectedUnit.ParentEmpire.GetComponent<Empire>().empireCells.Contains(selectedUnit.Location) && selectedUnit.ParentEmpire.GetComponent<Empire>().manpower >= 500)
                            {
								selectedUnit.ParentEmpire.GetComponent<Empire>().manpower -= 500;
								selectedUnit.ParentEmpire.GetComponent<Empire>().updateLabels();
								selectedUnit.Attack += 250;
								selectedUnit.Defense += 250;
								selectedUnit.Actions--;
								selectedUnit.transform.gameObject.GetComponent<AudioSource>().clip = gameManager.GetComponent<GameManager>().Clicks[1];
								selectedUnit.transform.gameObject.GetComponent<AudioSource>().Play();
							}
						}
					}
					
					if (selectedUnit.type == "Worker")
					{
						if (selectedUnit.ParentEmpire.GetComponent<Empire>().empireCells.Contains(selectedUnit.Location) && selectedUnit.Location.SpecialIndex != 1)
                        {
							if (!selectedUnit.Location.IsResource)
                            {
								// worker ability
								ConstructionMenu();
								selectedUnit.GetComponentInChildren<Animator>().SetTrigger("Build");
								selectedUnit.transform.gameObject.GetComponent<AudioSource>().clip = gameManager.GetComponent<GameManager>().Abilities[2];
								selectedUnit.transform.gameObject.GetComponent<AudioSource>().Play();
							}

						}

						if (selectedUnit.Location.IsResource)
						{
							selectedUnit.ParentEmpire.GetComponent<Empire>().resources[selectedUnit.Location.ResourceIndex]++;
							selectedUnit.GetComponentInChildren<Animator>().SetTrigger("Mine");
							selectedUnit.ParentEmpire.GetComponent<Empire>().updateLabels();
							selectedUnit.Actions--;
						}

						if (selectedUnit.Location.PlantLevel == 2 || selectedUnit.Location.PlantLevel == 3)
						{
							selectedUnit.ParentEmpire.GetComponent<Empire>().resources[5]++;
							selectedUnit.GetComponentInChildren<Animator>().SetTrigger("Chop");
							selectedUnit.Location.PlantLevel = 0;
							selectedUnit.ParentEmpire.GetComponent<Empire>().updateLabels();
							selectedUnit.Actions--;
						}

					}

					if (selectedUnit.type == "Settler")
                    {
						Settle();
						if (selectedUnit != null)
							selectedUnit.Actions--;
						selectedUnit.transform.gameObject.GetComponent<AudioSource>().clip = gameManager.GetComponent<GameManager>().Abilities[1];
						selectedUnit.transform.gameObject.GetComponent<AudioSource>().Play();
					}

					if (selectedUnit.type == "Catapult")
                    {
						if (CatapultAttack)
							CatapultAttack = false;
						else
							CatapultAttack = true;
                    }
				}

				if (Input.GetMouseButtonDown(1))
				{
					DoMove();
				}
				else
				{
					DoPathfinding();
				}
			}
		}
	}

    void LateUpdate()
    {
		if (CatapultAttack)
		{
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				if (selectedUnit.Location.GetNeighbor(d) != null)
					selectedUnit.Location.GetNeighbor(d).EnableHighlight(Color.white);

				if (GetCellUnderCursor() == selectedUnit.Location.GetNeighbor(d))
					selectedUnit.Location.GetNeighbor(d).EnableHighlight(Color.red);

				if (Input.GetMouseButtonDown(0) && GetCellUnderCursor() == selectedUnit.Location.GetNeighbor(d)&& GetCellUnderCursor().Unit != null
					&& selectedUnit.ParentEmpire.GetComponent<Empire>().AtWar.Contains(GetCellUnderCursor().Unit.ParentEmpire.GetComponent<Empire>().empireName))
				{
					Vector3 a, b, c = selectedUnit.Location.Position;
					a = c;
					b = GetCellUnderCursor().Position;
					c = (b + GetCellUnderCursor().Position) * 0.5f;
					float t = Time.deltaTime * 4f;
					Vector3 direction = Bezier.GetDerivative(a, b, c, t);
					direction.y = 0f;
					selectedUnit.transform.localRotation = Quaternion.LookRotation(direction);

					selectedUnit.GetComponentInChildren<Animator>().SetTrigger("Attack");

					if (selectedUnit.Attack > GetCellUnderCursor().Unit.Defense)
						grid.RemoveUnit(GetCellUnderCursor().Unit);
					else
						GetCellUnderCursor().Unit.Defense -= selectedUnit.Attack;

					selectedUnit.Actions--;
				}
			}
		}
		else
		{
			if (selectedUnit != null && selectedUnit.type == "Catapult")
			{
				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
				{
					if (selectedUnit.Location.GetNeighbor(d) != null)
						selectedUnit.Location.GetNeighbor(d).DisableHighlight();
				}
			}
		}
	}

    public void GetUnit()
    {
		foreach (HexUnit unit in gameManager.GetPlayerEmpire().GetComponent<Empire>().units)
        {
			if (unit.Actions > 0)
            {
				cam.TravelTo(unit.transform.position.x, unit.transform.position.z);
				break;
			}
		}
    }

	void ConstructionMenu()
    {
		Time.timeScale = 0f;
		ConstructionMenuTab.SetActive(true);
	}

	public void buildFarm()
    {
		if (selectedUnit.ParentEmpire.GetComponent<Empire>().manpower >= 1000)
        {
			Time.timeScale = 1f;
			selectedUnit.Location.FarmLevel = 1;
			selectedUnit.Location.PlantLevel = 1;
			selectedUnit.ParentEmpire.GetComponent<Empire>().farms.Add(selectedUnit.Location);
			selectedUnit.ParentEmpire.GetComponent<Empire>().manpower -= 1000;
			selectedUnit.Actions--;
			ConstructionMenuTab.SetActive(false);
		}
		
	}

	public void buildUrban()
    {
		if (selectedUnit.ParentEmpire.GetComponent<Empire>().manpower >= 1500)
		{
			Time.timeScale = 1f;
			selectedUnit.Location.UrbanLevel = 2;
			selectedUnit.Location.PlantLevel = 1;
			selectedUnit.ParentEmpire.GetComponent<Empire>().Urban.Add(selectedUnit.Location);
			selectedUnit.ParentEmpire.GetComponent<Empire>().manpower -= 1500;
			selectedUnit.Actions--;
			ConstructionMenuTab.SetActive(false);
		}
	}

	public void buildWall()
	{
		if (selectedUnit.ParentEmpire.GetComponent<Empire>().manpower >= 500)
		{
			Time.timeScale = 1f;
			selectedUnit.Location.PlantLevel = 0;
			selectedUnit.Location.Walled = true;
			selectedUnit.ParentEmpire.GetComponent<Empire>().manpower -= 500;
			selectedUnit.Actions--;
			ConstructionMenuTab.SetActive(false);
		}
	}

	public void buildRoad()
	{
		if (selectedUnit.ParentEmpire.GetComponent<Empire>().manpower >= 100)
		{
			Time.timeScale = 1f;
			selectedUnit.Location.AddRoad(selectedUnit.getDirection());
			selectedUnit.ParentEmpire.GetComponent<Empire>().manpower -= 100;
			selectedUnit.Actions--;
			ConstructionMenuTab.SetActive(false);
		}
	}

	public void buildTemple()
	{
		if (selectedUnit.ParentEmpire.GetComponent<Empire>().manpower >= 3000)
		{
			Time.timeScale = 1f;
			selectedUnit.Location.SpecialIndex = 2;
			selectedUnit.Location.PlantLevel = 0;
			selectedUnit.ParentEmpire.GetComponent<Empire>().Temples.Add(selectedUnit.Location);
			selectedUnit.ParentEmpire.GetComponent<Empire>().manpower -= 3000;
			selectedUnit.Actions--;
			ConstructionMenuTab.SetActive(false);
		}
	}

	// Settler Ability
	public void Settle()
    {
		if (selectedUnit.Location.owner == null)
		{
			if (selectedUnit.Location.SpecialIndex == 1)
			{
				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
				{
					if (selectedUnit.Location.GetNeighbor(d) != null && selectedUnit.Location.GetNeighbor(d).owner == null)
					{
						selectedUnit.ParentEmpire.GetComponent<Empire>().empireCells.Add(selectedUnit.Location.GetNeighbor(d));
						selectedUnit.Location.GetNeighbor(d).owner = selectedUnit.ParentEmpire;
					}
				}

				selectedUnit.ParentEmpire.GetComponent<Empire>().Cities.Add(selectedUnit.Location);
			}
            else
            {
				selectedUnit.ParentEmpire.GetComponent<Empire>().empireCells.Add(selectedUnit.Location);
				selectedUnit.Location.owner = selectedUnit.ParentEmpire;
			}

			grid.RemoveUnit(selectedUnit);
		}
	}

	// Soldier Ability
	public bool checkCapture()
	{
		if (selectedUnit.Location.owner.GetComponent<Empire>().AtWar.Contains(selectedUnit.ParentEmpire.GetComponent<Empire>().empireName))
		{
			selectedUnit.Location.owner.GetComponent<Empire>().empireCells.Remove(selectedUnit.Location);

			if (selectedUnit.Location.owner.GetComponent<Empire>().farms.Contains(selectedUnit.Location))
			{
				selectedUnit.Location.FarmLevel = 0;
				selectedUnit.Location.owner.GetComponent<Empire>().farms.Remove(selectedUnit.Location);
			}

			if (selectedUnit.Location.owner.GetComponent<Empire>().Urban.Contains(selectedUnit.Location))
			{
				selectedUnit.Location.UrbanLevel = 0;
				selectedUnit.Location.owner.GetComponent<Empire>().Urban.Remove(selectedUnit.Location);
			}

			if (selectedUnit.Location.owner.GetComponent<Empire>().Temples.Contains(selectedUnit.Location))
			{
				selectedUnit.Location.SpecialIndex = 0;
				selectedUnit.Location.owner.GetComponent<Empire>().Temples.Remove(selectedUnit.Location);
			}

			if (selectedUnit.Location.owner.GetComponent<Empire>().Cities.Contains(selectedUnit.Location))
			{
				selectedUnit.Location.owner.GetComponent<Empire>().Cities.Remove(selectedUnit.Location);
				selectedUnit.ParentEmpire.GetComponent<Empire>().Cities.Add(selectedUnit.Location);
			}

			return true;
		}
		else
			return false;
	}

	HexCell GetCellUnderCursor()
	{
		return grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
	}

	// SOLDIER UNIT
	public void CreateUnit()
	{
		GameObject currentEmpire = gameManager.GetCurrentActive();

		if(currentEmpire != null && currentSpawn != null)
        {
			if (currentEmpire.GetComponent<Empire>().treasury >= 1000 && currentEmpire.GetComponent<Empire>().manpower >= 1000)
			{
				HexCell cell = null;
				
				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
				{
					if (currentSpawn.GetNeighbor(d) != null && !currentSpawn.GetNeighbor(d).IsUnderwater && currentSpawn.GetNeighbor(d).Unit == null)
					{
						cell = currentSpawn.GetNeighbor(d);
						break;
					}
				}
				
				if (cell != null)
				{
					grid.AddUnit(Instantiate(HexUnit.unitPrefab), cell, Random.Range(0f, 360f), currentEmpire, "Soldier");

					currentEmpire.GetComponent<Empire>().treasury -= 1000;
					currentEmpire.GetComponent<Empire>().manpower -= 1000;

					currentEmpire.GetComponent<Empire>().soldiers += 1;

					if (currentEmpire == gameManager.GetPlayerEmpire())
                    {
						currentEmpire.GetComponent<Empire>().updateLabels();
					}
				}
			}
		}

	}

	// WORKER UNIT
	public void CreateWorker()
	{
		GameObject currentEmpire = gameManager.GetCurrentActive();

		if (currentEmpire != null && currentSpawn != null)
		{
			if (currentEmpire.GetComponent<Empire>().treasury >= 1000 && currentEmpire.GetComponent<Empire>().manpower >= 1000)
			{
				HexCell cell = null;

				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
				{
					if (currentSpawn.GetNeighbor(d) != null && !currentSpawn.GetNeighbor(d).IsUnderwater && currentSpawn.GetNeighbor(d).Unit == null)
					{
						cell = currentSpawn.GetNeighbor(d);
						break;
					}
				}

				if (cell != null)
				{
					grid.AddUnit(Instantiate(HexUnit.workerPrefab), cell, Random.Range(0f, 360f), currentEmpire, "Worker");

					currentEmpire.GetComponent<Empire>().treasury -= 1000;
					currentEmpire.GetComponent<Empire>().manpower -= 1000;

					currentEmpire.GetComponent<Empire>().workers += 1;

					if (currentEmpire == gameManager.GetPlayerEmpire())
					{
						currentEmpire.GetComponent<Empire>().updateLabels();
					}
				}
			}
		}
	}

	// SETTLER UNIT
	public void CreateSettler()
	{
		GameObject currentEmpire = gameManager.GetCurrentActive();

		if (currentEmpire != null && currentSpawn != null)
		{
			if (currentEmpire.GetComponent<Empire>().treasury >= 500 && currentEmpire.GetComponent<Empire>().manpower >= 500)
			{
				HexCell cell = null;

				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
				{
					if (currentSpawn.GetNeighbor(d) != null && !currentSpawn.GetNeighbor(d).IsUnderwater && currentSpawn.GetNeighbor(d).Unit == null)
					{
						cell = currentSpawn.GetNeighbor(d);
						break;
					}
				}

				if (cell != null)
				{
					grid.AddUnit(Instantiate(HexUnit.settlerPrefab), cell, Random.Range(0f, 360f), currentEmpire, "Settler");

					currentEmpire.GetComponent<Empire>().treasury -= 500;
					currentEmpire.GetComponent<Empire>().manpower -= 2000;

					currentEmpire.GetComponent<Empire>().settlers += 1;

					if (currentEmpire == gameManager.GetPlayerEmpire())
					{
						currentEmpire.GetComponent<Empire>().updateLabels();
					}
				}
				
			}
		}
	}

	// CATAPULT UNIT
	public void CreateCatapult()
	{
		GameObject currentEmpire = gameManager.GetCurrentActive();

		if (currentEmpire != null && currentSpawn != null)
		{
			if (currentEmpire.GetComponent<Empire>().treasury >= 1500 && currentEmpire.GetComponent<Empire>().manpower >= 1000)
			{
				HexCell cell = null;

				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
				{
					if (currentSpawn.GetNeighbor(d) != null && !currentSpawn.GetNeighbor(d).IsUnderwater && currentSpawn.GetNeighbor(d).Unit == null)
					{
						cell = currentSpawn.GetNeighbor(d);
						break;
					}
				}

				if (cell != null)
				{
					grid.AddUnit(Instantiate(HexUnit.catapultPrefab), cell, Random.Range(0f, 360f), currentEmpire, "Catapult");

					currentEmpire.GetComponent<Empire>().treasury -= 1500;
					currentEmpire.GetComponent<Empire>().manpower -= 1000;

					currentEmpire.GetComponent<Empire>().ranged += 1;

					if (currentEmpire == gameManager.GetPlayerEmpire())
					{
						currentEmpire.GetComponent<Empire>().updateLabels();
					}
				}

			}
		}
	}

	public void DestroyUnit()
	{
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.Unit)
		{
			grid.RemoveUnit(cell.Unit);
		}
	}

	void DoSelection()
	{
		grid.ClearPath();
		UpdateCurrentCell();
		if (currentCell)
		{
			if (currentCell.Unit && currentCell.Unit.isPlayerUnit)
			{
				selectedUnit = currentCell.Unit;

				if (selectedUnit.type != "Worker")
                {
					AudioClip clip = GetAudioClip(gameManager.GetComponent<GameManager>().Selection);
					if (clip != null)
					{
						selectedUnit.transform.gameObject.GetComponent<AudioSource>().clip = clip;
						selectedUnit.transform.gameObject.GetComponent<AudioSource>().Play();
					}
				}
                else
                {
					selectedUnit.transform.gameObject.GetComponent<AudioSource>().clip = gameManager.GetComponent<GameManager>().Worker[Random.Range(0, gameManager.GetComponent<GameManager>().Worker.Length)];
					selectedUnit.transform.gameObject.GetComponent<AudioSource>().Play();

				}

				
				foreach (HexUnit unit in gameManager.GetPlayerEmpire().GetComponent<Empire>().units)
				{
					unit.transform.GetChild(3).GetComponent<MeshRenderer>().material.color = selectedUnit.ParentEmpire.GetComponent<Empire>().empireColor;
					unit.transform.GetChild(4).GetComponent<MeshRenderer>().material.color = selectedUnit.ParentEmpire.GetComponent<Empire>().empireColor;
				}


				selectedUnit.transform.GetChild(3).GetComponent<MeshRenderer>().material.color = Color.green;
				selectedUnit.transform.GetChild(4).GetComponent<MeshRenderer>().material.color = Color.green;
			}

			else
            {
				if (CatapultAttack)
					return;

				selectedUnit = null;

				foreach (HexUnit unit in gameManager.GetPlayerEmpire().GetComponent<Empire>().units)
				{
					unit.transform.GetChild(3).GetComponent<MeshRenderer>().material.color = gameManager.GetPlayerEmpire().GetComponent<Empire>().empireColor;
					unit.transform.GetChild(4).GetComponent<MeshRenderer>().material.color = gameManager.GetPlayerEmpire().GetComponent<Empire>().empireColor;
				}

			}
		}
	}

	AudioClip GetAudioClip(AudioClip [] arr)
    {
		AudioClip found = null;
		
		while (found == null)
        {
			found = arr [Random.Range(0, arr.Length)];

			if (!found.name.Contains(selectedUnit.ParentEmpire.GetComponent<Empire>().empireName))
				found = null;
        }

		return found;
	}

	void DoPathfinding()
	{
		if (!CatapultAttack && UpdateCurrentCell())
		{
			if (currentCell && selectedUnit.IsValidDestination(currentCell) && selectedUnit.Actions > 0)
			{
				grid.FindPath(selectedUnit.Location, currentCell, selectedUnit);
			}
			else
			{
				grid.ClearPath();
			}
		}
	}

	void DoMove()
	{
		if (grid.HasPath)
		{
			if (currentCell.Unit && currentCell.Unit != selectedUnit)
			{
				if (currentCell.Unit.ParentEmpire != selectedUnit.ParentEmpire && selectedUnit.ParentEmpire.GetComponent<Empire>().AtWar.Contains(currentCell.Unit.ParentEmpire.GetComponent<Empire>().empireName))
				{
					if (selectedUnit.type == "Soldier")
					{
						Battle();
						if (selectedUnit.Attack > currentCell.Unit.Defense)
						{
							grid.RemoveUnit(currentCell.Unit);
							//selectedUnit.GetComponentInChildren<Animator>().SetTrigger("Attack");
							transform.GetComponentInChildren<Animation>().clip = transform.GetComponentInChildren<Animation>().GetClip("attack");
							transform.GetComponentInChildren<Animation>().Play();
							transform.GetComponentInChildren<Animation>().clip = transform.GetComponentInChildren<Animation>().GetClip("idle");
							transform.GetComponentInChildren<Animation>().Play();

							selectedUnit.Attack = currentCell.Unit.Defense;

						}
						else
						{
							if (currentCell.Unit.Defense > selectedUnit.Attack + selectedUnit.Defense)
							{
								grid.RemoveUnit(selectedUnit);
								selectedUnit.transform.gameObject.GetComponent<AudioSource>().clip = gameManager.GetComponent<GameManager>().Misc[1];
								selectedUnit.transform.gameObject.GetComponent<AudioSource>().Play();
								return;
							}
							else
							{
								//selectedUnit.GetComponentInChildren<Animator>().SetTrigger("Attack");
								transform.GetComponentInChildren<Animation>().clip = transform.GetComponentInChildren<Animation>().GetClip("attack");
								transform.GetComponentInChildren<Animation>().Play();
								transform.GetComponentInChildren<Animation>().clip = transform.GetComponentInChildren<Animation>().GetClip("idle");
								transform.GetComponentInChildren<Animation>().Play();

								Retreat(selectedUnit.gameObject);

							}
							selectedUnit.Attack = (currentCell.Unit.Defense - selectedUnit.Attack + selectedUnit.Defense) / 2;
							selectedUnit.Defense = (currentCell.Unit.Defense - selectedUnit.Attack + selectedUnit.Defense) / 2;
							
						}
					}
                    else
                    {
						grid.RemoveUnit(selectedUnit);
						return;
					}
				}
			}


			AudioClip clip = GetAudioClip(gameManager.GetComponent<GameManager>().Moves);
			if (clip != null)
			{
				selectedUnit.transform.gameObject.GetComponent<AudioSource>().clip = clip;
				selectedUnit.transform.gameObject.GetComponent<AudioSource>().Play();
			}

			selectedUnit.Actions--;
			selectedUnit.Travel(grid.GetPath());
			grid.ClearPath();
		}
	}

	bool UpdateCurrentCell()
	{
		HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if (cell != currentCell)
		{
			currentCell = cell;
			return true;
		}
		return false;
	}

	void Battle()
    {
		Time.timeScale = 0f;
		transform.Find("BattleScreen").gameObject.SetActive(true);
		transform.Find("BattleScreen").gameObject.transform.GetChild(1).GetComponent<Text>().text = selectedUnit.ParentEmpire.GetComponent<Empire>().empireName + "         "  +
				"Allied Forces : " + selectedUnit.Attack; 
		transform.Find("BattleScreen").gameObject.transform.GetChild(2).GetComponent<Text>().text = currentCell.Unit.ParentEmpire.GetComponent<Empire>().empireName + "        " +
				"Enemy Forces : " + currentCell.Unit.Defense; 
	}

	public void ExitBattle()
    {
		Time.timeScale = 1f;

		transform.Find("BattleScreen").gameObject.transform.GetChild(1).GetComponent<Text>().text = "";
		transform.Find("BattleScreen").gameObject.transform.GetChild(2).GetComponent<Text>().text = "";
		transform.Find("BattleScreen").gameObject.transform.GetChild(3).gameObject.SetActive(false);

		StartCoroutine(BattleResults());

		
	}
	IEnumerator BattleResults()
    {
		yield return new WaitForSeconds(3f);

		if (selectedUnit.Attack > currentCell.Unit.Defense)
		{
			transform.Find("BattleScreen").GetChild(0).gameObject.GetComponentInChildren<Text>().text = "VICTORY";
		}
		else
		{
			transform.Find("BattleScreen").GetChild(0).gameObject.GetComponentInChildren<Text>().text = "DEFEAT";
		}

		transform.Find("BattleScreen").gameObject.transform.GetChild(3).gameObject.SetActive(true);
		transform.Find("BattleScreen").gameObject.SetActive(false);
		
	}


	void Retreat(GameObject agent)
	{
		HexCell retreat = FindFurthestTile(agent);

		if (retreat != null)
		{
			agent.GetComponent<GoapAgent>().map.FindPath(transform.GetComponent<HexUnit>().Location, retreat, transform.GetComponent<HexUnit>());
			transform.GetComponent<HexUnit>().Travel(agent.GetComponent<GoapAgent>().map.GetPath());
		}
		else
		{
			agent.GetComponent<GoapAgent>().map.RemoveUnit(agent.GetComponent<HexUnit>());
			return;
		}
	}

	HexCell FindFurthestTile(GameObject agent)
	{
		HexCell furthest = null;

		float maxDis = -Mathf.Infinity;

		List<HexCell> visible = agent.GetComponent<GoapAgent>().map.GetVisibleCells(transform.gameObject.GetComponent<HexUnit>().Location, transform.gameObject.GetComponent<HexUnit>().VisionRange);

		foreach (HexCell cell in visible)
		{
			if (agent.GetComponent<HexUnit>().type != "Boat")
			{
				if (!agent.GetComponent<HexUnit>().Location.IsUnderwater && agent.GetComponent<HexUnit>().Location.Unit == null &&
					!agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AtPeace.Contains(cell.owner.GetComponent<Empire>().empireName) &&
					agent.GetComponent<HexUnit>().Location.coordinates.DistanceTo(cell.coordinates) > maxDis)
				{
					furthest = cell;
					maxDis = agent.GetComponent<HexUnit>().Location.coordinates.DistanceTo(cell.coordinates);

				}

			}
			else
			{
				if (agent.GetComponent<HexUnit>().Location.IsUnderwater && agent.GetComponent<HexUnit>().Location.Unit == null &&
					!agent.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().AtPeace.Contains(cell.owner.GetComponent<Empire>().empireName) &&
					agent.GetComponent<HexUnit>().Location.coordinates.DistanceTo(cell.coordinates) > maxDis)
				{
					furthest = cell;
					maxDis = agent.GetComponent<HexUnit>().Location.coordinates.DistanceTo(cell.coordinates);

				}
			}

		}


		return furthest;
	}
}