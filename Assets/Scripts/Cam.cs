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
	public float maxZoom = 16384;
	public float zoomSpeed = 1.2f;
	public float defaultZoomPosition = 10f;

	public float edgeScrollSize = 20f;
	public float scrollSpeed = 5f;
	public float edgeLimit = 7f;

	public float camRadiusRatio = 1.5f;
	float camRadius;

	public float rotateSpeed = 35f;

	readonly float defaultCoord = -7f;
	readonly Vector3 referencePoint = new Vector3(0f, -2f, 0f);


	public void Center()
	{
		camRadius = camRadiusRatio * Mathf.Max(World.width, World.height);
		//Debug.Log($"Cam radius {camRadius}");
		defaultPositionRef = new Vector3(
			(World.width / 2) + defaultCoord,
			defaultZoomPosition,
			(World.height / 2) + defaultCoord
		);

		CamReferencePosition.transform.position = defaultPositionRef;

		var idealZoom = Mathf.Max(World.width, World.height);
		var zoom = 1f;
		do
		{
			transform.Translate(Vector3.back);
			zoom++;
		} while (zoom < idealZoom && transform.position.y + 1 < maxZoom);

		/*
		var c = Camera.main;
		var r = c.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
		var h = new RaycastHit();
		
		var m = 1 << 31;

		if (Physics.Raycast(r, out h, 1000, m))
		{
			var tan = (Mathf.Sqrt(World.width * World.width + World.height * World.height) / 2) / h.distance;
			c.fieldOfView = Mathf.Clamp(Mathf.Atan(tan) * Mathf.Rad2Deg, 1, 90);
		}*/

		defaultPositionRef = CamReferencePosition.transform.position;
		defaultPosition = transform.position;
		defaultRotation = transform.rotation;
	}

	private void ResetView()
	{
		CamReferencePosition.transform.position = defaultPositionRef;
		transform.position = defaultPosition;
		transform.rotation = defaultRotation;
	}

	void Start()
	{
		Center();
	}

	void Update()
	{
		var t = CamReferencePosition.transform;
		lastValidPosition = t.position;
		var moveFactor = moveSpeed * Time.deltaTime;

		// Reset
		if (Input.GetButton("RotateBuild") && Input.GetButton("Modifier"))
		{
			ResetView();
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
		//Debug.Log($"Center={World.Center}");
		t.RotateAround(World.Center, Vector3.up, Input.GetAxis("Rotate") * rotateSpeed * Time.deltaTime);

		// Clamp position
		t.position = ClampCircle(lastValidPosition, t.position);

		lastValidPosition = t.position;

	}

	private void FixedUpdate()
	{
		// Zoom
		var positionBeforeZoom = transform.position;
		transform.Translate(new Vector3(0f, 0f, Input.GetAxis("Zoom") * zoomSpeed * Time.deltaTime));

		// Clamp camera after zoom
		if (transform.position.y < minZoom || transform.position.y > maxZoom)
			transform.position = positionBeforeZoom;
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
