using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Map : MonoBehaviour {

	public Material terrainMaterial;

	public int cellCountX = 60, cellCountZ = 45;

	public int seed;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;
	public HexGridChunk chunkPrefab;
	public HexUnit unitPrefab;
	public HexUnit workerPrefab;
	public HexUnit boatPrefab;
	public HexUnit settlerPrefab;
	public HexUnit catapultPrefab;

	public bool HasPath
	{
		get
		{
			return currentPathExists;
		}
	}

	HexGridChunk[] chunks;
	HexCell[] cells;
	public GameObject manager;
	public Texture2D image;
	public Texture2D heightMap;
	public Texture2D riverMap;
	public Texture2D featureMap;
	public Texture2D noiseSource;

	int chunkCountX, chunkCountZ;

	int waterLevel = 1;

	int activeUrbanLevel, activeFarmLevel, activePlantLevel;

	// pathfinding
	HexCellPriorityQueue searchFrontier;

	int searchFrontierPhase;

	HexCell currentPathFrom, currentPathTo;
	bool currentPathExists;
	public List<HexCell> cities = new List<HexCell>();
	public List<HexUnit> units = new List<HexUnit>();
	public List<HexUnit> workers = new List<HexUnit>();
	public List<HexUnit> Settlers = new List<HexUnit>();
	public List<HexUnit> Catapults = new List<HexUnit>();
	List<Color> colorList = new List<Color>()
	 {
		 Color.red,
		 Color.white,
		 Color.blue,
		 Color.black,
		 Color.green,
		 Color.magenta,
		 Color.cyan

	 };

	HexCellShaderData cellShaderData;


	void Awake () {
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.InitializeHashGrid(seed);
		HexUnit.unitPrefab = unitPrefab;
		HexUnit.workerPrefab = workerPrefab;
		HexUnit.settlerPrefab = settlerPrefab;
		HexUnit.catapultPrefab = catapultPrefab;

		HexUnit.manager = manager;
		cellShaderData = gameObject.AddComponent<HexCellShaderData>();
		cellShaderData.Grid = this;
		CreateMap(cellCountX, cellCountZ);
		ShowGrid(false);
		//Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
		Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
	}

	/*
    void Update()
    {
		for (int i = 0; i< cells.Length; i++)
        {
			cells[i].SetLabel(cells[i].coordinates.ToStringOnSeparateLines());
		}
    }
	*/

    public void AddUnit(HexUnit unit, HexCell location, float orientation, GameObject empire, string type)
	{
		if (type == "Soldier")
			units.Add(unit);
		if (type == "Worker")
			workers.Add(unit);
		if (type == "Catapult")
			Catapults.Add(unit);
		if (type == "Settler")
			Settlers.Add(unit);

		unit.Grid = this;
		unit.transform.SetParent(transform, false);
		unit.Location = location;
		unit.Orientation = orientation;
		unit.ParentEmpire = empire;
		unit.type = type;
		unit.ParentEmpire.GetComponent<Empire>().units.Add(unit);
		unit.isPlayerUnit = manager.GetComponent<GameManager>().GetPlayerEmpire() == empire;
	}

	public void RemoveUnit(HexUnit unit)
	{
		if (unit.type == "Soldier")
        {
			unit.ParentEmpire.GetComponent<Empire>().soldiers--;
			units.Remove(unit);
		}

		if (unit.type == "Worker")
        {
			unit.ParentEmpire.GetComponent<Empire>().workers--;
			workers.Remove(unit);
		}

		if (unit.type == "Catapult")
        {
			unit.ParentEmpire.GetComponent<Empire>().ranged--;
			Catapults.Remove(unit);
		}

		if (unit.type == "Settler")
        {
			unit.ParentEmpire.GetComponent<Empire>().settlers--;
			Settlers.Remove(unit);
		}

		unit.ParentEmpire.GetComponent<Empire>().units.Remove(unit);

		unit.Die();
	}

	public bool CreateMap(int x, int z)
    {
		if (x <= 0 || x % HexMetrics.chunkSizeX != 0 || z <= 0 || z % HexMetrics.chunkSizeZ != 0)
		{
			Debug.LogError("Unsupported map size.");
			return false;
		}

		ClearPath();
		ClearUnits();

		if (chunks != null)
		{
			for (int i = 0; i < chunks.Length; i++)
			{
				Destroy(chunks[i].gameObject);
			}
		}

		cellCountX = x;
		cellCountZ = z;
		chunkCountX = cellCountX / HexMetrics.chunkSizeX;
		chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;
		cellShaderData.Initialize(cellCountX, cellCountZ);
		CreateChunks();
		CreateCells();
		CreateRivers();
		CreateFeatures();
		return true;
	}

	void CreateChunks()
	{
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++)
		{
			for (int x = 0; x < chunkCountX; x++)
			{
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
	}

	public void ShowGrid(bool visible)
	{
		if (visible)
		{
			terrainMaterial.EnableKeyword("GRID_ON");
		}
		else
		{
			terrainMaterial.DisableKeyword("GRID_ON");
		}
	}

	void ClearUnits()
	{
		for (int i = 0; i < units.Count; i++)
		{
			units[i].Die();
		}

		for (int i = 0; i < workers.Count; i++)
		{
			workers[i].Die();
		}

		for (int i = 0; i < Settlers.Count; i++)
		{
			Settlers[i].Die();
		}

		for (int i = 0; i < Catapults.Count; i++)
		{
			Catapults[i].Die();
		}


		units.Clear();
		workers.Clear();
		Settlers.Clear();
		Catapults.Clear();
	}

	void OnEnable()
	{
		if (!HexMetrics.noiseSource)
		{
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
			HexUnit.unitPrefab = unitPrefab;
			HexUnit.workerPrefab = workerPrefab;
			HexUnit.settlerPrefab = settlerPrefab;
			HexUnit.catapultPrefab = catapultPrefab; 
			ResetVisibility();
		}
	}

	public HexCell GetCell(Ray ray)
	{
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			return GetCell(hit.point);
		}
		return null;
	}

	public HexCell GetCell(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		if (index >= 0 && index < cells.Length)
			return cells[index];
		else
			return null;
	}

	public HexCell GetCell(HexCoordinates coordinates)
	{
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ)
		{
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX)
		{
			return null;
		}
		return cells[x + z * cellCountX];
	}

	public void ShowUI(bool visible)
	{
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].ShowUI(visible);
		}
	}

	void CreateCell(int x, int z, int i)
	{
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.Index = i;
		cell.ShaderData = cellShaderData;

		Color TerrainColor = image.GetPixel((x * (image.width - image.width / chunkCountX / HexMetrics.chunkSizeX / 2) / chunkCountX / HexMetrics.chunkSizeX), z * ((image.height - image.height / chunkCountZ / HexMetrics.chunkSizeZ) / chunkCountZ / HexMetrics.chunkSizeZ));
		
		checkTerrainType(cell, TerrainColor);
		cell.WaterLevel = waterLevel;
		
		cell.Explorable = x >= 0 && z >= 0 && x <= cellCountX - 1 && z <= cellCountZ - 1;

		if (x > 0)
		{
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
		}
		if (z > 0)
		{
			if ((z & 1) == 0)
			{
				cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
				if (x > 0)
				{
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
				}
			}
			else
			{
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1)
				{
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
			}
		}

		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		cell.uiRect = label.rectTransform;

		cell.Elevation = 0; 

		if (cell.TerrainTypeIndex != 2)
        {
			cell.Elevation += Mathf.RoundToInt(heightMap.GetPixel((x * (heightMap.width - heightMap.width / chunkCountX / HexMetrics.chunkSizeX / 2) / chunkCountX / HexMetrics.chunkSizeX), z * ((heightMap.height - heightMap.height / chunkCountZ / HexMetrics.chunkSizeZ) / chunkCountZ / HexMetrics.chunkSizeZ)).grayscale * 4.1f);
			
			if (cell.Elevation > 3f)
            {
				cell.TerrainTypeIndex = 8;
            }
			
		}

		AddCellToChunk(x, z, cell);
	}


	void checkTerrainType(HexCell cell, Color terrain)
    {
		cell.TerrainTypeIndex = closestColor(colorList, terrain);		
	}

	int closestColor(List<Color> colors, Color target)
	{
		var colorDiffs = colors.Select(n => ColorDiff(n, target)).Min(n => n);
		return colors.FindIndex(n => ColorDiff(n, target) == colorDiffs);
	}

	int ColorDiff(Color c1, Color c2)
	{
		return (int)Mathf.Sqrt((c1.r - c2.r) * (c1.r - c2.r) + (c1.g - c2.g) * (c1.g - c2.g)   + (c1.b - c2.b) * (c1.b - c2.b));
	}

	void AddCellToChunk(int x, int z, HexCell cell)
	{
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}

	void CreateCells()
	{
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++)
		{
			for (int x = 0; x < cellCountX; x++)
			{
				CreateCell(x, z, i++);
			}
		}
	}

	void CreateRivers()
    {
		for (int z = 0, i = 0; z < cellCountZ; z++)
		{
			for (int x = 0; x < cellCountX; x++)
			{
				HexCell cell = cells[i];
				i++;
				Color colorDir = riverMap.GetPixel((x * (riverMap.width - riverMap.width / chunkCountX / HexMetrics.chunkSizeX / 2) / chunkCountX / HexMetrics.chunkSizeX), z * ((riverMap.height - riverMap.height / chunkCountZ / HexMetrics.chunkSizeZ) / chunkCountZ / HexMetrics.chunkSizeZ));

				HexDirection riverDirection = 0;
				
				if (colorDir == Color.cyan)
				{
					riverDirection = HexDirection.NE;
					SetRiver(cell, riverDirection);

				}
				if (colorDir == Color.green)
				{
					riverDirection = HexDirection.E;
					SetRiver(cell, riverDirection);
				}
				if (colorDir == Color.blue)
				{
					riverDirection = HexDirection.SE;
					SetRiver(cell, riverDirection);
				}
				if (colorDir == Color.red)
				{
					riverDirection = HexDirection.SW;
					SetRiver(cell, riverDirection);
				}

				if (colorDir == Color.white)
				{
					riverDirection = HexDirection.W;
					SetRiver(cell, riverDirection);
				}

				if (colorDir == Color.magenta)
				{
					riverDirection = HexDirection.NW;
					SetRiver(cell, riverDirection);
				}
			}
		}
	}

	void SetRiver(HexCell cell, HexDirection direction)
    {
		HexCell otherCell = cell.GetNeighbor(direction.Opposite());
		if (otherCell)
		{
			otherCell.SetOutgoingRiver(direction);
		}
	}

	void CreateFeatures()
    {
		for (int z = 0, i = 0; z < cellCountZ; z++)
		{
			for (int x = 0; x < cellCountX; x++)
			{
				HexCell cell = cells[i];
				i++;

				Color featureColor = featureMap.GetPixel((x * (featureMap.width - featureMap.width / chunkCountX / HexMetrics.chunkSizeX / 2) / chunkCountX / HexMetrics.chunkSizeX), z * ((featureMap.height - featureMap.height / chunkCountZ / HexMetrics.chunkSizeZ) / chunkCountZ / HexMetrics.chunkSizeZ));

				if (cell.TerrainTypeIndex == 1)
				{
					int prob = Random.Range(0, 25);

					if (prob > 10)
						cell.PlantLevel = 2;
				}

				if (cell.TerrainTypeIndex == 5)
				{

					int prob = Random.Range(0, 25);

					if (prob > 10)
						cell.PlantLevel = 1;
				}

				if (cell.TerrainTypeIndex == 4)
				{

					int prob = Random.Range(0, 25);

					if (prob > 10)
						cell.PlantLevel = 2;
				}

				if (featureColor == Color.red)
                {
					cell.PlantLevel = 3;
                }


				if (featureColor == Color.blue)
				{
					cell.SpecialIndex = 1;
					cell.Walled = true;
					cities.Add(cell);
					
				}

				if (featureColor == Color.green)
                {
					cell.ResourceIndex = 1;
                }

				if (featureColor == Color.cyan)
				{
					cell.ResourceIndex = 2;
				}

				if (featureColor == Color.magenta)
				{
					cell.ResourceIndex = 3;
				}

				if (featureColor == Color.white)
				{
					cell.ResourceIndex = 4;
				}
			}
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(cellCountX);
		writer.Write(cellCountZ);

		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].Save(writer);
		}

		writer.Write(units.Count);
		for (int i = 0; i < units.Count; i++)
		{
			units[i].Save(writer);
		}

		writer.Write(workers.Count);
		for (int i = 0; i < workers.Count; i++)
		{
			workers[i].Save(writer);
		}

		writer.Write(Settlers.Count);
		for (int i = 0; i < Settlers.Count; i++)
		{
			Settlers[i].Save(writer);
		}

		writer.Write(Catapults.Count);
		for (int i = 0; i < Catapults.Count; i++)
		{
			Catapults[i].Save(writer);
		}
	}

	public void Load(BinaryReader reader, int header)
	{
		ClearPath();
		ClearUnits();
		int x = 20, z = 15;
		if (header >= 1)
		{
			x = reader.ReadInt32();
			z = reader.ReadInt32();
		}
		if (x != cellCountX || z != cellCountZ)
		{
			if (!CreateMap(x, z))
			{
				return;
			}
		}

		bool originalImmediateMode = cellShaderData.ImmediateMode;
		cellShaderData.ImmediateMode = true;

		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].Load(reader, header);
		}
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].Refresh();
		}

		if (header >= 2)
		{
			int unitCount = reader.ReadInt32();
			for (int i = 0; i < unitCount; i++)
			{
				HexUnit.Load(reader, this);
			}

			int workerCount = reader.ReadInt32();
			for (int i = 0; i < workerCount; i++)
			{
				HexUnit.Load(reader, this);
			}

			int settlerCount = reader.ReadInt32();
			for (int i = 0; i < settlerCount; i++)
			{
				HexUnit.Load(reader, this);
			}

			int catapultCount = reader.ReadInt32();
			for (int i = 0; i < catapultCount; i++)
			{
				HexUnit.Load(reader, this);
			}
		}

		cellShaderData.ImmediateMode = originalImmediateMode;

		foreach(GameObject empire in manager.GetComponent<GameManager>().ReturnEmpires())
        {
			if (empire.GetComponent<Empire>().status == "Capitulated")
				continue;

			foreach(HexCell cell in cells)
            {
				if (cell.empireIndex != -1 && cell.empireIndex == empire.GetComponent<Empire>().EmpireIndex && !empire.GetComponent<Empire>().empireCells.Contains(cell))
                {
					cell.owner = empire;
					empire.GetComponent<Empire>().empireCells.Add(cell);
                }
            }
        }

	}

	public List<HexCell> GetPath()
	{
		if (!currentPathExists)
		{
			return null;
		}
		List<HexCell> path = ListPool<HexCell>.Get();
		for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
		{
			path.Add(c);
		}
		path.Add(currentPathFrom);
		path.Reverse();
		return path;
	}

	public void ClearPath()
	{
		if (currentPathExists)
		{
			HexCell current = currentPathTo;
			while (current != currentPathFrom)
			{
				current.SetLabel(null);
				current.DisableHighlight();
				current = current.PathFrom;
			}
			current.DisableHighlight();
			currentPathExists = false;
		}
		else if (currentPathFrom)
		{
			currentPathFrom.DisableHighlight();
			currentPathTo.DisableHighlight();
		}
		currentPathFrom = currentPathTo = null;
	}

	void ShowPath(int speed)
	{
		if (currentPathExists)
		{
			HexCell current = currentPathTo;
			while (current != currentPathFrom)
			{
				int turn = (current.Distance - 1) / speed + 1;
				current.SetLabel(turn.ToString());
				current.EnableHighlight(Color.white);
				current = current.PathFrom;
			}
		}
		currentPathFrom.EnableHighlight(Color.blue);
		currentPathTo.EnableHighlight(Color.green);

	}

	public void FindPath(HexCell fromCell, HexCell toCell, HexUnit unit)
	{
		ClearPath();
		currentPathFrom = fromCell;
		currentPathTo = toCell;

		if (unit.isPlayerUnit && currentPathFrom.coordinates.DistanceTo(currentPathTo.coordinates) > unit.Actions)
			return;

		currentPathExists = Search(fromCell, toCell, unit);

		if (unit.isPlayerUnit)
			ShowPath(unit.Speed);
	}

	bool Search(HexCell fromCell, HexCell toCell, HexUnit unit)
	{
		int speed = unit.Speed;
		searchFrontierPhase += 2;
		if (searchFrontier == null)
		{
			searchFrontier = new HexCellPriorityQueue();
		}
		else
		{
			searchFrontier.Clear();
		}

		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);

		while (searchFrontier.Count > 0)
		{
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;

			if (current == toCell)
			{
				return true;
			}

			int currentTurn = (current.Distance - 1) / speed;

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				HexCell neighbor = current.GetNeighbor(d);
				if (
					neighbor == null ||
					neighbor.SearchPhase > searchFrontierPhase
				)
				{
					continue;
				}
				if (!unit.IsValidDestination(neighbor))
				{
					continue;
				}
				int moveCost = unit.GetMoveCost(current, neighbor, d);
				if (moveCost < 0)
				{
					continue;
				}

				int distance = current.Distance + moveCost;
				int turn = (distance - 1) / speed;
				if (turn > currentTurn)
				{
					distance = turn * speed + moveCost;
				}

				if (neighbor.SearchPhase < searchFrontierPhase)
				{
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					neighbor.SearchHeuristic =
						neighbor.coordinates.DistanceTo(toCell.coordinates);
					searchFrontier.Enqueue(neighbor);
				}
				else if (distance < neighbor.Distance)
				{
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
		return false;
	}


	public void IncreaseVisibility(HexCell fromCell, int range)
	{
		List<HexCell> cells = GetVisibleCells(fromCell, range);
		for (int i = 0; i < cells.Count; i++)
		{
			cells[i].IncreaseVisibility();
		}
		ListPool<HexCell>.Add(cells);
	}

	public void DecreaseVisibility(HexCell fromCell, int range)
	{
		List<HexCell> cells = GetVisibleCells(fromCell, range);
		for (int i = 0; i < cells.Count; i++)
		{
			cells[i].DecreaseVisibility();
		}
		ListPool<HexCell>.Add(cells);
	}

	public void ResetVisibility()
	{
		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].ResetVisibility();
		}
		for (int i = 0; i < units.Count; i++)
		{
			HexUnit unit = units[i];
			IncreaseVisibility(unit.Location, unit.VisionRange);
		}
	}

	public List<HexCell> GetVisibleCells(HexCell fromCell, int range)
	{
		List<HexCell> visibleCells = ListPool<HexCell>.Get();

		searchFrontierPhase += 2;
		if (searchFrontier == null)
		{
			searchFrontier = new HexCellPriorityQueue();
		}
		else
		{
			searchFrontier.Clear();
		}

		range += fromCell.ViewElevation;
		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		HexCoordinates fromCoordinates = fromCell.coordinates;
		while (searchFrontier.Count > 0)
		{
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;
			visibleCells.Add(current);

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				HexCell neighbor = current.GetNeighbor(d);
				if (
					neighbor == null ||
					neighbor.SearchPhase > searchFrontierPhase ||
					!neighbor.Explorable
				)
				{
					continue;
				}

				int distance = current.Distance + 1;
				if (distance + neighbor.ViewElevation > range ||
					distance > fromCoordinates.DistanceTo(neighbor.coordinates)
				)
				{
					continue;
				}

				if (neighbor.SearchPhase < searchFrontierPhase)
				{
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.SearchHeuristic = 0;
					searchFrontier.Enqueue(neighbor);
				}
				else if (distance < neighbor.Distance)
				{
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
		return visibleCells;
	}
}