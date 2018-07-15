using UnityEngine;

public class VehiculeRender : MonoBehaviour {

	public GameObject Truck;
	public GameObject Cargo;

	private Vector3 finish;
	public float Speed;
	private float factor = 30;

	public RoadVehicule vehicule;

	public static Component Build(Component vehiculePrefab, Vector3 startingPosition, RoadVehicule vehicule)
	{
		var r = Instantiate(vehiculePrefab, startingPosition, Quaternion.identity);
		var rv = r.GetComponent<VehiculeRender>();
		rv.vehicule = vehicule;
		return r;
	}

	public void Init(Cell start, Cell finish, float speed, float position = 0)
	{
		//Debug.Log($"Truck init at {start}, target at {finish} (p={position} s={speed})");
		this.finish = new Vector3(finish.X, 0, finish.Y);
		this.Speed = speed;
        transform.position = Vector3.MoveTowards(new Vector3(start.X, 0, start.Y), this.finish, position);
    }

	public void InitColor(Color truckColor, Color cargoColor)
	{
		var tr = Truck.GetComponent<Renderer>();
		tr.material.color = truckColor;
		var cr = Cargo.GetComponent<Renderer>();
		cr.material.color = cargoColor;
	}
	
	void FixedUpdate () {
		//Debug.Log($"S={Speed} T={Time.fixedDeltaTime} M={Factor * Speed * Time.fixedDeltaTime}");
		transform.position = Vector3.MoveTowards(transform.position, finish, factor * Speed * Time.fixedDeltaTime);
	}

	private void OnValidate()
	{
		if(vehicule!=null)
			vehicule.Speed = Speed;
	}
}

