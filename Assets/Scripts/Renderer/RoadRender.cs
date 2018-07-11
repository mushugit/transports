using UnityEngine;

public class RoadRender : MonoBehaviour
{

	public GameObject road_N;
	public GameObject road_E;
	public GameObject road_S;
	public GameObject road_W;

	public GameObject turn_NE;
	public GameObject turn_NW;
	public GameObject turn_SE;
	public GameObject turn_SW;

	public GameObject roundabout;

	bool NorthNeighbor { get; set; } = false;
	bool EastNeighbor { get; set; } = false;
	bool SouthNeighbor { get; set; } = false;
	bool WestNeighbor { get; set; } = false;

	public static Component Build(Vector3 position, Component roadPrefab)
	{
		return Instantiate(roadPrefab, position, Quaternion.identity, World.Instance.roadContainer);
	}

	void Start()
	{
	}

	public void Destroy()
	{
		DestroyImmediate(this.gameObject);
	}

	public void SetRoadNorth(bool isNorth)
	{
		NorthNeighbor = isNorth;
	}

	public void SetRoadEast(bool isEast)
	{
		//Debug.Log($"Set east to {isEast}");
		EastNeighbor = isEast;
	}

	public void SetRoadSouth(bool isSouth)
	{
		SouthNeighbor = isSouth;
	}

	public void SetRoadWest(bool isWest)
	{
		WestNeighbor = isWest;
	}

	public void UpdateRender()
	{
		road_N.SetActive(NorthNeighbor);
		road_E.SetActive(EastNeighbor);
		road_S.SetActive(SouthNeighbor);
		road_W.SetActive(WestNeighbor);

		int countNeighbors = 0;

		if (NorthNeighbor) countNeighbors++; //TODO public static int CountTrue(params bool[] args) { return args.Count(t => t); }
		if (EastNeighbor) countNeighbors++;
		if (SouthNeighbor) countNeighbors++;
		if (WestNeighbor) countNeighbors++;

		turn_NE.SetActive(false);
		turn_NW.SetActive(false);
		turn_SE.SetActive(false);
		turn_SW.SetActive(false);

		if (countNeighbors == 2)
		{
			if (NorthNeighbor && !SouthNeighbor)
			{
				turn_NE.SetActive(EastNeighbor);
				turn_NW.SetActive(WestNeighbor);
			}
			if (SouthNeighbor && !NorthNeighbor)
			{
				turn_SE.SetActive(EastNeighbor);
				turn_SW.SetActive(WestNeighbor);
			}
		}

		roundabout.SetActive(countNeighbors >= 3 || countNeighbors == 0);
	}

	void Update()
	{
		
	}
}
