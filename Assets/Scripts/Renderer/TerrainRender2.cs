using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TerrainRender2 : MonoBehaviour, IPointerDownHandler
{
	public GameObject hightlighter;

	public void OnPointerDown(PointerEventData eventData)
	{
		Debug.Log("Clic");
	}

	private void Update()
	{
		DisplayPosition();
	}

	void DisplayPosition()
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

			hightlighter.SetActive(true);
			hightlighter.transform.position = d;
		}
		else
			hightlighter.SetActive(false);
	}
}
