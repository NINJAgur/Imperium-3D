using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class HexUnit : MonoBehaviour {

	const float rotationSpeed = 180f;
	const float travelSpeed = 4f;

	public int Actions;

	public static HexUnit unitPrefab;
	public static HexUnit workerPrefab;
	public static HexUnit settlerPrefab;
	public static HexUnit catapultPrefab;
	public static GameObject manager;
	public bool isPlayerUnit;

	public Material defaultMat;

	public Map Grid { get; set; }

	public GameObject ParentEmpire;
	[HideInInspector]
	public int ParentEmpireIndex;
	public string type;
	public List<HexCell> visibile;

	public int Attack;
	public int Defense;
	public int ManpowerCapacity;
	public int currentSoldiersOnBoard;
	public int currentWorkersOnBoard;

	public bool Embarked = false;

	void Awake()
	{
		Actions = 3;
	}

    void Start()
    {
		if (type == "Soldier")
		{
			Attack = 100;
			Defense = 100;
		}

		if (type == "Catapult")
		{
			Attack = 200;
			Defense = 50;
		}

		if (type == "Worker" || type == "Settler")
			Attack = Defense = 0;

		transform.GetChild(3).GetComponent<MeshRenderer>().material = new Material(defaultMat);
		transform.GetChild(3).GetComponent<MeshRenderer>().material.color = ParentEmpire.GetComponent<Empire>().empireColor;
		transform.GetChild(4).GetComponent<MeshRenderer>().material = new Material(defaultMat);
		transform.GetChild(4).GetComponent<MeshRenderer>().material.color = ParentEmpire.GetComponent<Empire>().empireColor;
	}

    void Update()
    {
		transform.GetChild(0).GetComponent<TextMesh>().text = "Attack : " + Attack;
		transform.GetChild(1).GetComponent<TextMesh>().text = "Defense : " + Defense;
		checkUnitinRange();
	}

	public void ApplyBuffs()
	{
		if (type == "Soldier")
		{
			if (ParentEmpire.GetComponent<Empire>().treeSoldier == "Left")
			{
				if (!ParentEmpire.GetComponent<Empire>().empireCells.Contains(Location))
				{
					Attack += Attack * (ParentEmpire.GetComponent<Empire>().soliderBuffs[0] / 100);
				}
			}

			if (ParentEmpire.GetComponent<Empire>().treeSoldier == "Middle")
			{
				VisionRange += VisionRange * (ParentEmpire.GetComponent<Empire>().soliderBuffs[0] / 100);
				Speed += Speed * (ParentEmpire.GetComponent<Empire>().soliderBuffs[1] / 100);
			}

			if (ParentEmpire.GetComponent<Empire>().treeSoldier == "Right")
			{
				if (ParentEmpire.GetComponent<Empire>().empireCells.Contains(Location))
				{
					Defense += Defense * (ParentEmpire.GetComponent<Empire>().soliderBuffs[0] / 100);
				}
			}
		}

		/*
		if (type == "Boat")
		{
			if (ParentEmpire.GetComponent<Empire>().treeBoat == "Left")
			{
				ManpowerCapacity += (ParentEmpire.GetComponent<Empire>().boatrBuffs[0] / 10);
				Speed += Speed * (ParentEmpire.GetComponent<Empire>().boatrBuffs[1] / 100);
			}

			if (ParentEmpire.GetComponent<Empire>().treeBoat == "Right")
			{
				Attack += Attack * (ParentEmpire.GetComponent<Empire>().boatrBuffs[0] / 100);
				VisionRange += VisionRange * (ParentEmpire.GetComponent<Empire>().boatrBuffs[1] / 100);
			}
		}
		*/
	}

	void checkUnitinRange()
    {
		visibile = Grid.GetVisibleCells(transform.gameObject.GetComponent<HexUnit>().Location, VisionRange);

		bool playerUnitNearby = false;

		foreach (HexCell cell in visibile)
		{
			if (cell.Explorable && !ParentEmpire.GetComponent<Empire>().exploredCells.Contains(cell))
				ParentEmpire.GetComponent<Empire>().exploredCells.Add(cell);

			if (cell.owner != null && cell.owner != ParentEmpire)
			{
				if (!ParentEmpire.GetComponent<Empire>().knownEmpires.Contains(cell.owner.GetComponent<Empire>().empireName))
				{
					ParentEmpire.GetComponent<Empire>().dipEmpires.Add(cell.owner);
					ParentEmpire.GetComponent<Empire>().CreateEmpireIntel(cell.owner.GetComponent<Empire>().empireName);

					cell.owner.GetComponent<Empire>().dipEmpires.Add(ParentEmpire);
					cell.owner.GetComponent<Empire>().CreateEmpireIntel(ParentEmpire.GetComponent<Empire>().empireName);
				}

				if (!transform.gameObject.GetComponent<HexUnit>().ParentEmpire.GetComponent<Empire>().UpdateIntel(cell.owner.GetComponent<Empire>().empireName).knownTerritories.Contains(cell))
					ParentEmpire.GetComponent<Empire>().UpdateIntel(cell.owner.GetComponent<Empire>().empireName).knownTerritories.Add(cell);

			}

			if (cell.Unit != null && cell.Unit.ParentEmpire != ParentEmpire)
			{
				if (!ParentEmpire.GetComponent<Empire>().knownEmpires.Contains(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName))
				{
					ParentEmpire.GetComponent<Empire>().dipEmpires.Add(cell.Unit.ParentEmpire);
					ParentEmpire.GetComponent<Empire>().CreateEmpireIntel(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName);

					cell.Unit.ParentEmpire.GetComponent<Empire>().dipEmpires.Add(ParentEmpire);
					cell.Unit.ParentEmpire.GetComponent<Empire>().CreateEmpireIntel(ParentEmpire.GetComponent<Empire>().empireName);
				}

				if (cell.Unit.isPlayerUnit && !isPlayerUnit)
					playerUnitNearby = true;

				if (!ParentEmpire.GetComponent<Empire>().UpdateIntel(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName).knownUnits.Contains(cell.Unit))
					ParentEmpire.GetComponent<Empire>().UpdateIntel(cell.Unit.ParentEmpire.GetComponent<Empire>().empireName).knownUnits.Add(cell.Unit);
			}

			if (playerUnitNearby || isPlayerUnit)
            {
				transform.GetChild(0).gameObject.SetActive(true);
				transform.GetChild(1).gameObject.SetActive(true);
				transform.GetChild(3).gameObject.SetActive(true);
				transform.GetChild(4).gameObject.SetActive(true);
				transform.GetChild(5).gameObject.SetActive(true);
				transform.GetChild(6).gameObject.SetActive(true);
			}
            else
            {
				transform.GetChild(0).gameObject.SetActive(false);
				transform.GetChild(1).gameObject.SetActive(false);
				transform.GetChild(3).gameObject.SetActive(false);
				transform.GetChild(4).gameObject.SetActive(false);
				transform.GetChild(5).gameObject.SetActive(false);
				transform.GetChild(6).gameObject.SetActive(false);
			}

		}

	}



	public HexCell Location {
		get {
			return location;
		}
		set {
			if (location) 
			{
				if (isPlayerUnit)
					Grid.DecreaseVisibility(location, VisionRange);
				//location.Unit = null;
			}
			location = value;
			value.Unit = this;
			if (isPlayerUnit)
				Grid.IncreaseVisibility(value, VisionRange);
			transform.localPosition = value.Position;
		}
	}

	HexCell location, currentTravelLocation;

	public float Orientation {
		get {
			return orientation;
		}
		set {
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	public HexDirection getDirection()
    {
		if (Orientation > 0 && Orientation <= 40)
			return HexDirection.NE;

		if (Orientation > 40 && Orientation <= 100)
			return HexDirection.E;

		if (Orientation > 100 && Orientation <= 175)
			return HexDirection.SE;

		if (Orientation > 185 && Orientation <= 250)
			return HexDirection.SW;

		if (Orientation > 250 && Orientation <= 300)
			return HexDirection.W;

		if (Orientation > 300 && Orientation < 360)
			return HexDirection.NW;

		return HexDirection.NE;
    }
	
	public int Speed = 24;

	public int VisionRange = 3;

	float orientation;

	List<HexCell> pathToTravel;
	
	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	public bool IsValidDestination (HexCell cell) 
	{
		if (cell.Unit != null && cell.Unit.ParentEmpire == ParentEmpire)
			return false;

		if (ParentEmpire.GetComponent<Empire>().exploredCells.Contains(cell) && !checkBelongsToEmpire(cell))
			return true;
		else
			return false;
	}

	bool checkBelongsToEmpire(HexCell target)
    {
		foreach (GameObject empire in manager.GetComponent<GameManager>().ReturnEmpires())
        {
			if (empire != ParentEmpire)
            {
				if (empire.GetComponent<Empire>().empireCells.Contains(target))
                {
					if (!empire.GetComponent<Empire>().AlliedWith.Contains(ParentEmpire.GetComponent<Empire>().empireName) &&
						!empire.GetComponent<Empire>().MAWith.Contains(ParentEmpire.GetComponent<Empire>().empireName) &&
						!empire.GetComponent<Empire>().AtWar.Contains(ParentEmpire.GetComponent<Empire>().empireName))
                    {
						return true;
                    }
                }
			}
        }

		return false;
    }

	public void Travel (List<HexCell> path) {
		location.Unit = null;
		location = path[path.Count - 1];
		location.Unit = this;
		pathToTravel = path;
		StopAllCoroutines();
		StartCoroutine(TravelPath());
	}

	IEnumerator TravelPath () {
		Vector3 a, b, c = pathToTravel[0].Position;

		yield return LookAt(pathToTravel[1].Position);

		if (!Embarked)
		{
			transform.GetComponentInChildren<Animator>().SetBool("isMoving", true);
			transform.GetComponentInChildren<Animator>().SetTrigger("Move");

			if (type == "Soldier")
            {
				transform.GetComponentInChildren<Animation>().clip = transform.GetComponentInChildren<Animation>().GetClip("walk");
				transform.GetComponentInChildren<Animation>().Play();

			}
		}

		if (isPlayerUnit)
			Grid.DecreaseVisibility(currentTravelLocation ? currentTravelLocation : pathToTravel[0],VisionRange);
			

		float t = Time.deltaTime * travelSpeed;
		for (int i = 1; i < pathToTravel.Count; i++) 
		{
			currentTravelLocation = pathToTravel[i];

			if (currentTravelLocation.IsUnderwater)
            {
				transform.GetChild(7).gameObject.SetActive(true);
				transform.GetChild(2).gameObject.SetActive(false);
				Embarked = true;
			}
			else
            {
				transform.GetChild(7).gameObject.SetActive(false);
				transform.GetChild(2).gameObject.SetActive(true);
				Embarked = false;
			}

			a = c;
			b = pathToTravel[i - 1].Position;
			c = (b + currentTravelLocation.Position) * 0.5f;
			if (isPlayerUnit)
				Grid.IncreaseVisibility(pathToTravel[i], VisionRange);
			for (; t < 1f; t += Time.deltaTime * travelSpeed) {
				transform.localPosition = Bezier.GetPoint(a, b, c, t);
				Vector3 d = Bezier.GetDerivative(a, b, c, t);
				d.y = 0f;

				if (d != Vector3.zero)
					transform.localRotation = Quaternion.LookRotation(d);
				yield return null;
			}
			if (isPlayerUnit)
				Grid.DecreaseVisibility(pathToTravel[i], VisionRange);
			t -= 1f;
		}
		currentTravelLocation = null;

		a = c;
		b = location.Position;
		c = b;
		if (isPlayerUnit)
			Grid.IncreaseVisibility(location, VisionRange);
		for (; t < 1f; t += Time.deltaTime * travelSpeed) {
			transform.localPosition = Bezier.GetPoint(a, b, c, t);
			Vector3 d = Bezier.GetDerivative(a, b, c, t);
			d.y = 0f;
			transform.localRotation = Quaternion.LookRotation(d);
			yield return null;
		}

		transform.localPosition = location.Position;
		orientation = transform.localRotation.eulerAngles.y;
		ListPool<HexCell>.Add(pathToTravel);
		pathToTravel = null;
		
		if (!Embarked)
        {
			transform.GetComponentInChildren<Animator>().SetBool("isMoving", false);

			if (type == "Soldier")
			{
				transform.GetComponentInChildren<Animation>().clip = transform.GetComponentInChildren<Animation>().GetClip("idle");
				transform.GetComponentInChildren<Animation>().Play();
			}
		}
		
	}

	IEnumerator LookAt (Vector3 point) {
		point.y = transform.localPosition.y;
		Quaternion fromRotation = transform.localRotation;
		Quaternion toRotation =
			Quaternion.LookRotation(point - transform.localPosition);
		float angle = Quaternion.Angle(fromRotation, toRotation);

		if (angle > 0f) {
			float speed = rotationSpeed / angle;
			for (
				float t = Time.deltaTime * speed;
				t < 1f;
				t += Time.deltaTime * speed
			) {
				transform.localRotation =
					Quaternion.Slerp(fromRotation, toRotation, t);
				yield return null;
			}
		}

		transform.LookAt(point);
		orientation = transform.localRotation.eulerAngles.y;
	}

	public int GetMoveCost (
		HexCell fromCell, HexCell toCell, HexDirection direction)
	{
		if (!IsValidDestination(toCell)) {
			return -1;
		}
		HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
		if (edgeType == HexEdgeType.Cliff) {
			return -1;
		}
		int moveCost;
		if (fromCell.HasRoadThroughEdge(direction)) {
			moveCost = 1;
		}
		else {
			moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
			moveCost +=
				toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
		}

		return moveCost;
	}

	public void Die () {
		if (location) {
			if (isPlayerUnit)
				Grid.DecreaseVisibility(location, VisionRange);
		}
		location.Unit = null;
		Destroy(gameObject);
	}

	public void Save (BinaryWriter writer) 
	{
		location.coordinates.Save(writer);
		writer.Write(orientation);
		writer.Write(type);
		ParentEmpireIndex = ParentEmpire.GetComponent<Empire>().EmpireIndex;
		writer.Write((byte)ParentEmpireIndex);
	}

	public static void Load (BinaryReader reader, Map grid) {
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		float orientation = reader.ReadSingle();
		string type = reader.ReadString();

		HexUnit currentPrefab = null;

		Debug.Log(type);

		if (type == "Soldier")
			currentPrefab = unitPrefab;
		if (type == "Worker")
			currentPrefab = workerPrefab;
		if (type == "Catapult")
			currentPrefab = catapultPrefab;
		if (type == "Settler")
			currentPrefab = settlerPrefab;

		grid.AddUnit(Instantiate(currentPrefab), grid.GetCell(coordinates), orientation, manager.GetComponent<GameManager>().ReturnEmpires()[reader.ReadByte()], type);
	}

	void OnEnable () 
	{
		if (location) 
		{
			transform.localPosition = location.Position;
			if (currentTravelLocation) 
			{
				if (isPlayerUnit)
                {
					Grid.IncreaseVisibility(location, VisionRange);
					Grid.DecreaseVisibility(currentTravelLocation, VisionRange);

				}
				currentTravelLocation = null;
			}
		}
	}
}