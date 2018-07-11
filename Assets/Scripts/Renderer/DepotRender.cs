using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DepotRender : MonoBehaviour
{
	public static Component Build(Vector3 position, Component depotPrefab, int direction)
	{
		var d = Instantiate(depotPrefab, position, Quaternion.identity, World.Instance.depotContainer);

		float angle = 180 - direction * -90;

		var center = d.transform.position;
		center.x += 0.5f;
		center.z += 0.5f;
		d.transform.RotateAround(center,Vector3.up, angle);

		return d;
	}

	public void Destroy()
	{
		DestroyImmediate(this.gameObject);
	}
}

