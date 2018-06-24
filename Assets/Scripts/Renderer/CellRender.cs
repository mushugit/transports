using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CellRender : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Material defaultMaterialGrass;

	public Material red;
	public Material blue;
	public Material blueArrow_2;
	public Material blueArrow_3;
	public Material blueArrow_0;
	public Material blueArrow_1;


	private bool isColored = false;

	private Coord point;

	private static int direction = 2;
	private bool isInCell;

	private void Update()
	{
		if (isInCell)
		{
			var left = Input.GetButtonDown("RotateBuild");
			var right = Input.GetButtonDown("RotateBuildNeg");

			if (left || right)
			{
				if (left) direction++;
				if (right) direction--;

				if (direction > 3) direction = 0;
				if (direction < 0) direction = 3;

				Builder.RotationDirection = direction;

				UpdateCell();
			}
		}
	}

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
		Material b = blue;
		switch (direction)
		{
			case 0:
				b = blueArrow_0;
				break;
			case 1:
				b = blueArrow_1;
				break;
			case 2:
				b = blueArrow_2;
				break;
			case 3:
				b = blueArrow_3;
				break;
		}
		GetComponent<Renderer>().material = b;
	}

	public void Revert()
	{
		isColored = false;
		GetComponent<Renderer>().material = defaultMaterialGrass;
	}

	public void Initialize(Coord position)
	{
		point = position;
	}

	public bool IsBuilt()
	{
		return World.Instance.Constructions[point.X, point.Y] != null;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isInCell = false;
		if (isColored)
			Revert();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		isInCell = true;
		UpdateCell();
	}

	private void UpdateCell()
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
					MakeBlue(direction);
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
					AudioManager.Player.Play("buildRoad");
				}
				if (Builder.TypeOfBuild == typeof(City))
				{
					World.Instance.BuildCity(point);
					AudioManager.Player.Play("buildCity");
				}
				if (Builder.TypeOfBuild == typeof(Depot))
				{
					World.Instance.BuildDepot(point);
					AudioManager.Player.Play("buildCity");
				}
			}
		}
		if (Builder.IsDestroying)
		{
			if (IsBuilt())
			{
				World.Instance.DestroyConstruction(point);
				AudioManager.Player.Play("destroy");
			}
		}
		UpdateCell();
	}
}
