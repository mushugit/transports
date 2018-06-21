using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragger : MonoBehaviour
{
	Vector3 offset;

	public void HandleMouseDrag()
	{
		var position = Input.mousePosition;
		transform.position = position + offset;
	}

	public void HandleBeginDrag()
	{
		offset = transform.position - Input.mousePosition;
		offset.z = 0f;
	}
}
