using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CellRender : MonoBehaviour, IPointerClickHandler
{
	private GameObject highlighter;
	private Highlight highlight;
	private RotationalHighlight rotationalHighlight;
	public Material defaultMaterialGrass;

	public Material red;
	public Material blue;
	public Material[] blueArrow;

	private Cell currentPoint = null;

	private bool isColored = false;

	private static int direction = 2;

	private Transform parentTransform;

	public void SetScale(float width, float height)
	{
		var r = GetComponent<Renderer>();
		r.material.mainTextureScale = new Vector2(width, height);
	}

	private void Awake()
	{
		highlighter = GameObject.FindGameObjectWithTag("Hightlighter");
		highlight = highlighter.GetComponent<Highlight>();
		rotationalHighlight = highlighter.GetComponentInChildren<RotationalHighlight>(true);
		parentTransform = GetComponentInParent<Transform>();
	}

	private void Start()
	{
		Revert();
	}

	bool Highlight()
	{
		if (Builder.IsBuilding || Builder.IsDestroying)
		{
			var c = Camera.main;
			var r = c.ScreenPointToRay(Input.mousePosition);
			var h = new RaycastHit();
			var m = 1 << 31;
			if (Physics.Raycast(r, out h, 1000, m))
			{
				var p = h.point;
				var d = p;
				
				d.x = Mathf.Round(p.x - 0.5f);
				d.y = 1;
				d.z = Mathf.Round(p.z - 0.5f);

				var x = (int) d.x;
				var y = (int) d.z;

				var constr = World.Instance.Constructions[x, y];
				currentPoint = new Cell(x, y, constr);
				//Debug.Log($"Highlighting {currentPoint}");

				highlighter.SetActive(true);
				highlighter.transform.position = d;
				return true;
			}
			else
			{
				currentPoint = null;
				highlighter.SetActive(false);
				return false;
			}
		}
		else
		{
			currentPoint = null;
			highlighter.SetActive(false);
			return false;
		}
	}

	private void RotateHighlight(bool positive = true)
	{
		rotationalHighlight.Rotate(positive);
	}

	private void Update()
	{
		if (Highlight())
		{
			var left = Input.GetButtonDown("RotateBuild");
			var right = Input.GetButtonDown("RotateBuildNeg");

			if (left || right)
			{
				RotateHighlight(right);
				if (left) direction++;
				if (right) direction--;

				if (direction > 3) direction = 0;
				if (direction < 0) direction = 3;

				Builder.RotationDirection = direction;
			}
			UpdateCell(currentPoint);
		}
	}

	private void HighlightRed()
	{
		highlighter.SetActive(true);
		highlight.KO();
	}

	private void HighlightBlue(int direction)
	{
		highlighter.SetActive(true);
		highlight.OkDirectional();
	}

	private void HighlightBlue()
	{
		highlighter.SetActive(true);
		highlight.Ok();
	}

	public void Revert()
	{
		isColored = false;
		highlighter.SetActive(false);
	}

	public bool IsBuilt(Cell point)
	{
		return World.Instance.Constructions[point.X, point.Y] != null;
	}

	private void UpdateCell(Cell point)
	{
		if (Builder.IsBuilding || Builder.IsDestroying)
		{
			if ((Builder.IsBuilding && IsBuilt(point)) || Builder.IsDestroying && !IsBuilt(point))
			{
				//Impossible
				HighlightRed();
			}
			else
			{
				//Possible
				if (Builder.CanRotateBuilding)
					HighlightBlue(direction);
				else
					HighlightBlue();
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (Builder.IsBuilding)
		{
			if (!IsBuilt(currentPoint))
			{
				if (Builder.TypeOfBuild == typeof(Road))
				{
					World.Instance.BuildRoad(currentPoint);
				}
				if (Builder.TypeOfBuild == typeof(City))
				{
					World.Instance.BuildCity(currentPoint);
				}
				if (Builder.TypeOfBuild == typeof(Depot))
				{
					World.Instance.BuildDepot(currentPoint);
				}
			}
		}
		if (Builder.IsDestroying)
		{
			if (IsBuilt(currentPoint))
			{
				World.Instance.DestroyConstruction(currentPoint);
				AudioManager.Player.Play("destroy");
			}
		}
		UpdateCell(currentPoint);
	}
}
