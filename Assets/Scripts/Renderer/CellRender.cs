using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CellRender : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Material defaultMaterialGrass;

	public Material red;
	public Material blue;
	public Material blueArrow;

	private bool isColored = false;

	private Coord point;
	private Construction construction;

	public void MakeRed()
	{
		isColored = true;
		GetComponent<Renderer>().material = red;
	}

	public void MakeBlue()
	{
		isColored = true;
		GetComponent<Renderer>().material = blue;
	}

	public void MakeBlue(int direction)
	{
		isColored = true;
		GetComponent<Renderer>().material = blueArrow;
	}

	public void Revert()
	{
		isColored = false;
		GetComponent<Renderer>().material = defaultMaterialGrass;
	}

	public void Initialize(Coord position, Construction building)
	{
		point = position;
		construction = building;
	}

	public void Build(Construction c)
	{
		construction = c;
	}

	public void Bulldoze()
	{
		construction = null;
	}

	public bool IsBuilt()
	{
		return construction != null;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (isColored)
			Revert();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (Builder.IsBuilding || Builder.IsDestroying)
		{
			if ((Builder.IsBuilding && IsBuilt()) || Builder.IsDestroying && !IsBuilt())
			{
				//Impossible
				MakeRed();
			}
			else
			{
				//Possible
				if (Builder.CanRotateBuilding)
					MakeBlue(0);
				else
					MakeBlue();
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (Builder.IsBuilding)
		{
			if (!IsBuilt())
			{
				if (Builder.TypeOfBuild == typeof(Road))
				{
					StartCoroutine(World.Instance.Roads(new List<Coord>() { point }));
				}
				if (Builder.TypeOfBuild == typeof(City))
				{
					World.Instance.BuildCity(point);
				}
			}
		}
		if (Builder.IsDestroying)
		{
			if (IsBuilt())
			{
				World.Instance.DestroyConstruction(point);
			}
		}
	}
}
