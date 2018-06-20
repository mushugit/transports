using System.Collections;
using UnityEngine;

public class Cam : MonoBehaviour
{
	Vector3 defaultPositionRef;
	Vector3 defaultPosition;
	Quaternion defaultRotation;

	Vector3 lastValidPosition;

	public GameObject CamReferencePosition;

	public float moveSpeed = 30f;
	public float minZoom = 2f;
	public float maxZoom = 64f;
	public float defaultZoomPosition = 4f;

	public float edgeScrollSize = 20f;
	public float scrollSpeed = 5f;
	public float edgeLimit = 7f;

	public float camRadiusRatio = 1.5f;
	float camRadius;

	public float rotateSpeed = 35f;

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

		CamReferencePosition.transform.position = defaultPositionRef;
		transform.position = defaultPosition;
		transform.rotation = defaultRotation;
	}

	void Start()
	{
		camRadius = camRadiusRatio * Mathf.Max(World.width, World.height);
		//Debug.Log($"Cam radius {camRadius}");
		defaultPositionRef = CamReferencePosition.transform.position;


		defaultPositionRef.x = (World.width / defaultZoomPosition) + defaultCoord;
		defaultPositionRef.z = (World.height / defaultZoomPosition) + defaultCoord;

		CamReferencePosition.transform.position = defaultPositionRef;

		Vector3 mapReferenceScreenPoint;
		do
		{
			mapReferenceScreenPoint = Camera.main.WorldToScreenPoint(referencePoint);
			//Debug.Log(mapReferenceScreenPoint);
			transform.Translate(Vector3.back);
		} while (mapReferenceScreenPoint.y < 0);


		defaultPositionRef = CamReferencePosition.transform.position;
		defaultPosition = transform.position;
		defaultRotation = transform.rotation;
	}

	void Update()
	{
		var t = CamReferencePosition.transform;
		lastValidPosition =  t.position;
		var moveFactor = moveSpeed * Time.deltaTime;

		// Reset
		if (Input.GetButtonDown("ResetView"))
		{
			Center();
		}

		// Edge scrolling
		if (Input.GetButton("Edge scroll"))
		{
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

			if (scrollVector.magnitude > 0f)
				t.Translate(scrollVector, t);
		}

		// Directionnal move
		t.Translate(Input.GetAxis("Horizontal") * moveFactor, 0f, Input.GetAxis("Vertical") * moveFactor);

		// Rotation
		t.RotateAround(World.Center, Vector3.up, Input.GetAxis("Rotate") * rotateSpeed * Time.deltaTime);

		// Clamp position
		t.position = ClampCircle(lastValidPosition, t.position);

		// Zoom
		var positionBeforeZoom = transform.position;
		transform.Translate(new Vector3(0f, 0f, Input.GetAxis("Zoom") * Time.deltaTime));

		// Clamp camera after zoom
		if (transform.position.y < minZoom || transform.position.y > maxZoom)
			transform.position = positionBeforeZoom;

		lastValidPosition = t.position;

	}

	Vector3 ClampSquare(Vector3 originalPosition, Vector3 position)
	{
		if (position.z > position.x + World.height)
			return originalPosition;
		if (position.z < position.x - World.width)
			return originalPosition;
		if (position.z > -(position.x - edgeLimit) + World.width)
			return originalPosition;
		if (position.z < -(position.x - edgeLimit) - World.height)
			return originalPosition;
		return position;
	}

	Vector3 ClampCircle(Vector3 originalPosition, Vector3 position)
	{
		//Debug.Log($"Check {position} : {(position.x - World.width / 2f)}²+{(position.z - World.height / 2f)}² > {camRadius}²");
		if ((position.x - World.width / 2f) * (position.x - World.width / 2f) + (position.z - World.height / 2f) * (position.z - World.height / 2f) > camRadius * camRadius)
			return originalPosition;
		return position;
	}
}
