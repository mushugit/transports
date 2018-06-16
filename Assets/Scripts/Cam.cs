using System.Collections;
using UnityEngine;

public class Cam : MonoBehaviour
{
	Vector3 defaultPosition;

	public GameObject CamReferencePosition;

	public float moveSpeed = 30f;
	public float minZoom = 2f;
	public float maxZoom = 64f;
	public float defaultZoomPosition = 4f;

	public float edgeScrollSize = 20f;
	public float scrollSpeed = 5f;
	public float edgeLimit = 7f;

	readonly float defaultCoord = -12f;
	readonly Vector3 referencePoint = new Vector3(0f, -2f, 0f);


	void Center()
	{
		/*var backward = transform.forward / -transform.forward.magnitude;
		var maxIteration = (int)maxZoom;
		var iteration = 0;
		for (iteration = 0; iteration < maxIteration; iteration++)
		{
			if (Physics.Raycast(transform.position, Vector3.zero - transform.position))
				break;
			//transform.Translate(backward);
			//transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y, transform.position.x), transform.rotation);
		}*/

		CamReferencePosition.transform.position = defaultPosition;

	}

	void Start()
	{
		defaultPosition = CamReferencePosition.transform.position;

		defaultPosition.x = (World.width / defaultZoomPosition) + defaultCoord;
		defaultPosition.z = (World.height / defaultZoomPosition) + defaultCoord;

		CamReferencePosition.transform.position = defaultPosition;

		Vector3 mapReferenceScreenPoint;
		do
		{
			mapReferenceScreenPoint = Camera.main.WorldToScreenPoint(referencePoint);
			Debug.Log(mapReferenceScreenPoint);
			transform.Translate(Vector3.back);
		} while (mapReferenceScreenPoint.y < 0);


		defaultPosition = CamReferencePosition.transform.position;
	}

	void Update()
	{
		var t = CamReferencePosition.transform;
		var originalPosition = t.position;
		var moveFactor = moveSpeed * Time.deltaTime;
		/*if (World.gameLoading)
            return;*/

		// Reset
		if (Input.GetButtonDown("ResetView"))
		{
			Center();
		}

		// Edge scrolling
		var mousePosition = Input.mousePosition;
		var scrollVector = new Vector3();
		if (mousePosition.x < edgeScrollSize)
			scrollVector.x = -scrollSpeed * Time.deltaTime;
		if (mousePosition.x > Screen.width - edgeScrollSize)
			scrollVector.x = scrollSpeed * Time.deltaTime;
		if (mousePosition.y < edgeScrollSize)
			scrollVector.z = -scrollSpeed * Time.deltaTime;
		if (mousePosition.y > Screen.height - edgeScrollSize)
			scrollVector.z = scrollSpeed * Time.deltaTime;

		if (scrollVector != null)
			t.Translate(scrollVector, t);
			


		// Directionnal move
		t.Translate(Input.GetAxis("Horizontal") * moveFactor, 0f, Input.GetAxis("Vertical") * moveFactor);

		
		// Clamp
		if (t.position.z > t.position.x + World.height)
			t.position = originalPosition;
		if (t.position.z < t.position.x - World.width)
			t.position = originalPosition;
		if (t.position.z > -(t.position.x - edgeLimit) + World.width)
			t.position = originalPosition;
		if (t.position.z < -(t.position.x - edgeLimit) - World.height)
			t.position = originalPosition;

		// Zoom
		var positionBeforeZoom = t.position;
		transform.Translate(new Vector3(0f, 0f, Input.GetAxis("Zoom") * Time.deltaTime));

		if (transform.position.y < minZoom || transform.position.y > maxZoom)
			transform.position = positionBeforeZoom;
			

	}
}
